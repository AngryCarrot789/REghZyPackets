using System;
using REghZy.Streams;
using REghZyPackets.Networking;
using REghZyPackets.Utils;

namespace REghZyPackets.Memory.Networking {
    /// <summary>
    /// A network connection that uses a pair of shared streams
    /// </summary>
    public class MemoryConnection : NetworkConnection {
        private readonly bool useA;
        private readonly SharedStreamPair pair;
        private SimpleDataStream stream;
        private bool isConnected;

        public override DataStream Stream => this.stream;

        public override bool IsConnected => this.isConnected;

        public MemoryConnection(SharedStreamPair pair, bool useA) {
            this.pair = pair;
            this.useA = useA;
        }

        public override void Connect() {
            AssertionUtils.ensureNotDisposed(this.isDisposed);
            AssertionUtils.ensureConnectionState(this.IsConnected, false);
            this.stream = new SimpleDataStream(this.useA ? this.pair.StreamA : this.pair.StreamB);
            this.isConnected = true;
        }

        public override void Disconnect() {
            AssertionUtils.ensureNotDisposed(this.isDisposed);
            AssertionUtils.ensureConnectionState(this.IsConnected, true);
            this.stream = null;
            this.isConnected = false;
        }

        public override void Dispose() {
            base.Dispose();
            Disconnect();
        }
    }
}