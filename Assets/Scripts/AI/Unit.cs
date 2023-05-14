using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

public abstract class Unit : MonoBehaviour
{
    #region public variables
    [FormerlySerializedAs("Astar")] public GameObject astar;
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

    private float _mVerticalSpeed = 0;
    protected Vector3[] MPath;
    protected int MTargetIndex;
    private CharacterController _mCharacterController;
    private Node _lastNodePosition;
    private List<Node> _lastPositionNeighbors;
    private Vector3 _mLastKnownPosition;
    private Quaternion _mLookAtRotation;
    private Grid _mGrid;
    private Coroutine _lastRoutine = null;
    private bool _preventExtraNodeUpdate = false;
    #endregion

    public virtual void Awake()
    {
        if (astar != null)
            _mGrid = astar.GetComponent<Grid>();
    }

    public virtual void Start()
    {
        _mCharacterController = GetComponent<CharacterController>();
        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
        lastTargetPosition = target.position;
    }

    public virtual void Update()
    {
        var right = transform.TransformDirection(Vector3.forward + Vector3.right).normalized * collisionDetectionDistance;
        var left = transform.TransformDirection(Vector3.forward + Vector3.left).normalized * collisionDetectionDistance;

        DetectRaycastCollision(right, transform.position, collisionDetectionDistance);
        DetectRaycastCollision(left, transform.position, collisionDetectionDistance);

        var forward = transform.TransformDirection(Vector3.forward) * collisionDetectionDistance;
        var isForwardCollision = DetectRaycastCollision(forward, transform.position, collisionDetectionDistance);

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
        if (isSafeToUpdatePath || (!isMoving && isTargetReached && !_preventExtraNodeUpdate))
        {
            _preventExtraNodeUpdate = true;
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
        var lowerForward = transform.TransformDirection(Vector3.forward) * collisionDetectionDistance;
        var isLowerForwardCollision = DetectRaycastCollision(lowerForward, (transform.position + new Vector3(0, -0.5f, 0)), collisionDetectionDistance);
        if (isLowerForwardCollision == null) return;
        if (_mCharacterController.isGrounded && ((RaycastHit)isLowerForwardCollision).transform.tag == "Jumpable")
        {
            _mVerticalSpeed = jumpSpeed;

        }
    }

    public void UpdatePath()
    {
        _lastNodePosition.Walkable = Walkable.Passable;
        PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
    }

    public virtual void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            pathFoundCount++;
            MPath = newPath;
            MTargetIndex = 0;

            // Stop coroutine if it is already running.
            if (_lastRoutine != null)
                StopCoroutine(_lastRoutine);

            _lastRoutine = StartCoroutine(FollowPath());
        }
    }

    public virtual IEnumerator FollowPath()
    {
        var currentWaypoint = MPath[0];
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
        var position = transform.position;
        var node = _mGrid.NodeFromWorldPoint(position);

        var direction = destination - position;
        _mVerticalSpeed -= Mathf.Clamp(gravity * Time.deltaTime, 0, 30);

        float penalty = node.MovementPenalty == 0 ? 1 : node.MovementPenalty;
        var movement = new Vector3(0, _mVerticalSpeed, 0) + direction.normalized * (movementSpeed * (100-penalty))/100 * Time.deltaTime;
        // Handles steps and other cases by default
        _mCharacterController.Move(movement);
        //transform.Translate(direction.normalized * movementSpeed * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// Rotate over time to look at target.
    /// </summary>
    public virtual void UpdateRotation()
    {
        _mLastKnownPosition = target.transform.position;
        _mLookAtRotation = Quaternion.LookRotation(_mLastKnownPosition - transform.position);
        //m_lookAtRotation.y = 0; removing Y breaks rotation. Probably has to do with conversion to quaternion.

        // If we are not already looking at target continue to rotate.
        if (transform.rotation != _mLookAtRotation)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, _mLookAtRotation, rotationSpeed * Time.deltaTime);
    }


    /// <summary>
    /// Set current node to unwalkable.
    /// </summary>
    public void UpdateNodePosition()
    {
        var node = _mGrid.NodeFromWorldPoint(transform.position);

        if (isMoving == false)
        {
            _lastPositionNeighbors = _mGrid.GetNeighbours(node);
            foreach (var n in _lastPositionNeighbors.Where(n => n.Walkable != Walkable.Impassable))
            {
                n.Walkable = Walkable.Blocked;
            }
            node.Walkable = Walkable.Blocked;
            _lastNodePosition = node;
            currentPosition = new Vector2(node.GridX, node.GridY);
            return;
        }

        if (_lastNodePosition != null && isMoving)
        {
            _preventExtraNodeUpdate = false;
            _lastPositionNeighbors = _mGrid.GetNeighbours(node);
            _lastNodePosition.Walkable = Walkable.Passable;
            if (_lastPositionNeighbors != null)
                foreach (var n in _lastPositionNeighbors)
                {
                    if (n.Walkable != Walkable.Impassable)
                        n.Walkable = Walkable.Passable;
                }
            if (!node.Equals(_lastNodePosition))
                spacesMoved++;
        }
        else
        {
            node.Walkable = Walkable.Blocked;
            _lastNodePosition = node;
            currentPosition = new Vector2(node.GridX, node.GridY);
        }



    }

    /// <summary>
    /// Draw waypoint path in editor.
    /// </summary>
    public void OnDrawGizmos()
    {
        if (!drawGizmos)
            return;

        if (MPath != null)
        {
            for (var i = MTargetIndex; i < MPath.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(MPath[i], Vector3.one);

                if (i == MTargetIndex)
                {
                    Gizmos.DrawLine(transform.position, MPath[i]);
                }
                else
                {
                    Gizmos.DrawLine(MPath[i - 1], MPath[i]);
                }
            }
        }
    }

    public RaycastHit? DetectRaycastCollision(Vector3 direction, Vector3 position, float distance)
    {
        var ray = new Ray(position, direction);
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

