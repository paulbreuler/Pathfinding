using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour
{


    public Transform target;
    public float movementSpeed = 20;

    /// <summary>
    /// How close to get to waypoint before moving towards next. This fixes rigidbody movement bug.
    /// Issue seen when close to waypoint rigidbody cannot get to exact position.
    /// </summary>
    public float distanceToWaypoint = 1;
    Vector3[] path;
    int targetIndex;

    void Start()
    {
        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = newPath;
            targetIndex = 0;

            // Stop coroutine if it is already running.
            StopCoroutine("FollowPath");
            StartCoroutine("FollowPath");
        }
    }

    IEnumerator FollowPath()
    {
        Vector3 currentWaypoint = path[0];
        while (true)
        {
            
            if (Vector3.Distance(transform.position, currentWaypoint) < distanceToWaypoint)
            {
                targetIndex++;

                // If we are done with path.
                if (targetIndex >= path.Length)
                {
                    yield break;
                }
                currentWaypoint = path[targetIndex];
            }

            // Occurs each frame
            //transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, movementSpeed * Time.deltaTime);
            Vector3 direction = currentWaypoint - transform.position;
            transform.Translate(direction.normalized * movementSpeed * Time.deltaTime, Space.World);
            transform.LookAt(currentWaypoint);
            yield return null;

        }
    }

    public void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = targetIndex; i < path.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(path[i], Vector3.one);

                if (i == targetIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i]);
                }
                else
                {
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }
    }
}

