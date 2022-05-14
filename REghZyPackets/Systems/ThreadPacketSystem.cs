using System;
using System.Runtime.CompilerServices;
using System.Threading;
using REghZyPackets.Exceptions;
using REghZyPackets.Networking;
using REghZyPackets.Systems.Handling;

namespace REghZyPackets.Systems {
    public class ThreadPacketSystem : PacketSystem, IDisposable {
        public volatile bool debugRead = false;
        public volatile bool debugWrite = false;

        private static int NEXT_READ;
        private static int NEXT_WRITE;

        private Thread readThread;
        private Thread writeThread;

        private volatile bool stopped;

        private volatile bool canRunRead;
        private volatile bool canRunWrite;

        private volatile bool isReadPaused;
        private volatile bool isWritePaused;

        private volatile int readCount;
        private volatile int writeCount;

        public Thread ReadThread => this.readThread;
        public Thread WriteThread => this.writeThread;

        public bool IsReadThreadRunning => this.readThread.ThreadState == ThreadState.Running;
        public bool IsWriteThreadRunning => this.writeThread.ThreadState == ThreadState.Running;

        public bool IsReadPaused {
            get => this.isReadPaused;
            set => this.isReadPaused = value;
            // set {
            //     lock (this.ReadQueue) {
            //         this.isReadPaused = value;
            //     }
            // }
        }

        public bool IsWritePaused {
            get => this.isWritePaused;
            set => this.isWritePaused = value;
            // set {
            //     lock (this.SendQueue) {
            //         this.isWritePaused = value;
            //     }
            // }
        }

        public bool IsFullyPaused {
            get => this.isReadPaused && this.isWritePaused;
            set => this.IsReadPaused = this.isWritePaused = value;
        }

        public bool IsStopped => this.stopped;

        public int ReadCount => this.readCount;
        public int WriteCount => this.writeCount;

        public delegate void PacketReadFail(ThreadPacketSystem system, PacketReadException e);
        public delegate void PacketWriteFail(ThreadPacketSystem system, PacketWriteException e);
        public delegate void ReadAvailable(ThreadPacketSystem system);

        /// <summary>
        /// Called when an exception was thrown while reading a packet from the connection
        /// <para>
        /// This will be invoked from the read thread, therefore you must ensure your code is thread safe!
        /// </para>
        /// </summary>
        public event PacketReadFail OnPacketReadError;

        /// <summary>
        /// Called when an exception was thrown while writing a packet to the connection
        /// <para>
        /// This will be invoked from the write thread, therefore you must ensure your code is thread safe!
        /// </para>
        /// </summary>
        public event PacketWriteFail OnPacketWriteError;

        /// <summary>
        /// Called when there are packets available to be read in <see cref="PacketSystem.ReadQueue"/>
        /// <para>
        /// This event will be called from another thread, therefore you must ensure your code is thread safe!
        /// </para>
        /// </summary>
        public event ReadAvailable OnReadAvailable;

        public ThreadPacketSystem(NetworkConnection connection) : base(connection) {
            this.readThread = new Thread(ReadMain);
            this.readThread.Name = $"REghZy Packet Read Thread {++NEXT_READ}";

            this.writeThread = new Thread(WriteMain);
            this.writeThread.Name = $"REghZy Packet Write Thread {++NEXT_WRITE}";
        }

        public ThreadPacketSystem() : this(null) {

        }

        public void StartThreads() {
            if (this.stopped) {
                throw new InvalidOperationException("Already stopped. Cannot restart");
            }
            else if (this.IsReadThreadRunning || this.IsWriteThreadRunning) {
                throw new InvalidOperationException($"{(this.IsWriteThreadRunning ? "Write" : "Read")} thread already running. It should be paused, not . Cannot restart");
            }

            try {
                this.canRunRead = true;
                this.readThread.Start();
            }
            finally {
                this.canRunRead = this.readThread.ThreadState == ThreadState.Running;
            }

            try {
                this.canRunWrite = true;
                this.writeThread.Start();
            }
            finally {
                this.canRunWrite = this.writeThread.ThreadState == ThreadState.Running;
            }
        }

        public void PauseThreads() {
            this.IsFullyPaused = !this.IsFullyPaused;
        }

        public (bool, bool) StopThreads() {
            if (this.stopped) {
                throw new Exception("Already stopped");
            }

            this.stopped = true;
            this.canRunRead = false;
            this.canRunWrite = false;
            return (this.readThread.Join(5000), this.writeThread.Join(5000));
        }

        private void ReadMain() {
            while (this.canRunRead) {
                while (true) {
                    if (this.isReadPaused || !CanReadNextPacket()) {
                        break;
                    }

                    bool read = false;
                    try {
                        lock (this.ReadQueue) {
                            read = ReadNextPacketUnchecked(this.Connection.Stream.Input);
                        }
                    }
                    catch (PacketReadException e) {
                        if (this.debugRead) {
                            throw;
                        }
                        else {
                            this.OnPacketReadError?.Invoke(this, e);
                        }
                    }

                    if (read) {
                        this.readCount += 1;
                        this.OnReadAvailable?.Invoke(this);
                    }
                    else {
                        SpinWaitReaderForInterval();
                    }
                }

                SpinWaitReaderWhilePaused();
            }
        }

        private void WriteMain() {
            while (this.canRunWrite) {
                while (true) {
                    if (this.isWritePaused || !this.IsConnected) {
                        break;
                    }

                    int written = 0;
                    try {
                        written = ProcessSendQueue(5);
                    }
                    catch (PacketWriteException e) {
                        if (this.debugWrite) {
                            throw;
                        }
                        else {
                            this.OnPacketWriteError?.Invoke(this, e);
                        }
                    }

                    if (written == 0) {
                        SpinWaitWriterForInterval();
                    }
                    else {
                        this.writeCount += written;
                    }
                }

                SpinWaitWriterWhilePaused();
            }
        }

        public virtual void SpinWaitReaderForInterval() {
            Thread.Sleep(1);
        }

        public virtual void SpinWaitWriterForInterval() {
            Thread.Sleep(1);
        }

        public virtual void SpinWaitReaderWhilePaused() {
            Thread.Sleep(10);
        }

        public virtual void SpinWaitWriterWhilePaused() {
            Thread.Sleep(10);
        }

        public void Dispose() {
            StopThreads();
        }
    }
}