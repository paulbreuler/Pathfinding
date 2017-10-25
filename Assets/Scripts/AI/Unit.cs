using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Unit : MonoBehaviour
{
    #region public variables
    public GameObject Astar;
    public bool drawGizmos = false;
    public float gravity = 9.8f;
    public Transform target;
    public Vector3 lastTargetPosition;
    public float movementSpeed = 20;
    public float rotationSpeed = 85;
    [Tooltip("How close to get to waypoint before moving towards next. Fixes movement bug. " +
        "Issue seen when close to waypoint this.transform cannot get to exact position and oscillates.")]
    public float distanceToWaypoint = 1;
    [Tooltip("Distance to stop before target if target is occupying selected space")]
    public float stopBeforeDistance = 2;
    public float collisionDetectionDistance = 2.0f;
    public Vector2 currentPosition = new Vector2(0, 0);
    public int spacesMoved = 0;
    // Default action times to 5 second interval
    public float period = 5f;
    public float nextActionTime = 5f;
    public bool isSafeToUpdatePath = false;
    public int pathFoundCount = 0;
    public bool isMoving = false;
    public bool isTargetReached = false;
    public int jumpSpeed = 1;
    #endregion

    #region member variables
    protected float m_verticalSpeed = 0;
    protected Vector3[] m_path;
    protected int m_targetIndex;
    protected CharacterController m_characterController;
    private Node lastNodePosition;
    private List<Node> lastPositionNeighbors;
    private Vector3 m_lastKnownPosition;
    private Quaternion m_lookAtRotation;
    private Grid m_grid;
    private Coroutine lastRoutine = null;
    private bool preventExtraNodeUpdate = false;
    #endregion

    public virtual void Awake()
    {
        if (Astar != null)
            m_grid = Astar.GetComponent<Grid>();
    }

    public virtual void Start()
    {
        m_characterController = GetComponent<CharacterController>();
        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
        lastTargetPosition = target.position;
    }

    public virtual void Update()
    {
        Vector3 right = transform.TransformDirection(Vector3.forward + Vector3.right).normalized * collisionDetectionDistance;
        Vector3 left = transform.TransformDirection(Vector3.forward + Vector3.left).normalized * collisionDetectionDistance;

        DetectRaycastCollision(right, transform.position, collisionDetectionDistance);
        DetectRaycastCollision(left, transform.position, collisionDetectionDistance);

        Vector3 forward = transform.TransformDirection(Vector3.forward) * collisionDetectionDistance;
        RaycastHit? isForwardCollision = DetectRaycastCollision(forward, transform.position, collisionDetectionDistance);

        if (Time.time > nextActionTime)
        {
            nextActionTime += period;
            isSafeToUpdatePath = true;
        }
        else
        {
            isSafeToUpdatePath = false;
        }

        // If we don't check !isMoving the AI may get stuck waiting to update the grid for nextActionTime.
        if (isSafeToUpdatePath || (!isMoving && isTargetReached && !preventExtraNodeUpdate))
        {
            preventExtraNodeUpdate = true;
            UpdateNodePosition();
        }

        if (spacesMoved % 20 == 0 && isSafeToUpdatePath)
        {
            UpdatePath();
        }
        else if (isForwardCollision != null && ((RaycastHit)isForwardCollision).transform.gameObject.GetComponent<Unit>() != null)
        {
            if ((!((RaycastHit)isForwardCollision).transform.gameObject.GetComponent<Unit>().isMoving && isSafeToUpdatePath))
            {
                UpdatePath();
            }
        }
        else if (target.position != lastTargetPosition)
        {
            isMoving = true;
            UpdateNodePosition();
            UpdatePath();
        }

        lastTargetPosition = target.position;

        // Jump obstacle
        Vector3 lowerForward = transform.TransformDirection(Vector3.forward) * collisionDetectionDistance;
        RaycastHit? islowerForwardCollision = DetectRaycastCollision(lowerForward, (transform.position + new Vector3(0, -0.5f, 0)), collisionDetectionDistance);
        if (islowerForwardCollision != null)
        {
            if (m_characterController.isGrounded && ((RaycastHit)islowerForwardCollision).transform.tag == "Jumpable")
            {
                m_verticalSpeed = jumpSpeed;

            }
        }
    }

    public void UpdatePath()
    {
        lastNodePosition.walkable = Walkable.Passable;
        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
    }

    public virtual void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            pathFoundCount++;
            m_path = newPath;
            m_targetIndex = 0;

            // Stop coroutine if it is already running.
            if (lastRoutine != null)
                StopCoroutine(lastRoutine);

            lastRoutine = StartCoroutine(FollowPath());
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
                {
                    isMoving = false;
                    yield break;
                }


                currentWaypoint = m_path[m_targetIndex];
            }

            // Occurs each frame
            UpdatePosition(currentWaypoint);
            yield return null;

        }
    }

    /// <summary>
    /// Calculates movement towards @param(destination).
    /// </summary>
    /// <param name="destination"> Target to be moved towards </param>
    public virtual void UpdatePosition(Vector3 destination)
    {
        Node node = m_grid.NodeFromWorldPoint(transform.position);

        Vector3 direction = destination - transform.position;
        m_verticalSpeed -= Mathf.Clamp(gravity * Time.deltaTime, 0, 30);

        float penalty = node.movementPenalty == 0 ? 1 : node.movementPenalty;
        Vector3 movement = new Vector3(0, m_verticalSpeed, 0) + direction.normalized * movementSpeed * (100-penalty)/100 * Time.deltaTime;
        // Handles steps and other cases by default
        m_characterController.Move(movement);
        //transform.Translate(direction.normalized * movementSpeed * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// Rotate over time to look at target.
    /// </summary>
    public virtual void UpdateRotation()
    {
        m_lastKnownPosition = target.transform.position;
        m_lookAtRotation = Quaternion.LookRotation(m_lastKnownPosition - transform.position);
        //m_lookAtRotation.y = 0; removing Y breaks rotation. Probably has to do with conversion to quaternion.

        // If we are not already looking at target continue to rotate.
        if (transform.rotation != m_lookAtRotation)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, m_lookAtRotation, rotationSpeed * Time.deltaTime);
    }


    /// <summary>
    /// Set current node to unwalkable.
    /// </summary>
    public void UpdateNodePosition()
    {
        Node node = m_grid.NodeFromWorldPoint(transform.position);

        if (isMoving == false)
        {
            lastPositionNeighbors = m_grid.GetNeighbours(node);
            foreach (Node n in lastPositionNeighbors)
            {
                if (n.walkable != Walkable.Impassable)
                    n.walkable = Walkable.Blocked;
            }
            node.walkable = Walkable.Blocked;
            lastNodePosition = node;
            currentPosition = new Vector2(node.gridX, node.gridY);
            return;
        }

        if (lastNodePosition != null && isMoving)
        {
            preventExtraNodeUpdate = false;
            lastPositionNeighbors = m_grid.GetNeighbours(node);
            lastNodePosition.walkable = Walkable.Passable;
            if (lastPositionNeighbors != null)
                foreach (Node n in lastPositionNeighbors)
                {
                    if (n.walkable != Walkable.Impassable)
                        n.walkable = Walkable.Passable;
                }
            if (!node.Equals(lastNodePosition))
                spacesMoved++;
        }
        else
        {
            node.walkable = Walkable.Blocked;
            lastNodePosition = node;
            currentPosition = new Vector2(node.gridX, node.gridY);
        }



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

    public RaycastHit? DetectRaycastCollision(Vector3 direction, Vector3 position, float distance)
    {
        Ray ray = new Ray(position, direction);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, distance))
        {
            Debug.DrawRay(position, direction, Color.red);
            return hit;
        }
        else
        {
            Debug.DrawRay(position, direction, Color.green);
            return null;
        }
    }
}

