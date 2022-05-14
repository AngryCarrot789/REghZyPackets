using System;
using System.Runtime.Serialization;

namespace REghZyPackets.Exceptions {
    public class PacketPayloadException : PacketException {
        public PacketPayloadException() {
        }

        protected PacketPayloadException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }

        public PacketPayloadException(string message) : base(message) {
        }

        public PacketPayloadException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}