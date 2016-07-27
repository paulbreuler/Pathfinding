using UnityEngine;
using System.Collections;

public class NPCUnit : Unit
{

    public void Update() {
        transform.LookAt(target);
    }


    public override IEnumerator FollowPath()
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

            // Determine if target space is occupied
            if (StopBeforeTarget(2))
                yield break;

            // Occurs each frame 
            Vector3 direction = currentWaypoint - transform.position;
            vSpeed -= gravity * Time.deltaTime;
            GetComponent<CharacterController>().Move(new Vector3(0, vSpeed, 0) + direction.normalized * movementSpeed * Time.deltaTime);

            yield return null;

        }
    }

    /// <summary>
    /// Stop before reaching the target.
    /// </summary>
    /// <returns>true if target is within distance</returns>
    public bool StopBeforeTarget(float distance)
    {
        bool result = false;

        // TODO Ray should be at eye level
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, distance) && hit.transform == target)
        {
            Debug.DrawLine(transform.position, hit.point, Color.red, 5);
            result = true;
        }

        return result;
    }

}
