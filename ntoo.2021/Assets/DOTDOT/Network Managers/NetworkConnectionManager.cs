using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DotDot.Core.Network
{
    public abstract class NetworkConnectionManager : MonoBehaviour
    {
        [System.Serializable]
        public class TextMessageEvent : UnityEvent<string> { }

        [System.Serializable]
        public class BinaryMessageEvent : UnityEvent<byte[]> { }
        
        public abstract bool IsOpen { get; }

        [Tooltip("Actions to perform when text message is received")]
        public TextMessageEvent OnTextMessageReceived;

        [Tooltip("Actions to perform when binary data message is received")]
        public BinaryMessageEvent OnBinaryMessageReceived;

        [Tooltip("Actions to perform when network connection is opened")]
        public UnityEvent OnConnectionOpened;

        public abstract void SendTextMessage(string message);
        public abstract void SendBinaryDataMessage(byte[] binaryData);
    }
}

