using System;
using System.Collections.Generic;
using System.Threading;
using REghZy.Streams;
using REghZyPackets.Exceptions;
using REghZyPackets.Networking;
using REghZyPackets.Packeting;
using REghZyPackets.Systems.Handling;

namespace REghZyPackets.Systems {
    public class PacketSystem : IPacketSystem {
        protected Queue<Packet> sendQueue;
        protected Queue<Packet> readQueue;

        private static int NEXT_ID = 0;

        public IReadOnlyCollection<Packet> SendQueue => this.sendQueue;

        public IReadOnlyCollection<Packet> ReadQueue => this.readQueue;

        public NetworkConnection Connection { get; set; }

        public HandlerMap Handlers { get; protected set; }

        public string Name { get; set; }

        public bool IsConnected => this.Connection != null && this.Connection.IsConnected;

        public PacketSystem(NetworkConnection connection) : this() {
            this.Connection = connection;
            this.Handlers = new HandlerMap(this);
        }

        public PacketSystem() {
            this.sendQueue = new Queue<Packet>(128);
            this.readQueue = new Queue<Packet>(128);
            this.Name = $"PacketSystem {++NEXT_ID}";
        }

        public void QueuePacket(Packet packet) {
            this.sendQueue.Enqueue(packet);
        }

        public void SendPacketImmidiately(Packet packet) {
            Packet.WritePacket(packet, this.Connection.Stream.Output);
        }

        public bool CanReadNextPacket() {
            return this.IsConnected && this.Connection.Stream != null && this.Connection.Stream.BytesAvailable >= Packet.MinimumHeaderSize;
        }

        public bool ReadNextPacket() {
            return CanReadNextPacket() && ReadNextPacketUnchecked(this.Connection.Stream.Input);
        }

        public bool ReadNextPacketUnchecked(IDataInput input) {
            bool read = false;
            Packet packet;
            try {
                packet = Packet.ReadPacket(input);
                read = true;
            }
            catch (Exception e) {
                throw new PacketReadException("Failed to read next packet", e);
            }
            finally {
                #if DEBUG
                Console.WriteLine($"{(read ? "Successfully" : "Failed to")} read next packet");
                #endif
            }

            this.readQueue.Enqueue(packet);
            return read;
        }

        public int ProcessReadQueue(int max) {
            bool isLocked = false;
            Queue<Packet> queue = this.readQueue;
            try {
                Monitor.Enter(queue, ref isLocked);
                int count = Math.Min(max, queue.Count);
                if (count <= 0) {
                    return 0;
                }

                HandlerMap map = this.Handlers;
                for (int i = 0; i < count; ++i) {
                    Packet packet = queue.Dequeue();
                    try {
                        map.DeliverPacket(packet);
                    }
                    catch (PacketHandlerException) {
                        throw;
                    }
                    catch (Exception e) {
                        throw new Exception($"Unexpected error while processing packet {i}/{count}", e);
                    }
                }

                return count;
            }
            finally {
                if (isLocked) {
                    Monitor.Exit(queue);
                }
            }
        }

        public int ProcessSendQueue(int max) {
            bool isLocked = false;
            Queue<Packet> queue = this.sendQueue;
            try {
                Monitor.Enter(queue, ref isLocked);
                int count = Math.Min(max, queue.Count);
                if (count <= 0) {
                    return 0;
                }

                IDataOutput output = this.Connection.Stream.Output;
                for (int i = 0; i < count; ++i) {
                    int payload = -1;
                    Packet packet = queue.Dequeue();
                    try {
                        payload = Packet.WritePacket(packet, output, true);
                    }
                    catch (Exception e) {
                        throw new PacketWriteException(count, i, payload, packet, e);
                    }

                    #if DEBUG
                    Console.WriteLine($"Wrote {packet.GetType().Name} of size {payload} (+ 4 for header = {payload + 4})");
                    #endif
                }

                return count;
            }
            finally {
                if (isLocked) {
                    Monitor.Exit(queue);
                }
            }
        }

        public override string ToString() {
            return $"{GetType().Name}('{this.Name}')";
        }
    }
}