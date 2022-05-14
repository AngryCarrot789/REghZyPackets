using System;
using System.Collections.Generic;
using System.IO;
using REghZy.Streams;
using REghZyPackets.Exceptions;
using REghZyPackets.Memory.Networking;
using REghZyPackets.Networking;
using REghZyPackets.Packeting;
using REghZyPackets.Systems;
using REghZyPackets.Systems.Handling;

namespace REghZyPackets.Memory {
    public class MemoryPacketSystem : IPacketSystem {
        private readonly MemoryStream stream;
        private readonly IDataInput input;
        private readonly IDataOutput output;

        private readonly Queue<Packet> sendQueue = new Queue<Packet>(8);
        private readonly Queue<Packet> readQueue = new Queue<Packet>(8);

        /// <summary>
        /// The other packet system that is paired to this one
        /// </summary>
        public MemoryPacketSystem Paired { get; set; }

        public IReadOnlyCollection<Packet> SendQueue => this.sendQueue;

        public IReadOnlyCollection<Packet> ReadQueue => this.readQueue;

        public NetworkConnection Connection { get; set; }

        public HandlerMap Handlers { get; }

        public string Name { get; set; }

        public bool IsConnected => true;

        public MemoryPacketSystem() {
            this.Handlers = new HandlerMap(this);
            this.Connection = new EmptyConnection();
            this.stream = new MemoryStream(256);
            this.input = new DataInputStream(this.stream);
            this.output = new DataOutputStream(this.stream);
        }

        /// <summary>
        /// Pairs the 2 connections, so that one can send packets to the other, and vice-versa
        /// </summary>
        public static void Pair(MemoryPacketSystem systemA, MemoryPacketSystem systemB) {
            systemA.Paired = systemB;
            systemB.Paired = systemA;
        }

        /// <summary>
        /// Pairs this system to the other system, and pairs that system to this system
        /// </summary>
        /// <param name="system"></param>
        public void Pair(MemoryPacketSystem system) {
            Pair(this, system);
        }

        public void QueuePacket(Packet packet) {
            // required because packets might do custom stuff in the read/write payload functions
            try {
                this.stream.SetLength(0);
                Packet.WritePacket(packet, this.output, true);
                this.stream.Seek(0, SeekOrigin.Begin);
                Packet read = Packet.ReadPacket(this.input);
                this.Paired.readQueue.Enqueue(read);
            }
            catch (Exception e) {
                throw new PacketException("Failed to write or read packet", e);
            }

            this.Paired.ProcessReadQueue(1);
        }

        public bool CanReadNextPacket() {
            return false;
        }

        public bool ReadNextPacket() {
            return false;
        }

        public int ProcessReadQueue(int max) {
            lock (this.readQueue) {
                int count = Math.Min(max, this.readQueue.Count);
                if (count <= 0) {
                    return 0;
                }

                HandlerMap map = this.Handlers;
                for (int i = 0; i < count; ++i) {
                    Packet packet = this.readQueue.Dequeue();
                    try {
                        map.DeliverPacket(packet);
                    }
                    catch (PacketHandlerException) {
                        throw;
                    }
                    catch (Exception e) {
                        throw new Exception($"Unexpected error while processing packet {i}/{count} of type {packet.GetType()}", e);
                    }
                }

                return count;
            }
        }

        public int ProcessSendQueue(int max) {
            return 0;
        }
    }
}