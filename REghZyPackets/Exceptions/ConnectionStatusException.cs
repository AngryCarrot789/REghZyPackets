using System;
using System.Runtime.Serialization;

namespace REghZyPackets.Exceptions {
    public class ConnectionStatusException : Exception {
        public bool IsConnected { get; }

        public ConnectionStatusException(bool isConnected) {
            this.IsConnected = isConnected;
        }

        protected ConnectionStatusException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }

        public ConnectionStatusException(string message, bool isConnected) : base(message) {
            this.IsConnected = isConnected;
        }

        public ConnectionStatusException(string message, Exception innerException, bool isConnected) : base(message, innerException) {
            this.IsConnected = isConnected;
        }
    }
}