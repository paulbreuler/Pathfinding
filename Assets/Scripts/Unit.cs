using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour
{

    public bool drawGizmos = false;
    public float gravity = 9.8f;
    public Transform target;
    public float movementSpeed = 20;

    protected float vSpeed = 0;
    /// <summary>
    /// How close to get to waypoint before moving towards next. This fixes rigidbody movement bug.
    /// Issue seen when close to waypoint rigidbody cannot get to exact position.
    /// </summary>
    public float distanceToWaypoint = 1;
    protected Vector3[] path;
    protected int targetIndex;

    public virtual void Start()
    {
        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
    }

    public virtual void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = newPath;
            targetIndex = 0;

            // Stop coroutine if it is already running.
            StopCoroutine(FollowPath());
            StartCoroutine(FollowPath());
        }
    }

    public virtual IEnumerator FollowPath()
    {
        Debug.Log("Base class Here");
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
            //transform.Translate(direction.normalized * movementSpeed * Time.deltaTime, Space.World);
            transform.LookAt(target);
            vSpeed -= gravity * Time.deltaTime;
            GetComponent<CharacterController>().Move(new Vector3(0, vSpeed, 0) + direction.normalized * movementSpeed * Time.deltaTime);
            yield return null;

        }
    }

    public void OnDrawGizmos()
    {
        if (!drawGizmos)
            return;

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

