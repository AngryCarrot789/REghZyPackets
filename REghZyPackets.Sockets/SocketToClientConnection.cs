﻿using System;
using System.Net;
using System.Net.Sockets;
using REghZy.Streams;
using REghZyPackets.Networking;
using REghZyPackets.Utils;

namespace REghZyPackets.Sockets {
    /// <summary>
    /// Represents a one-time connection to the client. When this class is instantated, it is
    /// assumed that the socket is already open. So calling <see cref="Connect"/> will do nothing
    /// <para>
    /// Calling <see cref="Disconnect"/> will fully disconenct and dispose of the socket,
    /// meaning you cannot reconnect (it will throw an exception if you try to invoke <see cref="Connect"/>,
    /// just for the sake of bug tracking)
    /// </para>
    /// </summary>
    public class SocketToClientConnection : NetworkConnection {
        private readonly Socket server;
        private readonly Socket client;
        private readonly NetworkDataStream stream;

        /// <summary>
        /// The data stream which is linked to the server
        /// </summary>
        public override DataStream Stream => this.stream;

        /// <summary>
        /// Whether this client is connected to the server
        /// </summary>
        public override bool IsConnected => !this.isDisposed;

        /// <summary>
        /// The socket that this connection is connected to
        /// </summary>
        public Socket Client => this.client;

        /// <summary>
        /// The server that this connection uses
        /// </summary>
        public Socket Server => this.server;

        public EndPoint LocalEndPoint => this.client.LocalEndPoint;

        public EndPoint RemoteEndPoint => this.client.RemoteEndPoint;

        public SocketToClientConnection(Socket client, Socket server, bool useLittleEndianness = false) {
            this.client = client;
            this.server = server;
            this.stream = useLittleEndianness ? NetworkDataStream.LittleEndianness(this.client) : NetworkDataStream.BigEndianness(this.client);
        }

        public override void Connect() {

        }

        public override void Disconnect() {
            AssertionUtils.ensureConnectionState(this.IsConnected, true);
            this.client.Disconnect(false);
            this.stream.Dispose();
            base.Dispose();
        }
    }
}
