using UnityEngine;
using System.Collections.Generic;

public class RangeChecker : MonoBehaviour
{
    public List<string> tags;

    private List<GameObject> m_targets = new List<GameObject>();

    void OnTriggerEnter(Collider other)
    {

        var invalid = true;
        for (var i = 0; i < tags.Count; i++)
        {
            if (other.CompareTag(tags[i]))
            {
                invalid = false;
            }

            if (invalid)
            {
                //Debug.Log("Exiting Invalid");
                return;
            }

            m_targets.Add(other.gameObject);
        }
    }

    /// <summary>
    /// Remove target from list so we do not add to calculations
    /// </summary>
    /// <param name="other"></param>
    void OnTriggerExit(Collider other)
    {
        for (var i = 0; i < m_targets.Count; i++)
        {
            if (other.gameObject == m_targets[i])
            {
                m_targets.Remove(other.gameObject);

                return;
            }
        }
    }

    /// <returns>List of targets acquired</returns>
    public List<GameObject> GetValidtargets()
    {
        return m_targets;
    }

    /// <summary>
    /// Determine if target is within list of valid targets.
    /// </summary>
    /// <param name="go"> current object </param>
    /// <returns> true is GameObject is in list of valid game objects.</returns>
    public bool InRange(GameObject go)
    {
        for (var i = 0; i < m_targets.Count; i++)
        {
            if (go == m_targets[i])
            {
                return true;
            }
        }
        return false;
    }


}
