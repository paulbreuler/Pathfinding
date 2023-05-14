using UnityEngine;
using System.Collections.Generic;

public class TurretAI : MonoBehaviour
{
    public enum AiStates { NEAREST, FARTHEST, WEAKEST, STRONGEST };

    public AiStates aiState = AiStates.NEAREST;

    private TrackingSystem m_tracker;
    private ShootingSystem m_shootingSystem;
    private RangeChecker m_rangeChecker;

    // Use this for initialization
    void Start()
    {
        m_tracker = GetComponent<TrackingSystem>();
        m_shootingSystem = GetComponent<ShootingSystem>();
        m_rangeChecker = GetComponent<RangeChecker>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_tracker || !m_shootingSystem || !m_rangeChecker)
            return;

        switch (aiState)
        {
            case AiStates.NEAREST:
                TargetNearest();
                break;
            case AiStates.FARTHEST:
                TargetFarthest();
                break;
            case AiStates.STRONGEST:
                break;
            case AiStates.WEAKEST:
                break;
        }
    }

    void TargetNearest()
    {
        var validTargets = m_rangeChecker.GetValidtargets();

        GameObject curTarget = null;
        var closestDist = 0.0f;

        for (var i = 0; i < validTargets.Count; i++)
        {
            if (!validTargets[i])
                continue;

            var dist = Vector3.Distance(transform.position, validTargets[i].transform.position);

            if (!curTarget || dist < closestDist)
            {
                curTarget = validTargets[i];
                closestDist = dist;
            }
        }

        m_tracker.SetTaget(curTarget);
        m_shootingSystem.SetTaget(curTarget);
    }

    void TargetFarthest()
    {
        var validTargets = m_rangeChecker.GetValidtargets();

        GameObject curTarget = null;
        var farthestDis = 0.0f;

        for (var i = 0; i < validTargets.Count; i++)
        {
            var dist = Vector3.Distance(transform.position, validTargets[i].transform.position);

            if (!curTarget || dist > farthestDis)
            {
                curTarget = validTargets[i];
                farthestDis = dist;
            }
        }

        m_tracker.SetTaget(curTarget);
        m_shootingSystem.SetTaget(curTarget);
    }
}
