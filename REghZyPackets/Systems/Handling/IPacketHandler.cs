using REghZyPackets.Packeting;

namespace REghZyPackets.Systems.Handling {
    /// <summary>
    /// A packet handler, used for essentially stopping packets from being received by other handlers of lower priority
    /// </summary>
    public interface IPacketHandler {
        /// <summary>
        /// Whether this handler should be able to handle cancelled packet
        /// </summary>
        bool IgnoreCancelled { get; }

        /// <summary>
        /// Handles the packet. <see cref="IgnoreCancelled"/> will already be checked before this is invoked
        /// </summary>
        /// <param name="packet">The packet to handle</param>
        /// <param name="isCancelled">A reference to say whether or not the packet is cancelled</param>
        /// <returns>Whether the packet should be cancelled or not</returns>
        bool Handle(Packet packet, bool isCancelled);
    }
}