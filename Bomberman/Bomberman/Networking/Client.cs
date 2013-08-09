﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using System.Net;
using BomberEngine.Debugging;
using BomberEngine.Core.IO;
using Bomberman.Multiplayer;

namespace Bomberman.Networking
{
    using ClientReceivedMessageDelegate     = ReceivedMessageDelegate<Client>;
    using ClientReceivedMessageDelegateList = ReceivedMessageDelegateList<Client>;

    public class Client : Peer
    {
        private IPEndPoint remoteEndPoint;
        private NetConnection serverConnection;
        private IDictionary<NetworkMessageId, ClientReceivedMessageDelegateList> m_delegatesMap;

        public Client(String name, IPEndPoint remoteEndPoint)
            : base(name, remoteEndPoint.Port)
        {
            this.remoteEndPoint = remoteEndPoint;
            m_delegatesMap = new Dictionary<NetworkMessageId, ClientReceivedMessageDelegateList>();
        }

        //////////////////////////////////////////////////////////////////////////////

        #region Lifecycle

        public override void Start()
        {
            if (peer != null)
            {
                throw new InvalidOperationException("Client already running");
            }

            NetPeerConfiguration config = new NetPeerConfiguration(name);

            peer = new NetClient(config);
            peer.Start();

            NetOutgoingMessage hailMessage = peer.CreateMessage();
            hailMessage.Write(CVars.name.value);
            peer.Connect(remoteEndPoint, hailMessage);
        }

        public override void Stop()
        {
            if (peer != null)
            {
                peer.Shutdown("disconnect");
                peer = null;
                serverConnection = null;
            }
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////

        #region Inheritance

        protected override void OnPeerConnected(NetConnection connection)
        {
            Log.i("Connected to the server: " + connection.RemoteEndPoint);
            Debug.Assert(serverConnection == null);
            serverConnection = connection;

            PostNotification(NetworkNotifications.ConnectedToServer);
        }

        protected override void OnPeerDisconnected(NetConnection connection)
        {
            Log.i("Disconnected from the server: " + connection.RemoteEndPoint);
            Debug.Assert(serverConnection == connection);

            serverConnection = null;

            PostNotification(NetworkNotifications.DisconnectedFromServer);
        }

        protected override void OnMessageReceive(NetworkMessageId messageId, NetIncomingMessage message)
        {
            NotifyMessageReceived(messageId, message);
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////

        #region Message delegates

        public void AddMessageDelegate(NetworkMessageId messageId, ClientReceivedMessageDelegate del)
        {
            ClientReceivedMessageDelegateList list = FindList(messageId);
            if (list == null)
            {
                list = new ClientReceivedMessageDelegateList();
                m_delegatesMap[messageId] = list;
            }
            list.Add(del);
        }

        public void RemoveMessageDelegate(NetworkMessageId messageId, ClientReceivedMessageDelegate del)
        {
            ClientReceivedMessageDelegateList list = FindList(messageId);
            if (list != null)
            {
                list.Remove(del);
            }
        }

        public void RemoveDelegates(Object target)
        {
            foreach (KeyValuePair<NetworkMessageId, ClientReceivedMessageDelegateList> e in m_delegatesMap)
            {
                ClientReceivedMessageDelegateList list = e.Value;
                list.RemoveAll(target);
            }
        }

        private void NotifyMessageReceived(NetworkMessageId messageId, NetIncomingMessage message)
        {
            ClientReceivedMessageDelegateList list = FindList(messageId);
            if (list != null)
            {
                list.NotifyMessageReceived(this, messageId, message);
            }
        }

        private ClientReceivedMessageDelegateList FindList(NetworkMessageId messageId)
        {
            ClientReceivedMessageDelegateList list;
            if (m_delegatesMap.TryGetValue(messageId, out list))
            {
                return list;
            }
            return null;
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////

        #region Messages

        public override void SendMessage(NetOutgoingMessage message, NetDeliveryMethod method = NetDeliveryMethod.Unreliable)
        {
            SendMessage(message, serverConnection, method);
        }

        public override void SendMessage(NetworkMessageId messageId, NetDeliveryMethod method = NetDeliveryMethod.Unreliable)
        {   
            SendMessage(messageId, serverConnection, method);
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////

        #region Properties

        public NetConnection GetServerConnection()
        {
            return serverConnection;
        }

        #endregion
    }
}
