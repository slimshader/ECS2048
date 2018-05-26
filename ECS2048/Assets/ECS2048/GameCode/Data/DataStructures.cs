﻿using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public struct TextData
{
    public int Length;
    [ReadOnly] public ComponentDataArray<TextUI> Text;
    [ReadOnly] public ComponentDataArray<Block> Block;
    [ReadOnly] public ComponentDataArray<Position> Position;
}

public struct PlayerData
{
    [ReadOnly] public ComponentDataArray<Player> Marker;
    public ComponentDataArray<Input> Input;
}

public struct BlockData
{
    public int Length;
    public ComponentDataArray<Block> Block;
}

public struct BlockEntityData
{
    public int Length;
    public ComponentDataArray<Block> Block;
    [ReadOnly] public EntityArray Entity;
}

public struct ScoreData
{
    [ReadOnly] public ComponentDataArray<Player> Marker;
    public ComponentDataArray<ScoreHolder> ScoreHolder;
}
