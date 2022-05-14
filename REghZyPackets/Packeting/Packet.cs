using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using REghZy.Streams;
using REghZyPackets.Exceptions;

namespace REghZyPackets.Packeting {
    public abstract class Packet {
        private static bool Initialised = false;
        private static readonly Dictionary<ushort, Type> IdToType;
        private static readonly Dictionary<Type, ushort> TypeToId;
        private static readonly Dictionary<ushort, Func<Packet>> IdToCreator;

        /// <summary>
        /// The absolute minimum size of a packet's header, in bytes
        /// </summary>
        public const ushort MinimumHeaderSize = 2 + 2; // id + payload size

        /// <summary>
        /// The maximum size of a packet's payload, in bytes
        /// </summary>
        public const ushort MaximumPayloadSize = short.MaxValue - MinimumHeaderSize;

        public int PacketID => GetPacketId(this.GetType());

        protected Packet() {

        }

        /// <summary>
        /// Reads all of the packet's payload from the given input
        /// </summary>
        /// <param name="input">The data input to read the payload from</param>
        /// <param name="payloadSize">The payload size</param>
        public abstract void ReadPayLoad(IDataInput input, ushort payloadSize);

        /// <summary>
        /// Writes all of the packet's payload into the given data output
        /// </summary>
        /// <param name="output">The data output to write the payload to</param>
        public abstract void WritePayload(IDataOutput output);

        /// <summary>
        /// The size of this packet's payload, which can change dynamically based on the data and states
        /// </summary>
        /// <returns>A value between 0 and 32767 (short.<see cref="short.MaxValue"/>)</returns>
        public abstract int GetPayloadSize();

        #region Utils Functions

        /// <summary>
        /// Writes the packet's header and payload
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="output"></param>
        /// <param name="flush"></param>
        /// <returns>The payload size (not including header size; just add 4)</returns>
        public static int WritePacket(Packet packet, IDataOutput output, bool flush = true) {
            int payloadSize = WritePacketHeader(packet, output);
            packet.WritePayload(output);
            if (flush) {
                output.Flush();
            }

            return payloadSize;
        }

        public static Packet ReadPacket(IDataInput input) {
            ushort id = input.ReadUShort();
            if (IdToCreator.TryGetValue(id, out Func<Packet> creator)) {
                ushort size = input.ReadUShort();
                if (size >= MaximumPayloadSize) {
                    throw new PacketReadException($"Payload length ({size}) was larger than the max size ({MaximumPayloadSize})");
                }

                Packet packet = creator();
                try {
                    packet.ReadPayLoad(input, size);
                }
                catch (EndOfStreamException e) {
                    throw new PacketPayloadException($"End of stream while reading payload from packet '{packet.GetType().Name}'", e);
                }
                catch (IOException e) {
                    throw new PacketPayloadException($"I/O Exception while reading payload from packet '{packet.GetType().Name}'", e);
                }
                catch (Exception e) {
                    throw new PacketPayloadException($"Failed to read payload from packet '{packet.GetType().Name}'", e);
                }

                return packet;
            }
            else {
                throw new PacketReadException($"Unknown packet for ID {id}");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int WritePacketHeader(Packet packet, IDataOutput output) {
            int payload = packet.GetPayloadSize();
            if (payload < 0 || payload >= MaximumPayloadSize) {
                throw new PacketWriteException($"Payload length ({payload}) was larger than the max size ({MaximumPayloadSize})");
            }

            if (TypeToId.TryGetValue(packet.GetType(), out ushort id)) {
                output.WriteUShort(id);
            }
            else {
                throw new PacketWriteException($"{packet.GetType()} was not registered");
            }

            output.WriteUShort((ushort) payload);
            return payload;
        }

        public static Packet CreateInstance(ushort id) {
            if (IdToCreator.TryGetValue(id, out Func<Packet> creator)) {
                return creator();
            }

            throw new Exception($"Unknown packet with ID {id} ");
        }

        public static T CreateInstance<T>() where T : Packet {
            return (T) CreateInstance(typeof(T));
        }

        public static Packet CreateInstance(Type type) {
            if (TypeToId.TryGetValue(type, out ushort id)) {
                return CreateInstance(id);
            }

            throw new Exception($"Unknown packet type {type} ");
        }

        #endregion

        #region Setup functions

        public static void Register(Type type, ushort id) {
            Register(type, id, null);
        }

        public static void Register(Type type, ushort id, Func<Packet> creator) {
            if (IdToType.ContainsKey(id) && TypeToId.ContainsKey(type)) {
                throw new InvalidOperationException($"{type} was already registered with ID {id}");
            }
            else if (TypeToId.ContainsKey(type)) {
                throw new InvalidOperationException($"{type} was already registered. It is registered with ID {TypeToId[type]}");
            }
            else if (IdToType.ContainsKey(id)) {
                throw new InvalidOperationException($"ID {id} was already registered with type {IdToType[id]}");
            }

            if (!typeof(Packet).IsAssignableFrom(type)) {
                throw new ArgumentException($"Type {type} is not assignable to Packet", nameof(type));
            }

            if (creator == null) {
                creator = () => (Packet) Activator.CreateInstance(type);
            }

            IdToType[id] = type;
            TypeToId[type] = id;
            IdToCreator[id] = creator;
        }

        public static bool IsRegistered<T>() where T : Packet {
            return IsRegistered(typeof(T));
        }

        public static bool IsRegistered(Type type) {
            return TypeToId.ContainsKey(type);
        }

        public static bool IsRegistered(int packetId) {
            return IdToType.ContainsKey((ushort) packetId);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetPacketId<T>() where T : Packet {
            return GetPacketId(typeof(T));
        }

        public static int GetPacketId(Type type) {
            return TypeToId.TryGetValue(type, out ushort id) ? id : -1;
        }

        /// <summary>
        /// Tries to automatically register a packet based on it's packet implementation annotation
        /// </summary>
        /// <typeparam name="T">The packet type</typeparam>
        public static void AutoRegister<T>() where T : Packet {
            AutoRegister(typeof(T));
        }

        /// <summary>
        /// Tries to automatically register a packet based on it's packet implementation annotation
        /// </summary>
        /// <typeparam name="T">The packet type</typeparam>
        public static void AutoRegister(Type type) {
            if (!typeof(Packet).IsAssignableFrom(type)) {
                throw new ArgumentException("The type is not a packet type", nameof(type));
            }

            PacketImplementation implementation = type.GetCustomAttribute<PacketImplementation>(false);
            if (implementation != null) {
                if (implementation.UseAutoRegister) {
                    implementation.TryRegister(type);
                }
                else {
                    RuntimeHelpers.RunClassConstructor(type.TypeHandle);
                }
            }
        }

        public static void Setup() {
            if (Initialised) {
                return;
            }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                foreach (Type type in assembly.GetTypes().Where(t => typeof(Packet).IsAssignableFrom(t))) {
                    AutoRegister(type);
                }
            }

            Initialised = true;
        }

        static Packet() {
            Packet.IdToType = new Dictionary<ushort, Type>();
            Packet.TypeToId = new Dictionary<Type, ushort>();
            Packet.IdToCreator = new Dictionary<ushort, Func<Packet>>();
            Setup();
        }

        #endregion
    }
}