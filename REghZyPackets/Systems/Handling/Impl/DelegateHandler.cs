using REghZyPackets.Packeting;

namespace REghZyPackets.Systems.Handling.Impl {
    public class DelegateHandler : IPacketHandler {
        public delegate void DelegatedHandler(Packet packet, bool isCancelled, ref bool cancel);

        public DelegatedHandler Handler { get; }

        public bool IgnoreCancelled { get; }

        public DelegateHandler(DelegatedHandler handler, bool ignoreCancelled) {
            this.Handler = handler;
            this.IgnoreCancelled = ignoreCancelled;
        }

        public bool Handle(Packet packet, bool isCancelled) {
            bool cancel = false;
            this.Handler(packet, isCancelled, ref cancel);
            return cancel;
        }
    }
}