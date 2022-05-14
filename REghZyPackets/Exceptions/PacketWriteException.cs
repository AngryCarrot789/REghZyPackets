using System;
using System.Runtime.Serialization;
using REghZyPackets.Packeting;

namespace REghZyPackets.Exceptions {
    public class PacketWriteException : PacketException {
        public int WritesAttempted { get; set; }
        public int Written { get; set; }
        public int PayloadSize { get; set; }
        public Packet Packet { get; set; }

        public PacketWriteException(int writesAttempted, int written, int payloadSize, Packet packet, Exception exception) : base($"Failed to write packet {written + 1}/{writesAttempted} for type {(packet == null ? "null" : packet.GetType().Name)}", exception) {
            this.WritesAttempted = writesAttempted;
            this.Written = written;
            this.PayloadSize = payloadSize;
            this.Packet = packet;
        }

        public PacketWriteException(string message) : base(message) {
        }

        public PacketWriteException(string message, Exception innerException) : base(message, innerException) {

        }

        protected PacketWriteException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}