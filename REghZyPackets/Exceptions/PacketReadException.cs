using System;
using System.Runtime.Serialization;

namespace REghZyPackets.Exceptions {
    public class PacketReadException : PacketException {
        public PacketReadException() {
        }

        protected PacketReadException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }

        public PacketReadException(string message) : base(message) {
        }

        public PacketReadException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}