using System;
using REghZy.Streams;

namespace REghZyPackets.Networking {
    public abstract class NetworkConnection : IDisposable {
        protected bool isDisposed;

        /// <summary>
        /// The stream that this connection uses. Can be null, and (usually) will be if <see cref="IsConnected"/> returns false
        /// </summary>
        public abstract DataStream Stream { get; }

        /// <summary>
        /// Whether this connection is open and data can can be read/written
        /// </summary>
        public abstract bool IsConnected { get; }

        protected NetworkConnection() {
            this.isDisposed = false;
        }

        /// <summary>
        /// Opens the connection, allowing data to be read/written (and
        /// causing <see cref="IsConnected"/> to become true)
        /// </summary>
        public abstract void Connect();

        /// <summary>
        /// Closes the connection, therefore stopping data from being
        /// read/written (and causing <see cref="IsConnected"/> to become false)
        /// </summary>
        public abstract void Disconnect();

        public virtual void Restart() {
            if (this.IsConnected) {
                Disconnect();
            }

            Connect();
        }

        public virtual void Dispose() {
            this.Stream?.Dispose();
            this.isDisposed = true;
        }
    }
}