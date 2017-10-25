using UnityEngine;
using System.Collections;

public class NPCUnit : Unit
{

    public override void Update()
    {
        base.Update();
        UpdateRotation();

    }

    public override IEnumerator FollowPath()
    {
        //TODO: Fails here with index out of range when rapidly changing paths
        Vector3 currentWaypoint;
        if (m_path != null && m_path.Length > 0)
            currentWaypoint = m_path[0];
        else
            currentWaypoint = transform.position;

        while (true)
        {

            if (Vector3.Distance(transform.position, currentWaypoint) < distanceToWaypoint)
            {
                m_targetIndex++;

                // If we are done with path.
                if (m_targetIndex >= m_path.Length)
                {
                    isMoving = false;
                    yield break;
                }


                currentWaypoint = m_path[m_targetIndex];
            }

            Vector3 forward = transform.TransformDirection(Vector3.forward) * stopBeforeDistance;
            RaycastHit? isForwardCollision = DetectRaycastCollision(forward, transform.position, collisionDetectionDistance);
            // Determine if target space is occupied
            if (isForwardCollision != null && ((RaycastHit)isForwardCollision).transform == target)
            {
                isMoving = false;
                isTargetReached = true;
                m_path = null;
                yield break;
            }
            else
            {
                // Occurs each frame
                isMoving = true;
                isTargetReached = false;
                UpdatePosition(currentWaypoint);

            }

            yield return null;

        } // End While
    }



}
