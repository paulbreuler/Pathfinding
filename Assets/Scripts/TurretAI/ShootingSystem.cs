using UnityEngine;
using System.Collections.Generic;

public class ShootingSystem : MonoBehaviour
{

    public float fireRate;
    public float fieldOfView;
    public GameObject projectile;
    public int damage;
    public List<GameObject> projectileSpawns;


    //private List<GameObject> m_lastProjectiles = new List<GameObject>();
    private float m_fireTimer = 0.0f;
    private GameObject m_target;

    // Update is called once per frame
    void Update()
    {
        
        if (!m_target)
            return;

        m_fireTimer += Time.deltaTime;
        if (m_fireTimer >= fireRate)
        {
            var angle = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(m_target.transform.position - transform.position));
            if (angle < fieldOfView)
            {
                SpawnProjectile();

            }

            m_fireTimer = 0.0f;
        }
    }

    void SpawnProjectile()
    {
        for (var i = 0; i < projectileSpawns.Count; i++)
        {
            if (projectileSpawns[i])
            {
                var proj = Instantiate(projectile, projectileSpawns[i].transform.position, Quaternion.Euler(projectileSpawns[i].transform.forward)) as GameObject;

                // Using base class we can inherit whatever type of projectile class we create.
                proj.GetComponent<ProjectileBase>().FireProjectile(projectileSpawns[i], m_target, damage);
            }
        }
    }

    public void SetTaget(GameObject target)
    {
        m_target = target;
    }
}
