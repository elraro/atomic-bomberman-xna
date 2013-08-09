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
    public interface IClientListener
    {
        void OnMessageReceived(Client client, NetworkMessageId messageId, NetIncomingMessage message);
    }

    public class Client : Peer
    {
        public IClientListener listener;

        private IPEndPoint remoteEndPoint;
        private NetConnection serverConnection;

        public Client(String name, IPEndPoint remoteEndPoint)
            : base(name, remoteEndPoint.Port)
        {
            this.remoteEndPoint = remoteEndPoint;
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
            listener.OnMessageReceived(this, messageId, message);
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
