using REghZyPackets.Packeting.Ack;
using REghZyPackets.Systems;
using REghZyPackets.Systems.Handling;

namespace TestPackets {
    public class AckProcessorPacketACK2 : AckProcessor<PacketACK2GetSystemName> {
        public AckProcessorPacketACK2(IPacketSystem system, Priority priority = Priority.Highest) : base(system, priority) {

        }

        protected override bool OnProcessPacketFromClient(PacketACK2GetSystemName packet) {
            SendToClient(packet, new PacketACK2GetSystemName(this.system.Name));
            return true;
        }
    }
}