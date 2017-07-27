using UnityEngine;
using System.Collections;

public enum Walkable { Blocked, Passable, Impassable };

public class Node : IHeapItem<Node>
{

    private Walkable m_walkable;
    public Walkable walkable
    {
        get
        {
            return m_walkable;
        }
        set
        {
            m_walkable = value;
            if(NodeMesh != null)
            NodeMesh.GetComponent<GridColor>().UpdateColor(value);
        }
        
    }
    public Vector3 worldPosition;
    public int gridX;
    public int gridY;
    public float height;
    public int movementPenalty;

    public int gCost;
    public int hCost;
    public Node parent;

    public GameObject NodeMesh;

    int heapIndex;

    public Node(Walkable _walkable, Vector3 _worldPos, int _gridX, int _gridY,float _height, int _penalty)
    {
        walkable = _walkable;
        worldPosition = _worldPos;
        gridX = _gridX;
        gridY = _gridY;
        height = _height;
        movementPenalty = _penalty;
    }

    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }

    public int HeapIndex
    {
        get
        {
            return heapIndex;
        }
        set
        {
            heapIndex = value;
        }
    }

    public int CompareTo(Node nodeToCompare)
    {
        // if F cost is lower
        int compare = fCost.CompareTo(nodeToCompare.fCost);

        // We and lower H cost if F cost is the same.
        if (compare == 0)
        {
            compare = hCost.CompareTo(nodeToCompare.hCost);
        }
        return -compare;
    }

    public override bool Equals(object obj)
    {
        return worldPosition == ((Node)obj).worldPosition;
    }
}
