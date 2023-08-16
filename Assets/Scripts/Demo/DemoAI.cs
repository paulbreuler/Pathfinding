using System.Collections;
using UnityEngine;

/// <summary>
/// This class represents the DemoAI controller that manages the movement of the NPC characters.
/// </summary>
public class DemoAi : MonoBehaviour
{
    /// <summary>
    /// Reference to the GameObject representing the player character.
    /// </summary>
    private GameObject _player;

    /// <summary>
    /// The position of the player GameObject in the previous frame.
    /// </summary>
    private Vector3 _playerLastPos;

    /// <summary>
    /// An array of NPC characters that this controller is responsible for managing.
    /// </summary>
    private GameObject[] _seekers;

    /// <summary>
    /// An array of spawn points from which the NPC characters are respawned.
    /// </summary>
    public GameObject[] Respawn;

    /// <summary>
    /// Reference to the Grid component used for pathfinding.
    /// </summary>
    Grid _mGrid;

    /// <summary>
    /// Called once when the scene starts. Finds the player and the NPC characters and sets up references to the Grid component.
    /// </summary>
    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _playerLastPos = _player.transform.position;

        _seekers = GameObject.FindGameObjectsWithTag("NPC");
        _mGrid = GetComponent<Grid>();
    }

    /// <summary>
    /// Called once per frame. Handles input and steering of NPC characters, and resets the color of GridNodes after a certain amount of time has passed.
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            foreach (var s in _seekers)
            {
                // Choose a random point on the Grid to move towards.
                var numX = (int)Random.Range(0, _mGrid.gridWorldSize.x);
                var numY = (int)Random.Range(0, _mGrid.gridWorldSize.y);

                // Assign the chosen point as the new target for the NPC character.
                s.GetComponent<Unit>().Target = _mGrid.grid[numX, numY].NodeMesh.transform;

                // Set the GridNode's color to blocked so that the pathfinding algorithm cannot use it.
                _mGrid.grid[numX, numY].NodeMesh.GetComponent<GridColor>().UpdateColor(Walkable.Blocked);

                // After a certain amount of time has passed, reset the color of the GridNode so that it can be used again.
                StartCoroutine(ResetGridColor(_mGrid.grid[numX, numY]));
            }
        }

        // If the player has moved since the last frame, assign each NPC character to follow the player again.
        if (_player.transform.position != _playerLastPos)
        {
            _playerLastPos = _player.transform.position;
            SetBackToMainTarget();
        }

        if (Input.GetKeyUp(KeyCode.R))
        {
            // Coroutine that respawns each NPC character and resets the color of each GridNode.
            foreach (var s in _seekers)
            {
                StartCoroutine(RespawnAi(s));
            }
        }
    }

    /// <summary>
    /// Sets each NPC character's target to the position of the player GameObject.
    /// </summary>
    private void SetBackToMainTarget()
    {
        foreach (var s in _seekers)
        {
            s.GetComponent<Unit>().Target = _player.transform;
        }
    }

    /// <summary>
    /// Coroutine that waits for a certain amount of time and then resets the color of a GridNode.
    /// </summary>
    private static IEnumerator ResetGridColor(Node n)
    {
        yield return new WaitForSeconds(5);
        n.NodeMesh.GetComponent<GridColor>().UpdateColor(Walkable.Passable);
    }

    /// <summary>
    /// Coroutine that waits for a short amount of time and then respawns an NPC character at a random spawn point.
    /// </summary>
    private IEnumerator RespawnAi(GameObject go)
    {
        yield return new WaitForSeconds(0.2f);
        go.transform.position = Respawn[Random.Range(0, Respawn.Length)].transform.position;

        // Reset the color of each GridNode that is not impassable so that it can be used again.
        foreach (var n in GetComponent<Grid>().grid)
        {
            if (n.Walkable != Walkable.Impassable)
            {
                n.Walkable = Walkable.Passable;
            }
        }

        // Set the NPC to begin moving towards its target again.
        go.GetComponent<Unit>().IsMoving = true;
        go.GetComponent<Unit>().UpdatePath();

    }
}
