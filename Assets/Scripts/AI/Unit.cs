using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;

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
    public Vector2 CurrentPosition = new(0, 0);
    public int SpacesMoved = 0;
    public float Period = 5f;
    public float NextActionTime = 5f;
    public bool IsSafeToUpdatePath;
    public bool IsMoving;
    public bool IsTargetReached = false;
    public int JumpSpeed = 1;
    #endregion

    #region member variables

    private float _mVerticalSpeed = 0;
    protected Vector3[] MPath;
    protected int TargetIndex;
    private CharacterController _characterController;
    private Node _lastNodePosition;
    private List<Node> _lastPositionNeighbors;
    private Vector3 _mLastKnownPosition;
    private Quaternion _mLookAtRotation;
    private Grid _mGrid;
    private Coroutine _lastRoutine;
    private bool _preventExtraNodeUpdate;
    #endregion

    public virtual void Awake()
    {
        if (AStar != null)
            _mGrid = AStar.GetComponent<Grid>();
    }

    public virtual void Start()
    {
        _characterController = GetComponent<CharacterController>();
        PathRequestManager.RequestPath(transform.position, Target.position, OnPathFound);
        LastTargetPosition = Target.position;
    }

    protected virtual void Update()
    {
        var right = transform.TransformDirection(Vector3.forward + Vector3.right).normalized * CollisionDetectionDistance;
        var left = transform.TransformDirection(Vector3.forward + Vector3.left).normalized * CollisionDetectionDistance;

        DetectRaycastCollision(right, transform.position, CollisionDetectionDistance);
        DetectRaycastCollision(left, transform.position, CollisionDetectionDistance);

        var forward = transform.TransformDirection(Vector3.forward) * CollisionDetectionDistance;
        var isForwardCollision = DetectRaycastCollision(forward, transform.position, CollisionDetectionDistance);

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
        else if (isForwardCollision != null && ((RaycastHit)isForwardCollision).transform.gameObject.GetComponent<Unit>() != null)
        {
            if ((!((RaycastHit)isForwardCollision).transform.gameObject.GetComponent<Unit>().IsMoving && IsSafeToUpdatePath))
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

        // Jump obstacle
        var lowerForward = transform.TransformDirection(Vector3.forward) * CollisionDetectionDistance;
        var isLowerForwardCollision = DetectRaycastCollision(lowerForward, (transform.position + new Vector3(0, -0.5f, 0)), CollisionDetectionDistance);
        if (isLowerForwardCollision == null) return;
        if (_characterController.isGrounded && ((RaycastHit)isLowerForwardCollision).transform.tag == "Jumpable")
        {
            _mVerticalSpeed = JumpSpeed;

        }
    }

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

    /// <summary>
    /// Calculates movement towards @param(destination).
    /// </summary>
    /// <param name="destination"> Target to be moved towards </param>
    protected virtual void UpdatePosition(Vector3 destination)
    {
        var position = transform.position;
        var node = _mGrid.NodeFromWorldPoint(position);

        var direction = destination - position;
        _mVerticalSpeed -= Mathf.Clamp(Gravity * Time.deltaTime, 0, 30);

        float penalty = node.MovementPenalty == 0 ? 1 : node.MovementPenalty;
        var movement = new Vector3(0, _mVerticalSpeed, 0) + direction.normalized * (MovementSpeed * (100-penalty))/100 * Time.deltaTime;
        // Handles steps and other cases by default
        _characterController.Move(movement);
        //transform.Translate(direction.normalized * movementSpeed * Time.deltaTime, Space.World);
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
            CurrentPosition = new Vector2(node.GridX, node.GridY);
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
            CurrentPosition = new Vector2(node.GridX, node.GridY);
        }
    }

    /// <summary>
    /// Draw waypoint path in editor.
    /// </summary>
    public void OnDrawGizmos()
    {
        if (!DrawGizmos)
            return;

        if (MPath != null)
        {
            for (var i = TargetIndex; i < MPath.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(MPath[i], Vector3.one);

                if (i == TargetIndex)
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

