using UnityEngine;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{

    public bool displayGridGizmos;
    public LayerMask unwalkableMask;
    public Vector2 gridWorldSize;
    public float nodeRadius;
    public TerrainType[] walkableRegions;
    public float checkRadiusModifier = 2;
    public float terrainOffset = 3;
    private LayerMask _walkableMask;
    private readonly Dictionary<int, int> _walkableRegionsDictionary = new();

    public Node[,] grid;

    protected float NodeDiameter;
    protected int GridSizeX, GridSizeY;

    void Awake()
    {
        NodeDiameter = nodeRadius * 2;
        GridSizeX = Mathf.RoundToInt(gridWorldSize.x / NodeDiameter);
        GridSizeY = Mathf.RoundToInt(gridWorldSize.y / NodeDiameter);


        // Note: Layers are stored in a 32 bit int
        foreach (var region in walkableRegions)
        {
            _walkableMask.value |= region.terrainMask.value;
            _walkableRegionsDictionary.Add((int)Mathf.Log((float)region.terrainMask.value, 2f), region.terrainPenalty);
        }

        CreateGrid();
    }

    public int MaxSize => GridSizeX * GridSizeY;

    void CreateGrid()
    {
        grid = new Node[GridSizeX, GridSizeY];
        var worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

        for (var x = 0; x < GridSizeX; x++)
        {
            for (var y = 0; y < GridSizeY; y++)
            {
                var worldPoint = worldBottomLeft + Vector3.right * (x * NodeDiameter + nodeRadius) + Vector3.forward * (y * NodeDiameter + nodeRadius);
                var walkable = !(Physics.CheckSphere(worldPoint, nodeRadius * checkRadiusModifier, unwalkableMask));

                var movementPenalty = 0;
                float height = 0;
                // raycast
                if (walkable)
                {
                    var ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
                    if (Physics.Raycast(ray, out var hit, 100, _walkableMask))
                    {
                        // Determine the movement penalty of the terrain type
                        _walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);

                        // Get the height of a block
                        worldPoint.y = height = (hit.transform.position.y + hit.collider.bounds.extents.y);
                    }
                }

                var walkableEnum = Walkable.Passable;
                if (!walkable)
                    walkableEnum = Walkable.Impassable;

                //worldPoint.y = Mathf.Clamp(worldPoint.y, 0.1f, Mathf.Infinity);
                grid[x, y] = new Node(walkableEnum, worldPoint, x, y, height, movementPenalty);
                DrawGrid(grid[x, y]);
            }
        }
    }

    /// <summary>
    /// Find all nodes around a given node.
    /// </summary>
    /// <param name="node"> node whose neighbors are to be found</param>
    /// <returns></returns>
    public List<Node> GetNeighbours(Node node)
    {
        var neighbours = new List<Node>();

        for (var x = -1; x <= 1; x++)
        {
            for (var y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                var checkX = node.GridX + x;
                var checkY = node.GridY + y;

                if (checkX >= 0 && checkX < GridSizeX && checkY >= 0 && checkY < GridSizeY)
                {
                    neighbours.Add(grid[checkX, checkY]);
                }
            }
        }

        return neighbours;
    }

    /// <summary>
    /// Convert workspace to grid space cartesian coordinates and return node at that point.
    /// </summary>
    /// <param name="worldPosition"> Actual position of node in 3D space</param>
    /// <returns> Node intersecting worldPosition</returns>
    public Node NodeFromWorldPoint(Vector3 worldPosition)
    {
        var percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
        var percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
        percentX = Mathf.Clamp01(percentX);
        percentY = Mathf.Clamp01(percentY);

        var x = Mathf.RoundToInt((GridSizeX - 1) * percentX);
        var y = Mathf.RoundToInt((GridSizeY - 1) * percentY);
        return grid[x, y];
    }

    public virtual void DrawGrid(Node node) { }

    void OnDrawGizmos()
    {
        // TODO: Does not seem to work in newer versions of Unity
        Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
        
        if (grid == null || !displayGridGizmos) return;
        
        foreach (var n in grid)
        {
            Gizmos.color = (n.Walkable == Walkable.Passable) ? Color.white : Color.red;
            Gizmos.DrawCube(n.WorldPosition, Vector3.one * (NodeDiameter - .1f));
        }
    }

    [System.Serializable]
    public class TerrainType
    {
        public LayerMask terrainMask;
        public int terrainPenalty;
    }

}
