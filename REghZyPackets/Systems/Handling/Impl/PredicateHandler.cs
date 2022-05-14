using System;
using REghZyPackets.Packeting;

namespace REghZyPackets.Systems.Handling.Impl {
    public class PredicateHandler : IPacketHandler {
        public Predicate<Packet> Handler { get; }

        public bool IgnoreCancelled { get; }

        public PredicateHandler(Predicate<Packet> handler, bool ignoreCancelled) {
            this.Handler = handler;
            this.IgnoreCancelled = ignoreCancelled;
        }

        public bool Handle(Packet packet, bool isCancelled) {
            return this.Handler(packet);
        }
    }
}