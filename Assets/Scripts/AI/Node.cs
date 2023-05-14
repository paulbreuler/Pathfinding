using UnityEngine;

public enum Walkable { Blocked, Passable, Impassable };

public class Node : IHeapItem<Node>
{

    private Walkable _mWalkable;
    public Walkable Walkable
    {
        get
        {
            return _mWalkable;
        }
        set
        {
            _mWalkable = value;
            if(NodeMesh != null)
                NodeMesh.GetComponent<GridColor>().UpdateColor(value);
        }
        
    }
    public readonly Vector3 WorldPosition;
    public readonly int GridX;
    public readonly int GridY;
    public float Height;
    public int MovementPenalty;

    public int GCost;
    public int HCost;
    public Node Parent;

    public GameObject NodeMesh;

    private int _heapIndex;

    public Node(Walkable walkable, Vector3 worldPos, int gridX, int gridY,float height, int penalty)
    {
        Walkable = walkable;
        WorldPosition = worldPos;
        GridX = gridX;
        GridY = gridY;
        Height = height;
        MovementPenalty = penalty;
    }

    public int FCost
    {
        get
        {
            return GCost + HCost;
        }
    }

    public int HeapIndex
    {
        get
        {
            return _heapIndex;
        }
        set
        {
            _heapIndex = value;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        // if F cost is lower
        var compare = FCost.CompareTo(nodeToCompare.FCost);

        // We and lower H cost if F cost is the same.
        if (compare == 0)
        {
            compare = HCost.CompareTo(nodeToCompare.HCost);
        }
        return -compare;
    }

    public override bool Equals(object obj) => WorldPosition == ((Node)obj).WorldPosition;
    
    public override int GetHashCode()
    {
        return WorldPosition.GetHashCode();
    }
}
