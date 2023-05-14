using UnityEngine;

public class Health : MonoBehaviour {

    public int maxHealth = 100;

    public int m_currentHealth;
    private MessageHandler m_messageHandler;

	// Use this for initialization
	public virtual void Start () {
        m_currentHealth = maxHealth;
        m_messageHandler = GetComponent<MessageHandler>();
        
        if (m_messageHandler)
            m_messageHandler.RegisterDelegate(ReceiveMessage);

	}

    /// <summary>
    /// Receive messages.
    /// Must have the same parameters as the MessageDelegate delegate.
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="go"></param>
    /// <param name="data"></param>
    void ReceiveMessage(MessageType messageType, GameObject go, MessageData data) {

        switch (messageType) {
            case MessageType.DAMAGED:
                
                // Cast message to damage data
                var damageData = data as DamageData;

                if(damageData != null)
                {
                    ApplyDamage(damageData.Damage, go);
                }

                break;
        }

    }

    public virtual void ApplyDamage(int damage, GameObject go)
    {
        m_currentHealth -= damage;

        // We died!
        if(m_currentHealth <= 0)
        {
            m_currentHealth = 0;

            // Send out message letting other know I died.
            if(m_messageHandler)
            {
                var deathData = new DeathData();
                deathData.Attacker = go;
                deathData.Attacked = gameObject; // this object was attacked

                m_messageHandler.CustomSendMessage(MessageType.DIED, gameObject, deathData);
            }
        }

        if (m_messageHandler)
        {
            var healthData = new HealthData();
            healthData.CurHealth = m_currentHealth;
            healthData.MaxHealth = maxHealth;

        }
    }

}
