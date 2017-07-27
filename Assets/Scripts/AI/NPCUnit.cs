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
        Vector3 currentWaypoint = m_path[0];
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
            RaycastHit? isForwardCollision = DetectRaycastCollision(forward);
            // Determine if target space is occupied
            if (isForwardCollision != null && ((RaycastHit)isForwardCollision).transform == target )
            {
                isMoving = false;
                m_path = null;
                yield break;
            }else
            {
                // Occurs each frame
                isMoving = true;
                UpdatePosition(currentWaypoint);
                
            }
            
            yield return null;

        } // End While
    }



}
