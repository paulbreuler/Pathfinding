using UnityEngine;
using System.Collections;

public abstract class Unit : MonoBehaviour
{
    #region public variables
    public bool drawGizmos = false;
    public float gravity = 9.8f;
    public Transform m_target;
    public float movementSpeed = 20;
    public float rotationSpeed = 85;
    [Tooltip("How close to get to waypoint before moving towards next. Fixes movement bug. " +
        "Issue seen when close to waypoint this.transform cannot get to exact position and oscillates.")]
    public float distanceToWaypoint = 1;
    [Tooltip("Distance to stop before target if target is occupying selected space")]
    public float stopBeforeDistance = 2;
    #endregion

    #region member variables
    protected float m_verticalSpeed = 0;
    protected Vector3[] m_path;
    protected int m_targetIndex;
    protected CharacterController m_characterController;

    private Vector3 m_lastKnownPosition = Vector3.zero;
    private Quaternion m_lookAtRotation;
    #endregion

    public virtual void Start()
    {
        m_characterController = GetComponent<CharacterController>();
        PathRequestManager.RequestPath(transform.position, m_target.position, OnPathFound);
    }

    public virtual void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            m_path = newPath;
            m_targetIndex = 0;

            // Stop coroutine if it is already running.
            StopCoroutine(FollowPath());
            StartCoroutine(FollowPath());
        }
    }

    public virtual IEnumerator FollowPath()
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

            // Occurs each frame
            UpdatePosition(currentWaypoint);
            UpdateRotation();

            yield return null;

        }
    }

    /// <summary>
    /// Calculates movement towards @param(destination).
    /// </summary>
    /// <param name="destination"> Target to be moved towards </param>
    public virtual void UpdatePosition(Vector3 destination)
    {
        Vector3 direction = destination - transform.position;
        m_verticalSpeed -= gravity * Time.deltaTime;

        // Handles steps and other cases by default
        m_characterController.Move(new Vector3(0, m_verticalSpeed, 0) + direction.normalized * movementSpeed * Time.deltaTime);
        //transform.Translate(direction.normalized * movementSpeed * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// Rotate over time to look at target.
    /// </summary>
    public virtual void UpdateRotation()
    {
        // Get look direction
        if (m_lastKnownPosition != m_target.transform.position)
        {
            m_lastKnownPosition = m_target.transform.position;
            m_lookAtRotation = Quaternion.LookRotation(m_lastKnownPosition - transform.position);
            //m_lookAtRotation.y = 0; removing Y breaks rotation. Probably has to do with conversion to quaternion.
        }

        // If we are not already looking at target continue to rotate.
        if (transform.rotation != m_lookAtRotation)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, m_lookAtRotation, rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// Stop before reaching the target.
    /// </summary>
    /// <returns>true if target is within distance</returns>
    protected bool StopBeforeTarget(float distance)
    {
        bool result = false;

        // TODO Ray should be at eye level
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, distance) && hit.transform == m_target)
        {
            Debug.DrawLine(transform.position, hit.point, Color.red, 5);
            result = true;
        }

        return result;
    }

    /// <summary>
    /// Draw waypoint path in editor.
    /// </summary>
    public void OnDrawGizmos()
    {
        if (!drawGizmos)
            return;

        if (m_path != null)
        {
            for (int i = m_targetIndex; i < m_path.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(m_path[i], Vector3.one);

                if (i == m_targetIndex)
                {
                    Gizmos.DrawLine(transform.position, m_path[i]);
                }
                else
                {
                    Gizmos.DrawLine(m_path[i - 1], m_path[i]);
                }
            }
        }
    }
}

