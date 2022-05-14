using System;
using System.IO;
using REghZy.Utils;

namespace REghZyPackets.Memory.Networking {
    /// <summary>
    /// A shared stream allows 2 streams to write into each other. Writing into streamA allows streamB to read those bytes, and vise-versa
    /// </summary>
    public class SharedStream : Stream {
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;
        public override long Length => this.bufferIndex;
        public override long Position { get => this.bufferIndex; set => this.bufferIndex = (int) value; }

        public SharedStream Stream { get; set; }

        /// <summary>
        /// The buffer containing bytes ready to be read
        /// </summary>
        private byte[] buffer;

        /// <summary>
        /// The next write index within the buffer
        /// </summary>
        private int bufferIndex;

        public SharedStream(int initialCapacity = 128) {
            this.buffer = new byte[initialCapacity];
        }

        private void EnsureCapacity(int requiredCapacity) {
            if (requiredCapacity > this.buffer.Length) {
                this.buffer = this.buffer.CopyOf(requiredCapacity);
            }
        }

        private void EnsureAdditionalCapacity(int toAdd) {
            if (this.bufferIndex + toAdd > this.buffer.Length) {
                this.buffer = this.buffer.CopyOf(this.bufferIndex + toAdd + 32);
            }
        }

        public override void Flush() {

        }

        public override int Read(byte[] buffer, int offset, int count) {
            count = Math.Min(count, this.buffer.Length);
            this.buffer = this.buffer.RemoveRange(0, count, buffer, offset);
            this.bufferIndex -= count;
            return count;
        }

        public override long Seek(long offset, SeekOrigin origin) {
            switch (origin) {
                case SeekOrigin.Begin:
                    return this.bufferIndex = (int) offset;
                case SeekOrigin.Current:
                    return this.bufferIndex += (int) offset;
                case SeekOrigin.End:
                    return this.bufferIndex -= (int) offset;
                default: throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }
        }

        public override void SetLength(long value) {
            this.buffer = this.buffer.CopyOf((int) value);
        }

        public override void Write(byte[] bytes, int offset, int count) {
            if (count < 0 || count > bytes.Length) {
                throw new ArgumentOutOfRangeException(nameof(count), $"Count is out of range. Cannot read {count} bytes from an array of size {bytes.Length}");
            }
            else if (offset < 0 || (offset + count) > bytes.Length) {
                throw new ArgumentOutOfRangeException(nameof(count), $"Offset is out of range. Cannot read {count} bytes, starting at {offset}, from an array of size {bytes.Length}");
            }

            this.Stream.AppendBytes(bytes, offset, count);
        }

        private void AppendBytes(byte[] bytes, int offset, int count) {
            EnsureAdditionalCapacity(count);
            this.buffer = this.buffer.Append(this.bufferIndex, bytes, offset, count);
            this.bufferIndex += count;
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                this.buffer = null;
                this.Stream = null;
                this.bufferIndex = -1;
            }
        }
    }
}