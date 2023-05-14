using UnityEngine;

public class Death : MonoBehaviour
{
    private MessageHandler m_messageHandler;
    // Use this for initialization
    void Start()
    {
        m_messageHandler = GetComponent<MessageHandler>();

        if (m_messageHandler)
            m_messageHandler.RegisterDelegate(ReceiveMessage);
    }

    void ReceiveMessage(MessageType messageType, GameObject go, MessageData data)
    {
        switch (messageType)
        {
            case MessageType.DIED:
               // Debug.Log("Death Note: " + gameObject.name);

                var deathData = data as DeathData;

                if (deathData != null)
                    Die();
                break;
            default:
                break;
        }

    }

    /// <summary>
    /// Handle death
    /// </summary>
    private void Die()
    {
        Destroy(gameObject);
    }
}
