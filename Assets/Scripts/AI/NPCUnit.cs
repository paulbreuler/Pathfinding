using UnityEngine;
using System.Collections;

public class NPCUnit : Unit
{

    public override IEnumerator FollowPath()
    {
        Vector3 currentWaypoint = m_path[0];
        while (true)
        {

            if (Vector3.Distance(transform.position, currentWaypoint) < distanceToWaypoint)
            {
                m_targetIndex++;

                // If we are done with path.
                if (m_targetIndex >= m_path.Length)
                    yield break;

                currentWaypoint = m_path[m_targetIndex];
            }

            // Determine if target space is occupied
            if (StopBeforeTarget(2))
            {
                m_targetIndex++; // Clear out last waypoint
                yield break;
            }

            // Occurs each frame
            UpdatePosition(currentWaypoint);
            UpdateRotation();

            yield return null;

        } // End While
    }



}
