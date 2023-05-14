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
        if (MPath is { Length: > 0 })
            currentWaypoint = MPath[0];
        else
            currentWaypoint = transform.position;

        while (true)
        {

            if (Vector3.Distance(transform.position, currentWaypoint) < distanceToWaypoint)
            {
                MTargetIndex++;

                // If we are done with path.
                if (MTargetIndex >= MPath.Length)
                {
                    isMoving = false;
                    yield break;
                }


                currentWaypoint = MPath[MTargetIndex];
            }

            var forward = transform.TransformDirection(Vector3.forward) * stopBeforeDistance;
            var isForwardCollision = DetectRaycastCollision(forward, transform.position, collisionDetectionDistance);
            // Determine if target space is occupied
            if (isForwardCollision != null && ((RaycastHit)isForwardCollision).transform == target)
            {
                isMoving = false;
                isTargetReached = true;
                MPath = null;
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
