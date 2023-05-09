using UnityEngine;
using System.Collections.Concurrent;
using UnityEngine.Assertions;
using WebSocketSharp;

namespace DotDot.Core.Network
{
    public class WebSocketManager : NetworkConnectionManager
    {
        [SerializeField, Tooltip("Hostname to connect to (can be name or ip address)")]
        private string hostname;

        [SerializeField, Tooltip("Port number to connect to")]
        private int port;

#if UNITY_EDITOR
        [SerializeField, Tooltip("Select this to simulate a connection instead of actually sending and receiving data")]
        private bool simulate;
#endif

        public override bool IsOpen
        {
            get
            {
#if UNITY_EDITOR
                if(simulate) return true;
#endif
                if (_socket == null) return false;
                return _socket.ReadyState == WebSocketState.Open;
            }
        }

        private WebSocket _socket;
        private readonly ConcurrentQueue<MessageEventArgs> receivedMessages = new ConcurrentQueue<MessageEventArgs>();

        private readonly ConcurrentQueue<System.Action> portEvents = new ConcurrentQueue<System.Action>();

        private void Update()
        {
            while (!portEvents.IsEmpty)
            {
                System.Action portEventAction;
                if(portEvents.TryDequeue(out portEventAction))
                {
                    portEventAction();
                }
            }

            while (!receivedMessages.IsEmpty)
            {
                MessageEventArgs message;
                if(receivedMessages.TryDequeue(out message))
                {
                    if (message.IsText)
                    {
                        Debug.Log("[Web Socket Manager] Raising Unity Event with data: " + message.Data);
                        OnTextMessageReceived.Invoke(message.Data);
                    }
                    else if (message.IsBinary)
                    {
                        OnBinaryMessageReceived.Invoke(message.RawData);
                    }
                }
            }
        }

        /// <summary>
        /// Receives messages from the websocket. Note this function is called on a different thread, so messages must be passed to
        /// main thread using threadsafe structure.
        /// </summary>
        /// <param name="sender">The source of the message</param>
        /// <param name="message">The received message</param>
        private void Socket_OnMessage(object sender, MessageEventArgs message)
        {
            Debug.Log("[Web Socket Manager] Received " + (message.IsText ? "Text " : "Binary ") + "message from " + sender.ToString());
            receivedMessages.Enqueue(message);
        }

        private void Socket_OnOpen(object sender, System.EventArgs e)
        {
            Debug.Log("[Web Socket Manager] Web Socket Open");
            portEvents.Enqueue(() => { OnConnectionOpened.Invoke(); });
        }

        private void Socket_OnClose(object sender, CloseEventArgs e)
        {
            Debug.Log("[Web Socket Manager] Web Socket Closed (Reason:  " + e.Reason + ")");
        }

        private void Socket_OnError(object sender, ErrorEventArgs e)
        {
            Debug.LogError("[Web Socket Manager] Received Socket Error: " + e.Message);
        }
        
        private void OnEnable()
        {
#if UNITY_EDITOR
            if(simulate)
            {
                Debug.LogWarning("[Web Socket Manager] Simulating connection: no data will be received, and sends will be echoed to Log");
                return;
            }
#endif

            if (_socket == null)
            {
                Assert.IsFalse(string.IsNullOrEmpty(hostname), "Hostname must be specified!");
                //Assert.IsTrue(port >= 1000 && port <= 9999, "Port must be in the range 1000 to 9999");

                if (!hostname.StartsWith("ws"))
                {
                    Debug.LogWarning("[Web Socket Manager] Supplied hostname is missing a format specifier, adding ws (which assumes an unencrypted connection)");
                    hostname = "ws://" + hostname;
                }
                string host = hostname + (port > 0 ? ":" + port : "");
                Debug.Log("[Web Socket Manager] Creating socket for host: " + host);
                try
                {
                    _socket = new WebSocket(host);
                    _socket.OnError += Socket_OnError;
                    _socket.OnClose += Socket_OnClose;
                    _socket.OnOpen += Socket_OnOpen;
                    _socket.OnMessage += Socket_OnMessage;

                    _socket.ConnectAsync();
                }
                catch(System.Exception e)
                {
                    Debug.LogError("[Web Socket Manager] Error initialising socket: " + e.Message);
                }
            }
            else if(_socket.ReadyState != WebSocketState.Closed)
            {
                Debug.LogWarning("[Web Socket Manager] Socket is not closed! Current State: " + _socket.ReadyState);
            }
            else
            {
                try
                {
                    _socket.ConnectAsync();
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[Web Socket Manager] Error connecting to websocket: " + e.Message);
                }
            }
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            if(simulate) return;
#endif

            if (_socket == null) return;

            if(_socket.ReadyState == WebSocketState.Connecting || _socket.ReadyState == WebSocketState.Open)
            {
                try
                {
                    _socket.Close();
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[Web Socket Manager] Error closing websocket: " + e.Message);
                }
            }
        }
        
        /// <summary>
        /// Send a text message to the web socket
        /// </summary>
        /// <param name="message">The text message to send</param>
        /// <param name="callback">Optional callback action to perform on completion of send attempt</param>
        public override void SendTextMessage(string message)
        {
#if UNITY_EDITOR
            if(simulate)
            {
                Debug.Log("[Web Socket Manager] Send message: " + message);
                return;
            }
#endif
            if(_socket.ReadyState == WebSocketState.Open)
            {
                _socket.SendAsync(message, null);
            }
        }

        /// <summary>
        /// Send a binary data message to the web socket
        /// </summary>
        /// <param name="binaryData">The binary data to send</param>
        /// <param name="callback">Optional callback action to perform on completion of send attempt</param>
        public override void SendBinaryDataMessage(byte[] binaryData)
        {
#if UNITY_EDITOR
            if(simulate)
            {
                Debug.Log("[Web Socket Manager] Send binary: " + binaryData);
                return;
            }
#endif
            if (_socket.ReadyState == WebSocketState.Open)
            {
                _socket.SendAsync(binaryData, null);
            }
        }
    }
}


