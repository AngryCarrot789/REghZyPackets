using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using REghZyPackets.Exceptions;
using REghZyPackets.Packeting;
using REghZyPackets.Systems.Handling.Impl;

namespace REghZyPackets.Systems.Handling {
    public class HandlerMap {
        private readonly IPacketSystem system;
        private readonly List<IPacketHandler>[] handlers;

        public IPacketSystem System => this.system;

        public HandlerMap(IPacketSystem system) {
            this.system = system;
            this.handlers = new List<IPacketHandler>[6];
            this.handlers[(int) Priority.Highest] = new List<IPacketHandler>();
            this.handlers[(int) Priority.High] = new List<IPacketHandler>();
            this.handlers[(int) Priority.Normal] = new List<IPacketHandler>();
            this.handlers[(int) Priority.Low] = new List<IPacketHandler>();
            this.handlers[(int) Priority.Lowest] = new List<IPacketHandler>();
            this.handlers[(int) Priority.Monitor] = new List<IPacketHandler>();
        }

        public void ClearHandlers() {
            foreach (List<IPacketHandler> collection in this.handlers) {
                collection.Clear();
            }
        }

        public IPacketHandler AddHandler(Priority priority, IPacketHandler handler) {
            GetHandlers(priority).Add(handler);
            return handler;
        }

        public IPacketHandler RegisterListener(Action<Packet> callback, Priority priority = Priority.Normal, bool ignoreCancelled = false) {
            return AddHandler(priority, new ActionListener((p, isCancelled) => callback(p), ignoreCancelled));
        }

        public IPacketHandler RegisterListener(Action<Packet, bool> callback, Priority priority = Priority.Normal, bool ignoreCancelled = false) {
            return AddHandler(priority, new ActionListener(callback, ignoreCancelled));
        }

        public IPacketHandler RegisterListener(Action<IPacketSystem, Packet> callback, Priority priority = Priority.Normal, bool ignoreCancelled = false) {
            return AddHandler(priority, new ActionListener((p, isCancelled) => callback(this.system, p), ignoreCancelled));
        }

        public IPacketHandler RegisterListener(Action<IPacketSystem, Packet, bool> callback, Priority priority = Priority.Normal, bool ignoreCancelled = false) {
            return AddHandler(priority, new ActionListener((p, i) => callback(this.system, p, i), ignoreCancelled));
        }

        public IPacketHandler RegisterHandler(Predicate<Packet> callback, Priority priority = Priority.Normal, bool ignoreCancelled = false) {
            return AddHandler(priority, new PredicateHandler(callback, ignoreCancelled));
        }

        public IPacketHandler RegisterHandler(DelegateHandler.DelegatedHandler callback, Priority priority = Priority.Normal, bool ignoreCancelled = false) {
            return AddHandler(priority, new DelegateHandler(callback, ignoreCancelled));
        }

        public IPacketHandler RegisterListener(Func<Packet, bool, bool> callback, Priority priority = Priority.Normal, bool ignoreCancelled = false) {
            return AddHandler(priority, new FuncHandler(callback, ignoreCancelled));
        }

        public IPacketHandler RegisterListener(Func<Packet, bool> callback, Priority priority = Priority.Normal, bool ignoreCancelled = false) {
            return AddHandler(priority, new FuncHandler((p, isCancelled) => callback(p), ignoreCancelled));
        }

        public IPacketHandler RegisterListener(Func<IPacketSystem, Packet, bool, bool> callback, Priority priority = Priority.Normal, bool ignoreCancelled = false) {
            return AddHandler(priority, new FuncHandler((p, i) => callback(this.system, p, i), ignoreCancelled));
        }

        public IPacketHandler RegisterListener(Func<IPacketSystem, Packet, bool> callback, Priority priority = Priority.Normal, bool ignoreCancelled = false) {
            return AddHandler(priority, new FuncHandler((p, isCancelled) => callback(this.system, p), ignoreCancelled));
        }

        public IPacketHandler RegisterListener<T>(Action<T> callback, Priority priority = Priority.Normal, bool ignoreCancelled = false) where T : Packet {
            return AddHandler(priority, new ActionListener((p, isCancelled) => { if (p is T t) callback(t); }, ignoreCancelled));
        }

        public IPacketHandler RegisterListener<T>(Action<T, bool> callback, Priority priority = Priority.Normal, bool ignoreCancelled = false) {
            return AddHandler(priority, new ActionListener((p, b) => { if (p is T t) callback(t, b); }, ignoreCancelled));
        }

        public IPacketHandler RegisterListener<T>(Action<IPacketSystem, T> callback, Priority priority = Priority.Normal, bool ignoreCancelled = false) {
            return AddHandler(priority, new ActionListener((p, isCancelled) => { if (p is T t) callback(this.system, t); }, ignoreCancelled));
        }

        public IPacketHandler RegisterListener<T>(Action<IPacketSystem, T, bool> callback, Priority priority = Priority.Normal, bool ignoreCancelled = false) where T : Packet {
            return AddHandler(priority, new ActionListener((p, i) => { if (p is T t) callback(this.system, t, i); }, ignoreCancelled));
        }

        public IPacketHandler RegisterHandler<T>(Predicate<T> callback, Priority priority = Priority.Normal, bool ignoreCancelled = false) where T : Packet {
            return AddHandler(priority, new PredicateHandler((p) => p is T t && callback(t), ignoreCancelled));
        }

        public IPacketHandler RegisterListener<T>(Func<T, bool, bool> callback, Priority priority = Priority.Normal, bool ignoreCancelled = false) where T : Packet {
            return AddHandler(priority, new FuncHandler((p, a) => p is T t && callback(t, a), ignoreCancelled));
        }

        public IPacketHandler RegisterListener<T>(Func<T, bool> callback, Priority priority = Priority.Normal, bool ignoreCancelled = false) where T : Packet {
            return AddHandler(priority, new FuncHandler((p, isCancelled) => p is T t && callback(t), ignoreCancelled));
        }

        public IPacketHandler RegisterListener<T>(Func<IPacketSystem, T, bool, bool> callback, Priority priority = Priority.Normal, bool ignoreCancelled = false) where T : Packet {
            return AddHandler(priority, new FuncHandler((p, i) => p is T t && callback(this.system, t, i), ignoreCancelled));
        }

        public IPacketHandler RegisterListener<T>(Func<IPacketSystem, T, bool> callback, Priority priority = Priority.Normal, bool ignoreCancelled = false) where T : Packet {
            return AddHandler(priority, new FuncHandler((p, isCancelled) => p is T t && callback(this.system, t), ignoreCancelled));
        }








        public bool UnregisterHandler(IPacketHandler handler, Priority priority) {
            return GetHandlers(priority).Remove(handler);
        }

        public List<IPacketHandler> GetHandlers(Priority priority) {
            int index = (int) priority;
            if (index < ((int) Priority.Highest) || index > ((int) Priority.Monitor)) {
                throw new Exception("Unknown priority: " + priority);
            }

            return this.handlers[index];
        }

        /// <summary>
        /// Delivers the given packet to all of the listeners and handlers, respecting their priority
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public bool DeliverPacket(Packet packet) {
            bool isCancelled = false;
            HandlePriority(Priority.Highest, packet, ref isCancelled);
            HandlePriority(Priority.High, packet, ref isCancelled);
            HandlePriority(Priority.Normal, packet, ref isCancelled);
            HandlePriority(Priority.Low, packet, ref isCancelled);
            HandlePriority(Priority.Lowest, packet, ref isCancelled);
            HandlePriority(Priority.Monitor, packet, ref isCancelled);
            return isCancelled;
        }

        private void HandlePriority(Priority priority, Packet packet, ref bool isCancelled) {
            foreach (IPacketHandler handler in this.handlers[(int) priority]) {
                if (isCancelled && !handler.IgnoreCancelled) {
                    continue;
                }

                try {
                    if (handler.Handle(packet, isCancelled)) {
                        isCancelled = true;
                    }
                }
                catch (Exception e) {
                    throw new PacketHandlerException(packet, priority, e);
                }
            }
        }
    }
}