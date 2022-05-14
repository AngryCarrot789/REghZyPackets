using System;
using REghZyPackets.Packeting;

namespace REghZyPackets.Systems.Handling.Impl {
    public class FuncHandler : IPacketHandler{
        public Func<Packet, bool, bool> Handler { get; }

        public bool IgnoreCancelled { get; }

        public FuncHandler(Func<Packet, bool, bool> handler, bool ignoreCancelled) {
            this.Handler = handler;
            this.IgnoreCancelled = ignoreCancelled;
        }

        public bool Handle(Packet packet, bool isCancelled) {
            return this.Handler(packet, isCancelled);
        }
    }
}