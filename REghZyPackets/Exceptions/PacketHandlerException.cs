using System;
using System.Runtime.Serialization;
using REghZyPackets.Packeting;
using REghZyPackets.Systems.Handling;

namespace REghZyPackets.Exceptions {
    public class PacketHandlerException : PacketException {
        public Priority Priority { get; set; }

        public Packet Packet { get; set; }

        public PacketHandlerException() {
        }

        protected PacketHandlerException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }

        public PacketHandlerException(string message) : base(message) {
        }

        public PacketHandlerException(string message, Exception innerException) : base(message, innerException) {
        }

        public PacketHandlerException(Packet packet, Priority priority, Exception inner) : this($"Failed to handle packet of type {(packet == null ? "null" : packet.GetType().Name)}", inner){
            this.Packet = packet;
            this.Priority = priority;
        }
    }
}