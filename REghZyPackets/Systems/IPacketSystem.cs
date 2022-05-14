using System.Collections.Generic;
using REghZyPackets.Networking;
using REghZyPackets.Packeting;
using REghZyPackets.Systems.Handling;

namespace REghZyPackets.Systems {
    /// <summary>
    /// A network interface, that can send and receive packets
    /// </summary>
    public interface IPacketSystem {
        /// <summary>
        /// A queue of packets that are ready to be sent
        /// </summary>
        IReadOnlyCollection<Packet> SendQueue { get; }

        /// <summary>
        /// A queue of packets that have been read, and are ready to be processed
        /// </summary>
        IReadOnlyCollection<Packet> ReadQueue { get; }

        /// <summary>
        /// The connection that this packet system uses. Can be null
        /// </summary>
        NetworkConnection Connection { get; set; }

        /// <summary>
        /// A map of packet handlers
        /// </summary>
        HandlerMap Handlers { get; }

        /// <summary>
        /// A simple readable name for this packet system
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Whether the connection is available (<see cref="Connection"/> is non-null) and connected (<see cref="NetworkConnection.IsConnected"/>)
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Queues a packet (adds it to the <see cref="SendQueue"/>). It can then be sent via <see cref="ProcessSendQueue"/>
        /// </summary>
        /// <param name="packet">The non-null packet to be queued</param>
        void QueuePacket(Packet packet);

        /// <summary>
        /// Checks if a packet can be read
        /// </summary>
        /// <returns>The connection is open and there's enough available data to read the packet header</returns>
        bool CanReadNextPacket();

        /// <summary>
        /// Tries to read a packet from the connection, and adds it to the <see cref="ReadQueue"/>
        /// </summary>
        /// <returns><see cref="CanReadNextPacket"/> and a packet was successfully read</returns>
        bool ReadNextPacket();

        /// <summary>
        /// Processes packets in the <see cref="ReadQueue"/>, sending them to the packet handlers
        /// </summary>
        /// <param name="max">The max number of packets to process</param>
        /// <returns>The exact number of packets that were handled</returns>
        int ProcessReadQueue(int max);

        /// <summary>
        /// Processes packets in the <see cref="SendQueue"/>, sending them through the network
        /// </summary>
        /// <param name="max">The max number of packets to send</param>
        /// <returns>The exact number of packets that were delivered</returns>
        int ProcessSendQueue(int max);
    }
}