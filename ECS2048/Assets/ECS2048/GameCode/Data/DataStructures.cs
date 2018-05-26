﻿using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public struct FloorData
{
    public int Length;
    public ComponentDataArray<FloorMarker> Marker;
}

public struct TextData
{
    public int Length;
    [ReadOnly] public ComponentDataArray<TextUI> Text;
    [ReadOnly] public ComponentDataArray<Block> Block;
    [ReadOnly] public ComponentDataArray<Position> Position;
}

public struct PlayerData
{
    [ReadOnly] public ComponentDataArray<PlayerMarker> Marker;
    public ComponentDataArray<Input> Input;
}

public struct BlockData
{
    public int Length;
    public ComponentDataArray<Block> Block;
}
