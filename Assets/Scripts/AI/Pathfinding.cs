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

        var waypoints = new Vector3[0];
        var pathSuccess = false;

        var startNode = grid.NodeFromWorldPoint(startPos);
        var targetNode = grid.NodeFromWorldPoint(targetPos);

        
        // if starting node is not reachable try to move to an adjacent node. 
        if (startNode.Walkable != Walkable.Passable)
        {
            var neighbors = grid.GetNeighbours(startNode);
            foreach(var n in neighbors)
            {
                if (n.Walkable == Walkable.Passable) { 
                    startNode = n;
                    break;
                 }
                
            }
        }

        // Only execute if both source and target are reachable
        if (startNode.Walkable == Walkable.Passable && targetNode.Walkable == Walkable.Passable)
        {
            // Nodes to be explored. Lowest cost first
            var openSet = new Heap<Node>(grid.MaxSize);
            // Explored nodes
            var closedSet = new HashSet<Node>();

            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                var currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    pathSuccess = true;
                    break;
                }

                foreach (var neighbour in grid.GetNeighbours(currentNode))
                {
                    if (neighbour.Walkable != Walkable.Passable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    var newMovementCostToNeighbour = currentNode.GCost + GetDistance(currentNode, neighbour)  + neighbour.MovementPenalty;
                    if (newMovementCostToNeighbour < neighbour.GCost || !openSet.Contains(neighbour))
                    {
                        neighbour.GCost = newMovementCostToNeighbour;
                        neighbour.HCost = GetDistance(neighbour, targetNode);
                        neighbour.Parent = currentNode;

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
        var path = new List<Node>();
        var currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.Parent;
        }

        // Reduce path complexity
        var waypoints = SimplifyPath(path);
        var smoothedWaypoints = BezierPath(waypoints, 1.0f);
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
        var waypoints = new List<Vector3>();
        var directionOld = Vector2.zero;

        for (var i = 1; i < path.Count; i++)
        {
            // Direction between two nodes
            var directionNew = new Vector2(path[i - 1].GridX - path[i].GridX, path[i - 1].GridY - path[i].GridY);

            // If path has changed direction
            if (directionNew != directionOld)
            {
                // Does not adapt to height of character
                // Maybe put empty game object at ground level
                waypoints.Add(path[i].WorldPosition + Vector3.up );
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
        var waypointsLength = 0;
        var curvedLength = 0;

        if (smoothness < 1.0f) smoothness = 1.0f;

        waypointsLength = waypoints.Length;

        curvedLength = (waypointsLength * Mathf.RoundToInt(smoothness)) - 1;
        smoothedWaypoints = new List<Vector3>(curvedLength);

        var t = 0.0f;
        for (var pointInTimeOnCurve = 0; pointInTimeOnCurve < curvedLength + 1; pointInTimeOnCurve++)
        {
            t = Mathf.InverseLerp(0, curvedLength, pointInTimeOnCurve);

            points = new List<Vector3>(waypoints);

            for (var j = waypointsLength - 1; j > 0; j--)
            {
                for (var i = 0; i < j; i++)
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
        var dstX = Mathf.Abs(nodeA.GridX - nodeB.GridX);
        var dstY = Mathf.Abs(nodeA.GridY - nodeB.GridY);

        if (dstX > dstY)
            return 14 * dstY + 10 * (dstX - dstY);
        return 14 * dstX + 10 * (dstY - dstX);
    }


}
