using System;
using REghZyPackets.Memory;
using REghZyPackets.Packeting;
using REghZyPackets.Systems;

namespace TestPackets {
    internal class Program {
        public static void Main(string[] args) {
            try {
                // SharedStreamPair pair = new SharedStreamPair();
                // Packet.Setup();
                // ThreadPacketSystem systemA = new ThreadPacketSystem(new MemoryConnection(pair, true));
                // ThreadPacketSystem systemB = new ThreadPacketSystem(new MemoryConnection(pair, false));
                // AckProcessorPacketACK2 processorA = new AckProcessorPacketACK2(systemA);
                // AckProcessorPacketACK2 processorB = new AckProcessorPacketACK2(systemB);
                // systemA.Handlers.RegisterListener(OnPacketReceived);
                // systemB.Handlers.RegisterListener(OnPacketReceived);
                // systemA.OnPacketWriteError += (system, exception) => Console.WriteLine($"[A WRITE ERROR] {exception}");
                // systemB.OnPacketWriteError += (system, exception) => Console.WriteLine($"[B WRITE ERROR] {exception}");
                // systemA.OnPacketReadError += (system, exception) => Console.WriteLine($"[A READ  ERR] {exception}");
                // systemB.OnPacketReadError += (system, exception) => Console.WriteLine($"[B READ  ERR] {exception}");
                // systemA.OnReadAvailable += system => system.ProcessReadQueue(5);
                // systemB.OnReadAvailable += system => system.ProcessReadQueue(5);
                // systemA.Connection?.Connect();
                // systemB.Connection?.Connect();
                // systemA.StartThreads();
                // systemB.StartThreads();
                // Console.WriteLine("Started!");

                Packet.Setup();
                MemoryPacketSystem systemA = new MemoryPacketSystem() { Name = "System A" };
                MemoryPacketSystem systemB = new MemoryPacketSystem() { Name = "System B" };
                MemoryPacketSystem.Pair(systemA, systemB);
                AckProcessorPacketACK2 processorA = new AckProcessorPacketACK2(systemA);
                AckProcessorPacketACK2 processorB = new AckProcessorPacketACK2(systemB);
                systemA.Handlers.RegisterListener(OnPacketReceived);
                systemB.Handlers.RegisterListener(OnPacketReceived);
                systemA.Connection?.Connect();
                systemB.Connection?.Connect();
                Console.WriteLine("Started!");

                // ------------------------------------------------------------------------------
                // Console.WriteLine("Setting up packets...");
                // Packet.Setup();
                // Console.WriteLine("Creating SystemA");
                // MemoryPacketSystem systemA = new MemoryPacketSystem();
                // Console.WriteLine("Creating SystemB");
                // MemoryPacketSystem systemB = new MemoryPacketSystem();
                // MemoryPacketSystem.Pair(systemA, systemB);
                // AckProcessorPacketACK2 processorA = new AckProcessorPacketACK2(systemA);
                // AckProcessorPacketACK2 processorB = new AckProcessorPacketACK2(systemB);
                // systemA.Handlers.RegisterListener(OnPacketReceived);
                // systemB.Handlers.RegisterListener(OnPacketReceived);
                // Console.WriteLine("Connecting A to B...");
                // systemA.Connection.Connect();
                // Console.WriteLine("Connecting B to A...");
                // systemB.Connection.Connect();
                // Console.WriteLine("Connected!");
                // ------------------------------------------------------------------------------

                // systemA.SendPacket(new Packet1Chat("hi"));
                // systemA.SendPacket(new Packet1Chat("hi"));
                // systemB.SendPacket(new Packet1Chat("hi"));

                PacketACK2GetSystemName packet = processorA.MakeRequestAsync(new PacketACK2GetSystemName()).Result;
                Console.WriteLine(packet.name);

                // Task.Run(async () => {
                //     PacketACK2GetSystemName packet = await processorA.MakeRequestAsync(new PacketACK2GetSystemName());
                //     Console.WriteLine(packet.name);
                // });

                // Thread.Sleep(1000);
                // systemA.Connection.Disconnect();
                // systemB.Connection.Disconnect();
                // Thread.Sleep(5000);
                // systemA.StopThreads();
                // systemB.StopThreads();
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }

        private static void OnPacketReceived(IPacketSystem system, Packet packet, bool iscancelled) {
            Console.WriteLine($"[{system.Name}] Received {packet.GetType().Name}: {packet}");
            // system.SendPacket(packet);
        }
    }
}