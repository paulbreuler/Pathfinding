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
        if(MPath.Length  == 0) yield break;

        var currentWaypoint = MPath[0];
        
        while (true)
        {
            if (IsTargetInReach() || PathIsFinished())
            {
                IsMoving = false;
                yield break;
            }

            if (Vector3.Distance(transform.position, currentWaypoint) < DistanceToWaypoint)
            {
                TargetIndex++;

                if (TargetIndex >= MPath.Length)
                {
                    yield break;
                }
                currentWaypoint = MPath[TargetIndex];
            }

            UpdatePosition(currentWaypoint);
            yield return null;
        }
    }

    private bool IsTargetInReach()
    {
        // If the target is null or doesn't have the "Player" tag, it's not in reach.
        if (Target == null || Target.tag != "Player") return false;

        var distanceToTarget = Vector3.Distance(transform.position, Target.position);
        var reachDistance = StopBeforeDistance; // Or any other value you find suitable.

        return distanceToTarget <= reachDistance;
    }

    private bool PathIsFinished()
    {
        return TargetIndex >= MPath.Length;
    }
}
