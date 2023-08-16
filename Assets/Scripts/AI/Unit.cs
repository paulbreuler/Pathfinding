using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(Rigidbody))]
public abstract class Unit : MonoBehaviour
{
    #region public variables
    public GameObject AStar;
    public bool DrawGizmos = false;
    public float Gravity = 9.8f;
    public Transform Target;
    public Vector3 LastTargetPosition;
    [Range(1, 20)]
    public float MovementSpeed = 10;

    [Range(100, 500)]
    public float RotationSpeed = 85;
    [Tooltip("How close to get to waypoint before moving towards next. Fixes movement bug. " +
        "Issue seen when close to waypoint this.transform cannot get to exact position and oscillates.")]
    public float DistanceToWaypoint = 1;
    [Tooltip("Distance to stop before target if target is occupying selected space")]
    public float StopBeforeDistance = 2;
    public float CollisionDetectionDistance = 2.0f;
    public int SpacesMoved = 0;
    public float Period = 5f;
    public float NextActionTime = 5f;
    public bool IsSafeToUpdatePath;
    public bool IsMoving;
    public bool IsTargetReached = false;
    public int JumpSpeed = 50;
    public bool ShouldJump;

    #endregion

    #region member variables

    private float _mVerticalSpeed = 0;
    protected Vector3[] MPath;
    protected int TargetIndex;
    private Node _lastNodePosition;
    private List<Node> _lastPositionNeighbors;
    private Vector3 _mLastKnownPosition;
    private Quaternion _mLookAtRotation;
    private Grid _mGrid;
    private Coroutine _lastRoutine;
    private bool _preventExtraNodeUpdate;
    protected Rigidbody _rigidbody;
    private RaycastHit? _isForwardCollision;
    #endregion

    /// <summary>
    /// Called when the script instance is being loaded.
    /// </summary>
    public virtual void Awake()
    {
        if (AStar != null)
            _mGrid = AStar.GetComponent<Grid>();
    }

    /// <summary>
    /// Use this for initialization.
    /// </summary>
    public virtual void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.useGravity = false; // We'll handle gravity manually
        PathRequestManager.RequestPath(transform.position, Target.position, OnPathFound);
        LastTargetPosition = Target.position;
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    protected virtual void Update()
    {
        HandleCollisionChecks();

        ManagePathUpdates();

        HandleJumping();

        if (!IsMoving)
        {
            _rigidbody.velocity = Vector3.zero;
        }

    }

    /// <summary>
    /// Manages the path updates and conditions that trigger them.
    /// </summary>
    private void ManagePathUpdates()
    {
        if (Time.time > NextActionTime)
        {
            NextActionTime += Period;
            IsSafeToUpdatePath = true;
        }
        else
        {
            IsSafeToUpdatePath = false;
        }

        // If we don't check !isMoving the AI may get stuck waiting to update the grid for nextActionTime.
        if (IsSafeToUpdatePath || (!IsMoving && IsTargetReached && !_preventExtraNodeUpdate))
        {
            _preventExtraNodeUpdate = true;
            UpdateNodePosition();
        }

        if (SpacesMoved % 20 == 0 && IsSafeToUpdatePath)
        {
            UpdatePath();
        }
        else if (_isForwardCollision != null && ((RaycastHit)_isForwardCollision).transform.gameObject.GetComponent<Unit>() != null)
        {
            if ((!((RaycastHit)_isForwardCollision).transform.gameObject.GetComponent<Unit>().IsMoving && IsSafeToUpdatePath))
            {
                UpdatePath();
            }
        }
        else if (Target.position != LastTargetPosition)
        {
            IsMoving = true;
            UpdateNodePosition();
            UpdatePath();
        }

        LastTargetPosition = Target.position;
    }

    private void HandleCollisionChecks()
    {
        //var right = transform.TransformDirection(Vector3.forward + Vector3.right).normalized * CollisionDetectionDistance;
        //var left = transform.TransformDirection(Vector3.forward + Vector3.left).normalized * CollisionDetectionDistance;

        //DetectRaycastCollision(right, transform.position, CollisionDetectionDistance);
        //DetectRaycastCollision(left, transform.position, CollisionDetectionDistance);

        var forward = transform.TransformDirection(Vector3.forward) * CollisionDetectionDistance;
        _isForwardCollision = DetectRaycastCollision(forward, transform.position, CollisionDetectionDistance);
    }


    /// <summary>
    /// Manage the jump action if the conditions are met.
    /// </summary>
    private void HandleJumping()
    {
        var lowerForward = transform.TransformDirection(Vector3.forward) * CollisionDetectionDistance;
        var isLowerForwardCollision = DetectRaycastCollision(lowerForward, (transform.position + new Vector3(0, -0.5f, 0)), CollisionDetectionDistance);

        if (isLowerForwardCollision == null) return;

        if (IsGrounded() && ((RaycastHit)isLowerForwardCollision).transform.tag == "Jumpable")
        {
            ShouldJump = true;
            _mVerticalSpeed = JumpSpeed;
            var jumpDirection = (transform.forward + transform.up).normalized;  // Adds forward momentum with the jump
            _rigidbody.AddForce(jumpDirection * JumpSpeed, ForceMode.Impulse);
        }
        else
        {
            ShouldJump = false;
        }
    }



    /// <summary>
    /// Updates the path for the unit.
    /// </summary>
    public void UpdatePath()
    {
        _lastNodePosition.Walkable = Walkable.Passable;
        PathRequestManager.RequestPath(transform.position, Target.position, OnPathFound);
    }

    protected virtual void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            MPath = newPath;
            TargetIndex = 0;

            // Stop coroutine if it is already running.
            if (_lastRoutine != null)
                StopCoroutine(_lastRoutine);

            _lastRoutine = StartCoroutine(FollowPath());
        }
    }

    protected virtual IEnumerator FollowPath()
    {
        var currentWaypoint = MPath[0];
        while (true)
        {

            if (Vector3.Distance(transform.position, currentWaypoint) < DistanceToWaypoint)
            {
                TargetIndex++;

                // If we are done with path.
                if (TargetIndex >= MPath.Length)
                {
                    IsMoving = false;
                    yield break;
                }

                currentWaypoint = MPath[TargetIndex];
            }

            // Occurs each frame
            UpdatePosition(currentWaypoint);
            yield return null;

        }
    }

    public void UpdatePosition(Vector3 targetPosition)
    {
        var desiredVelocity = (targetPosition - transform.position).normalized * MovementSpeed;

        if (!IsGrounded())
        {
            _mVerticalSpeed -= Gravity * Time.deltaTime;
        }
        else
        {
            _mVerticalSpeed = 0;
        }

        desiredVelocity.y = _mVerticalSpeed;

        _rigidbody.velocity = desiredVelocity;
    }

    /// <summary>
    /// Rotate over time to look at target.
    /// </summary>
    protected virtual void UpdateRotation()
    {
        _mLastKnownPosition = Target.transform.position;
        _mLookAtRotation = Quaternion.LookRotation(_mLastKnownPosition - transform.position);
        //m_lookAtRotation.y = 0; removing Y breaks rotation. Probably has to do with conversion to quaternion.

        // If we are not already looking at target continue to rotate.
        if (transform.rotation != _mLookAtRotation)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, _mLookAtRotation, RotationSpeed * Time.deltaTime);
    }


    /// <summary>
    /// Set current node to unwalkable.
    /// </summary>
    private void UpdateNodePosition()
    {
        var node = _mGrid.NodeFromWorldPoint(transform.position);

        if (IsMoving == false)
        {
            _lastPositionNeighbors = _mGrid.GetNeighbours(node);
            foreach (var n in _lastPositionNeighbors.Where(n => n.Walkable != Walkable.Impassable))
            {
                n.Walkable = Walkable.Blocked;
            }
            node.Walkable = Walkable.Blocked;
            _lastNodePosition = node;
            new Vector2(node.GridX, node.GridY);
            return;
        }

        if (_lastNodePosition != null && IsMoving)
        {
            _preventExtraNodeUpdate = false;
            _lastPositionNeighbors = _mGrid.GetNeighbours(node);
            _lastNodePosition.Walkable = Walkable.Passable;
            if (_lastPositionNeighbors != null)
                foreach (var n in _lastPositionNeighbors.Where(n => n.Walkable != Walkable.Impassable))
                {
                    n.Walkable = Walkable.Passable;
                }
            if (!node.Equals(_lastNodePosition))
                SpacesMoved++;
        }
        else
        {
            node.Walkable = Walkable.Blocked;
            _lastNodePosition = node;
            new Vector2(node.GridX, node.GridY);
        }
    }

    /// <summary>
    /// Determines if the unit is grounded.
    /// </summary>
    /// <returns>True if grounded, false otherwise.</returns>
    private bool IsGrounded()
    {
        var distanceToGround = GetComponent<Collider>().bounds.extents.y;
        return Physics.Raycast(transform.position, -Vector3.up, distanceToGround + 0.1f);
    }


    /// <summary>
    /// Draw waypoint path in editor.
    /// </summary>
    public void OnDrawGizmos()
    {
        if (!DrawGizmos)
            return;

        if (MPath == null) return;
        for (var i = TargetIndex; i < MPath.Length; i++)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawCube(MPath[i], Vector3.one);

            Gizmos.DrawLine(i == TargetIndex ? transform.position : MPath[i - 1], MPath[i]);
        }
    }

    /// <summary>
    /// Detects a raycast collision in a given direction.
    /// </summary>
    /// <param name="direction">Direction of the raycast.</param>
    /// <param name="position">Starting position of the raycast.</param>
    /// <param name="distance">Length of the raycast.</param>
    /// <returns>A RaycastHit object if a collision was detected, null otherwise.</returns>
    public RaycastHit? DetectRaycastCollision(Vector3 direction, Vector3 position, float distance)
    {
        var ray = new Ray(position, direction);
        if (Physics.Raycast(ray, out var hit, distance))
        {
            Debug.DrawRay(position, direction, Color.red);
            return hit;
        }

        Debug.DrawRay(position, direction, Color.green);
        return null;
    }
}