using UnityEngine;

public class StandardProjectile : ProjectileBase
{

    private Vector3 m_direction;
    private bool m_fired;
    private GameObject m_target;
    private GameObject m_turret;
    private int m_damage;

    void Update()
    {
        if (m_fired)
        {

            if (m_target)
                transform.position = Vector3.MoveTowards(transform.position, m_target.transform.position, speed * Time.deltaTime);

            if (!m_target)
                Destroy(gameObject);
        }
    }

    public override void FireProjectile(GameObject projectileSpawn, GameObject target, int damage)
    {
        if (projectileSpawn && target)
        {
            // Get direction only
            m_direction = (target.transform.position - projectileSpawn.transform.position).normalized;
            m_target = target;
            m_fired = true;
            m_turret = projectileSpawn;
            m_damage = damage;

            Destroy(gameObject, 10.0f);
        }
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject == m_target)
        {
            var damageData = new DamageData();
            damageData.Damage = m_damage;

            var messageHandler = m_target.GetComponent<MessageHandler>();

            messageHandler.CustomSendMessage(MessageType.DAMAGED, m_turret, damageData);
        }

        // Don't destroy if you hit another projectile
        if (other.gameObject.GetComponent<ProjectileBase>() == null)
            Destroy(gameObject);


    }

}
