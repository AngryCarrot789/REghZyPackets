using System;
using REghZyPackets.Packeting;

namespace REghZyPackets.Systems.Handling.Impl {
    public class ActionListener : IPacketHandler {
        public Action<Packet, bool> Handler { get; }

        public bool IgnoreCancelled { get; }

        public ActionListener(Action<Packet, bool> handler, bool ignoreCancelled) {
            this.Handler = handler;
            this.IgnoreCancelled = ignoreCancelled;
        }

        public bool Handle(Packet packet, bool isCancelled) {
            this.Handler(packet, isCancelled);
            return false;
        }
    }
}