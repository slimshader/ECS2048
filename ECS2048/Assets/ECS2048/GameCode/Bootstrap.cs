﻿using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;
using TMPro;

namespace TP.ECS2048
{
    public class Bootstrap : MonoBehaviour
    {
        public static GameSettings GameSettings { get; private set; }

        public static EntityArchetype PlayerArchetype { get; private set; }
        public static EntityArchetype BlockArchetype { get; private set; }
        public static EntityArchetype FloorArchetype { get; private set; }

        public static MeshInstanceRenderer BlockLook { get; private set; }
        public static MeshInstanceRenderer FloorLook { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeBeforeScene()
        {
            World.Active.SetBehavioursActive(false);
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();
            SetArchetypes(entityManager);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void InitializeAfterScene()
        {
            GameSettings = FindObjectOfType<GameSettings>()?.GetComponent<GameSettings>();
            if (!GameSettings)
                return;

            BlockLook = Utils.GetLookFromPrototype("BlockLook");
            FloorLook = Utils.GetLookFromPrototype("FloorLook");

            World.Active.GetExistingManager<UISystem>().Initialize(NewGame);
            World.Active.GetExistingManager<SpawnSystem>().Initialize(GameSettings.GridSize.x * GameSettings.GridSize.y);
            NewGame(); // dummy
        }

        public static void NewGame()
        {
            var entityManager = World.Active.GetOrCreateManager<EntityManager>();

            CreatePlayer(entityManager);
            CreateTextMeshPros();
            CreatePlane(entityManager);
            World.Active.SetBehavioursActive(true);
            Time.timeScale = 1;
            Delayed(); // it's hack
        }

        public static async void Delayed()
        {
            await System.Threading.Tasks.Task.Delay(25);
            World.Active.GetExistingManager<MoveSystem>().Initialize();
        }

        public static void GameOver()
        {
            World.Active.SetBehavioursActive(false);
            GameSettings.GameOverCanvas.SetActive(true);
            GameSettings.HUDCanvas.SetActive(false);
        }

        public static void CreateBlock(EntityManager em, float3 pos, int2 posIndex, int baseValue, Entity block = default(Entity))
        {
            if (block == default(Entity))
                block = em.CreateEntity(BlockArchetype);

            em.SetComponentData(block, new Position { Value = pos });
            em.SetComponentData(block, new Heading { Value = new float3(0.0f, 1.0f, 0) });
            em.SetComponentData(block, new TextUI { Index = GetFreeTextMeshIndex() });
            em.SetComponentData(block, new Block { PosIndex = posIndex, Value = baseValue });
            em.AddSharedComponentData(block, BlockLook);
        }

        private static void CreatePlayer(EntityManager em)
        {
            var player = em.CreateEntity(PlayerArchetype);
            em.SetComponentData(player, new ScoreHolder() { Score = 0 });
        }

        private static void CreatePlane(EntityManager em)
        {
            Heading defaultHeading = new Heading() { Value = new float3(0.0f, 1.0f, 0) };
            NativeArray<Entity> newEntities = new NativeArray<Entity>(GameSettings.GridSize.x * GameSettings.GridSize.y, Allocator.Temp);
            em.CreateEntity(FloorArchetype, newEntities);
            int2 randIndex = GetRandomPosOnGrid();
            int2 randIndex2 = GetRandomPosOnGrid();

            int index = 0;
            for (int x = 0; x < GameSettings.GridSize.x; x++)
            {
                var gapX = (x * GameSettings.GridGap) - (GameSettings.GridSize.x / 2);
                for (int y = 0; y < GameSettings.GridSize.y; y++)
                {
                    var entity = newEntities[index];
                    var gapY = y * GameSettings.GridGap - (GameSettings.GridSize.y / 2);
                    float3 pos = new float3(x + gapX, y + gapY, 0);
                    int2 gridIndex = new int2(x, y);
                    em.SetComponentData(entity, new Position { Value = pos });
                    em.SetComponentData(entity, default(FloorMarker));
                    em.SetComponentData(entity, defaultHeading);
                    em.AddSharedComponentData(entity, FloorLook);
                    CreateBlock(em, pos, gridIndex, ShouldMakeStartBlock(gridIndex, randIndex, randIndex2) ? 2 : 0);
                    index++;
                }
            }
            newEntities.Dispose();
        }

        private static bool ShouldMakeStartBlock(int2 actualIndex, params int2[] randomIndexes)
        {
            int length = randomIndexes.Length;
            for (int i = 0; i < length; i++)
            {
                if (actualIndex.Equals(randomIndexes[i]))
                    return true;
            }
            return false;
        }

        private static void CreateTextMeshPros()
        {
            int length = GameSettings.GridSize.x * GameSettings.GridSize.y;
            for (int i = 0; i < length; i++)
            {
                GameSettings.BlockTexts.Add(CreateTextMeshPro());
            }
        }

        private static int GetFreeTextMeshIndex()
        {
            int length = GameSettings.BlockTexts.Count;
            for (int i = 0; i < length; i++)
            {
                if (!GameSettings.BlockTexts[i].isActiveAndEnabled)
                {
                    GameSettings.BlockTexts[i].gameObject.SetActive(true);
                    return i;
                }
            }
            return -1;
        }

        private static TextMeshPro CreateTextMeshPro()
        {
            GameObject newTextObj = new GameObject("Block Text", typeof(TextMeshPro)) { isStatic = true };
            TextMeshPro textMesh = newTextObj.GetComponent<TextMeshPro>();
            textMesh.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 1);
            textMesh.fontSize = 5;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.margin = Vector4.zero;
            newTextObj.SetActive(false);
            return textMesh;
        }

        private static int2 GetRandomPosOnGrid()
        {
            int x = Random.Range(0, GameSettings.GridSize.x);
            int y = Random.Range(0, GameSettings.GridSize.y);
            return new int2(x, y);
        }

        private static void SetArchetypes(EntityManager entityManager)
        {
            var scoreHolder = ComponentType.Create<ScoreHolder>();
            var textTag = ComponentType.Create<TextUI>();
            var input = ComponentType.Create<Input>();

            var playerTag = ComponentType.Create<PlayerMarker>();
            var blockTag = ComponentType.Create<Block>();
            var floorTag = ComponentType.Create<FloorMarker>();

            var heading = ComponentType.Create<Heading>();
            var position = ComponentType.Create<Position>();
            var transformMatrix = ComponentType.Create<TransformMatrix>();

            PlayerArchetype = entityManager.CreateArchetype(
                playerTag, scoreHolder, input
                );

            BlockArchetype = entityManager.CreateArchetype(
                position, transformMatrix, heading, input, blockTag, textTag
                );

            FloorArchetype = entityManager.CreateArchetype(
                position, transformMatrix, heading, floorTag
                );
        }

    }
}
