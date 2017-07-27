using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Pathfinding : MonoBehaviour
{

    PathRequestManager requestManager;
    Grid grid;

    void Awake()
    {
        requestManager = GetComponent<PathRequestManager>();
        grid = GetComponent<Grid>();
    }


    public void StartFindPath(Vector3 startPos, Vector3 targetPos)
    {
        StartCoroutine(FindPath(startPos, targetPos));
    }

    /// <summary>
    /// Find walkable nodes between startPos and targetPos to form a complete path between the two nodes if they are both in walkable regions.
    /// </summary>
    /// <param name="startPos"> Start Position</param>
    /// <param name="targetPos"> Target Position</param>
    /// <returns></returns>
    IEnumerator FindPath(Vector3 startPos, Vector3 targetPos)
    {

        Vector3[] waypoints = new Vector3[0];
        bool pathSuccess = false;

        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node targetNode = grid.NodeFromWorldPoint(targetPos);

        
        // if starting node is not reachable try to move to an adjacent node. 
        if (startNode.walkable != Walkable.Passable)
        {
            List<Node> neighbors = grid.GetNeighbours(startNode);
            foreach(Node n in neighbors)
            {
                if (n.walkable == Walkable.Passable) { 
                    startNode = n;
                    break;
                 }
                
            }
        }

        // Only execute if both source and target are reachable
        if (startNode.walkable == Walkable.Passable && targetNode.walkable == Walkable.Passable)
        {
            // Nodes to be explored. Lowest cost first
            Heap<Node> openSet = new Heap<Node>(grid.MaxSize);
            // Explored nodes
            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    pathSuccess = true;
                    break;
                }

                foreach (Node neighbour in grid.GetNeighbours(currentNode))
                {
                    if (neighbour.walkable != Walkable.Passable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour)  + neighbour.movementPenalty;
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                    }
                }
            }
        }
        yield return null;
        if (pathSuccess)
        {
            waypoints = RetracePath(startNode, targetNode);
        }
        requestManager.FinishedProcessingPath(waypoints, pathSuccess);

    }

    /// <summary>
    /// Find forward path
    /// </summary>
    /// <param name="startNode"></param>
    /// <param name="endNode"></param>
    /// <returns> Walkable path</returns>
    Vector3[] RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        // Reduce path complexity
        Vector3[] waypoints = SimplifyPath(path);
        Vector3[] smoothedWaypoints = BezierPath(waypoints, 1.0f);
        Array.Reverse(smoothedWaypoints);
        return smoothedWaypoints;

    }


    /// <summary>
    /// Reduce waypoint array. Remove nodes that are in the same direction as the previous node
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    Vector3[] SimplifyPath(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        Vector2 directionOld = Vector2.zero;

        for (int i = 1; i < path.Count; i++)
        {
            // Direction between two nodes
            Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);

            // If path has changed direction
            if (directionNew != directionOld)
            {
                // Does not adapt to height of character
                // Maybe put empty game object at ground level
                waypoints.Add(path[i].worldPosition + Vector3.up );
            }
            directionOld = directionNew;
        }
        return waypoints.ToArray();
    }


    /// <summary>
    /// Smooth path using Bezier Spline algorithm.
    /// Reference: http://answers.unity3d.com/questions/392606/line-drawing-how-can-i-interpolate-between-points.html
    /// Reference: http://ibiblio.org/e-notes/Splines/Bezier.htm
    /// Reference: http://catlikecoding.com/unity/tutorials/curves-and-splines/
    /// </summary>
    /// <returns> Interpolated path </returns>
    Vector3[] BezierPath(Vector3[] waypoints, float smoothness)
    {
        if (waypoints.Length <= 1)
            return waypoints;

        List<Vector3> points;
        List<Vector3> smoothedWaypoints;
        int waypointsLength = 0;
        int curvedLength = 0;

        if (smoothness < 1.0f) smoothness = 1.0f;

        waypointsLength = waypoints.Length;

        curvedLength = (waypointsLength * Mathf.RoundToInt(smoothness)) - 1;
        smoothedWaypoints = new List<Vector3>(curvedLength);

        float t = 0.0f;
        for (int pointInTimeOnCurve = 0; pointInTimeOnCurve < curvedLength + 1; pointInTimeOnCurve++)
        {
            t = Mathf.InverseLerp(0, curvedLength, pointInTimeOnCurve);

            points = new List<Vector3>(waypoints);

            for (int j = waypointsLength - 1; j > 0; j--)
            {
                for (int i = 0; i < j; i++)
                {
                    points[i] = (1 - t) * points[i] + t * points[i + 1];
                }
            }

            smoothedWaypoints.Add(points[0]);
        }

        return (smoothedWaypoints.ToArray());
    }

    /// <summary>
    /// Get distance from Node A to Node B
    /// </summary>
    /// <param name="nodeA"></param>
    /// <param name="nodeB"></param>
    /// <returns></returns>
    int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }


}
