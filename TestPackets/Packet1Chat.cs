using REghZy.Streams;
using REghZyPackets.Packeting;

namespace TestPackets {
    [PacketImplementation(1)]
    public class Packet1Chat : Packet {
        public string message;

        public Packet1Chat() {

        }

        public Packet1Chat(string message) {
            this.message = message;
        }

        public override void ReadPayLoad(IDataInput input, ushort payloadSize) {
            this.message = input.ReadStringUTF16WL();
        }

        public override void WritePayload(IDataOutput output) {
            output.WriteStringUTF16WL(this.message);
        }

        public override int GetPayloadSize() {
            return this.message.GetSizeUTF16WL();
        }

        public override string ToString() {
            return $"{GetType().Name}({this.message ?? ""})";
        }
    }
}