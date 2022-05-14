using REghZy.Streams;

namespace REghZyPackets.Memory.Networking {
    public class SimpleDataStream : DataStream {
        public SharedStream SharedStream { get; }

        public override long BytesAvailable => this.stream.Length;

        public SimpleDataStream(SharedStream stream) : base(stream) {
            this.SharedStream = stream;
        }
    }
}