using UnityEngine;
using System.Collections.Generic;
using System;

public class PathRequestManager : MonoBehaviour
{

    private readonly Queue<PathRequest> _pathRequestQueue = new();
    private PathRequest _currentPathRequest;

    private static PathRequestManager _instance;
    private Pathfinding _pathfinding;

    private bool _isProcessingPath;

    void Awake()
    {
        _instance = this;
        _pathfinding = GetComponent<Pathfinding>();
    }

    public static void RequestPath(Vector3 pathStart, Vector3 pathEnd, Action<Vector3[], bool> callback)
    {
        var newRequest = new PathRequest(pathStart, pathEnd, callback);
        _instance._pathRequestQueue.Enqueue(newRequest);
        _instance.TryProcessNext();
    }

    /// <summary>
    /// Determine if a path is currently being processed. If not, start the next path.
    /// </summary>
    void TryProcessNext()
    {
        if (!_isProcessingPath && _pathRequestQueue.Count > 0)
        {
            _currentPathRequest = _pathRequestQueue.Dequeue();
            _isProcessingPath = true;
            _pathfinding.StartFindPath(_currentPathRequest.PathStart, _currentPathRequest.PathEnd);
        }
    }

    /// <summary>
    /// Path has been calculated.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="success"></param>
    public void FinishedProcessingPath(Vector3[] path, bool success)
    {
        _currentPathRequest.Callback(path, success);
        _isProcessingPath = false;
        TryProcessNext();
    }

    struct PathRequest
    {
        public readonly Vector3 PathStart;
        public readonly Vector3 PathEnd;
        public readonly Action<Vector3[], bool> Callback;

        public PathRequest(Vector3 start, Vector3 end, Action<Vector3[], bool> callback)
        {
            PathStart = start;
            PathEnd = end;
            Callback = callback;
        }

    }
}
