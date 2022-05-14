using REghZy.Streams;
using REghZyPackets.Networking;

namespace REghZyPackets.Memory.Networking {
    public class EmptyConnection : NetworkConnection {
        public override DataStream Stream { get; }

        private bool isConnected;
        public override bool IsConnected => this.isConnected;

        public override void Connect() {
            this.isConnected = true;
        }

        public override void Disconnect() {
            this.isConnected = false;
        }
    }
}