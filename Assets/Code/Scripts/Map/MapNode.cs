using UnityEngine;
using System.Collections.Generic;

public class MapNode : BaseNode
{
    public Vector2Int Pivot { get; private set; }

    public List<MapNode> Neighbors { get; private set; }
    
}