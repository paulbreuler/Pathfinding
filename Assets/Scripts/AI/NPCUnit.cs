using UnityEngine;
using System.Collections;

public class NPCUnit : Unit
{
    protected override void Update()
    {
        base.Update();
        UpdateRotation();

    }

    protected override IEnumerator FollowPath()
    {
        var currentWaypoint = MPath is { Length: > 0 } ? MPath[0] : transform.position;

        while (true)
        {

            if (Vector3.Distance(transform.position, currentWaypoint) < DistanceToWaypoint)
            {
                TargetIndex++;

                if (MPath != null && TargetIndex < MPath.Length)
                {
                    currentWaypoint = MPath[TargetIndex];
                }
                else
                {
                    yield break; // Exit if the path doesn't exist or we're out of bounds.
                }

                currentWaypoint = MPath[TargetIndex];
            }

            var forward = transform.TransformDirection(Vector3.forward) * StopBeforeDistance;
            var isForwardCollision = DetectRaycastCollision(forward, transform.position, CollisionDetectionDistance);
            // Determine if target space is occupied
            if (isForwardCollision != null && ((RaycastHit)isForwardCollision).transform == Target)
            {
                IsMoving = false;
                IsTargetReached = true;
                MPath = null;
                yield break;
            }
            else
            {
                // Occurs each frame
                IsMoving = true;
                IsTargetReached = false;
                UpdatePosition(currentWaypoint);

            }

            yield return null;

        } // End While
    }
}
