using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class RangeChecker : MonoBehaviour
{
    public List<string> tags;

    private readonly List<GameObject> _targets = new();

    void OnTriggerEnter(Collider other)
    {
        var invalid = true;
        foreach (var t in tags)
        {
            if (other.CompareTag(t))
            {
                invalid = false;
            }

            if (invalid)
            {
                //Debug.Log("Exiting Invalid");
                return;
            }

            _targets.Add(other.gameObject);
        }
    }

    /// <summary>
    /// Remove target from list so we do not add to calculations
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerExit(Collider other)
    {
        for (var i = 0; i < _targets.Count; i++)
        {
            if (other.gameObject == _targets[i])
            {
                _targets.Remove(other.gameObject);

                return;
            }
        }
    }

    /// <returns>List of targets acquired</returns>
    public List<GameObject> GetValidtargets()
    {
        return _targets;
    }

    /// <summary>
    /// Determine if target is within list of valid targets.
    /// </summary>
    /// <param name="go"> current object </param>
    /// <returns> true is GameObject is in list of valid game objects.</returns>
    public bool InRange(GameObject go)
    {
        return _targets.Any(t => go == t);
    }


}
