using UnityEngine;
using System.Collections.Generic;
using System.Linq;


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

    private readonly List<MessageDelegate> _messageDelegates = new();

    /// <summary>
    /// Register a new delegate
    /// </summary>
    /// <param name="messageDelegate"></param>
    public void RegisterDelegate(MessageDelegate messageDelegate)
    {
        _messageDelegates.Add(messageDelegate);

    }

    public bool CustomSendMessage(MessageType messageType, GameObject go, MessageData data)
    {
        var approved = messages.Any(t => t == messageType);

        // Check to see if we have a message that can be sent

        if (!approved)
            return false;

        foreach (var messageDelegate in _messageDelegates)
        {
            messageDelegate(messageType, go, data);
        }

        return true;

    }
}

public class DamageData : MessageData {
    public int Damage;
}


/// <summary>
/// Data passed on death
/// </summary>
public class DeathData : MessageData
{
    public GameObject Attacker;
    public GameObject Attacked;
}

/// <summary>
/// Data passed on health change
/// </summary>
public class HealthData: MessageData
{
    public int MaxHealth;
    public int CurHealth;
}