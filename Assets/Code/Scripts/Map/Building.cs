using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DoorSide
{
    Top,
    Left,
    Right,
    Bottom
}

public class Building : BaseNode
{
    public BuildingMapNodeSO Settings;

    public int TotalTileSize;

    private void Awake()
    {
        TotalTileSize = Settings.WidthOnBlock + Settings.HeightOnBlock;
    }
}
