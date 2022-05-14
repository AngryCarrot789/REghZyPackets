using REghZy.Streams;
using REghZyPackets.Packeting;
using REghZyPackets.Packeting.Ack;

namespace TestPackets {
    [PacketImplementation(2)]
    public class PacketACK2GetSystemName : PacketACK {
        public string name;

        public PacketACK2GetSystemName() {

        }

        public PacketACK2GetSystemName(string name) {
            this.name = name;
        }

        public override int GetPayloadSizeToServer() {
            return 0;
        }

        public override int GetPayloadSizeToClient() {
            return this.name.GetSizeUTF16WL();
        }

        public override void ReadPayloadFromClient(IDataInput input, ushort length) {

        }

        public override void ReadPayloadFromServer(IDataInput input, ushort length) {
            this.name = input.ReadStringUTF16WL();
        }

        public override void WritePayloadToServer(IDataOutput output) {

        }

        public override void WritePayloadToClient(IDataOutput output) {
            output.WriteStringUTF16WL(this.name);
        }
    }
}