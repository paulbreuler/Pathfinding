using UnityEngine;
using System.Collections.Generic;


// Used to inherit data types
public abstract class MessageData { };
public enum MessageType { DAMAGED, HEALTHCHANGED, DIED };

/// <summary>
/// Delegate function that can send messages out
/// </summary>
/// <param name="messageType"> Type of message</param>
/// <param name="go"> GameObject that is sending the message</param>
/// <param name="data"> Data to be sent</param>
public delegate void MessageDelegate(MessageType messageType, GameObject go, MessageData data);

public class MessageHandler : MonoBehaviour
{
    [Tooltip("Types of messages this GameObject can receive")]
    public List<MessageType> messages;

    private List<MessageDelegate> m_messageDelegates = new List<MessageDelegate>();

    /// <summary>
    /// Register a new delegate
    /// </summary>
    /// <param name="messageDelegate"></param>
    public void RegisterDelegate(MessageDelegate messageDelegate)
    {
        m_messageDelegates.Add(messageDelegate);

    }

    public bool CustomSendMessage(MessageType messageType, GameObject go, MessageData data)
    {
        var approved = false;

        // Check to see if we have a message that can be sent
        for (var i = 0; i < messages.Count; i++)
        {
            if (messages[i] == messageType)
            {
                approved = true;
                break;
            }

        }

        if (!approved)
            return false;

        for (var i = 0; i < m_messageDelegates.Count; i++)
        {
            m_messageDelegates[i](messageType, go, data);
        }

        return true;

    }
}

public class DamageData : MessageData {
    public int damage;
}


/// <summary>
/// Data passed on death
/// </summary>
public class DeathData : MessageData
{
    public GameObject attacker;
    public GameObject attacked;
}

/// <summary>
/// Data passed on health change
/// </summary>
public class HealthData: MessageData
{
    public int maxHealth;
    public int curHealth;
}