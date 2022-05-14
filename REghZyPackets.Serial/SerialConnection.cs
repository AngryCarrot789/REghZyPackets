using System;
using System.IO.Ports;
using REghZy.Streams;
using REghZyPackets.Exceptions;
using REghZyPackets.Networking;
using REghZyPackets.Utils;

namespace REghZyPackets.Serial {
    public class SerialConnection : NetworkConnection {
        private SerialDataStream stream;
        private readonly SerialPort port;

        public override DataStream Stream => this.stream;

        public override bool IsConnected => this.port.IsOpen;

        /// <summary>
        /// The serial port that this serial connection uses to send/receive data
        /// </summary>
        public SerialPort Port => this.port;

        /// <summary>
        /// Whether to use little endianness or big endianness (aka the order of bytes in big data types)
        /// </summary>
        public bool UseLittleEndianness { get; set; }

        public int WriteTimeout {
            get => this.port.WriteTimeout;
            set => this.port.WriteTimeout = value;
        }

        public int ReadTimeout {
            get => this.port.ReadTimeout;
            set => this.port.ReadTimeout = value;
        }

        /// <summary>
        /// Creates a new serial connection, using the given serial parameters, with a default read/write timeout of 2000ms, and that does not discard nulls
        /// </summary>
        /// <param name="port"></param>
        /// <param name="baud"></param>
        /// <param name="parity"></param>
        /// <param name="dataBits"></param>
        /// <param name="stopBits"></param>
        public SerialConnection(string port, int baud = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One, Handshake handshake = Handshake.None) {
            this.port = new SerialPort(port, baud, parity, dataBits, stopBits);
            this.port.Handshake = handshake;
            this.port.DiscardNull = false;
            this.port.ReadTimeout = 2000;
            this.port.WriteTimeout = 2000;
            // this.port.ErrorReceived += this.Port_ErrorReceived;
            // this.port.ReadTimeout = 10000;
            // this.port.WriteTimeout = 10000;
        }

        public SerialConnection SetReadTimeout(int readTimeout) {
            this.port.ReadTimeout = readTimeout;
            return this;
        }

        public SerialConnection SetWriteTimeout(int writeTimeout) {
            this.port.WriteTimeout = writeTimeout;
            return this;
        }

        // private void Port_ErrorReceived(object sender, SerialErrorReceivedEventArgs e) {
        //     switch (e.EventType) {
        //         case SerialError.TXFull:
        //             Console.WriteLine($"[SerialError] - TXFull: Write buffer was overridden");
        //             break;
        //         case SerialError.RXOver:
        //             Console.WriteLine($"[SerialError] - RXOver: Too much data received, cannot read quick enough!");
        //             break;
        //         case SerialError.Overrun:
        //             Console.WriteLine($"[SerialError] - Overrun: The last written character was overridden... packet loss!");
        //             break;
        //         case SerialError.RXParity:
        //             Console.WriteLine($"[SerialError] - RXParity: A parity error was detected!");
        //             break;
        //         case SerialError.Frame:
        //             Console.WriteLine($"[SerialError] - Frame: A framing error was detected!");
        //             break;
        //         default:
        //             break;
        //     }
        // }

        public override void Connect() {
            AssertionUtils.ensureNotDisposed(this.isDisposed);
            AssertionUtils.ensureConnectionState(this.IsConnected, false);
            this.port.Open();
            this.port.DtrEnable = true;
            this.stream = this.UseLittleEndianness ? SerialDataStream.LittleEndianness(this.port) : SerialDataStream.BigEndianness(this.port);
            ClearBuffers();
        }

        public override void Disconnect() {
            AssertionUtils.ensureNotDisposed(this.isDisposed);
            AssertionUtils.ensureConnectionState(this.IsConnected, true);
            this.stream = null;
            this.port.DtrEnable = false;
            ClearBuffers();
            this.port.Close();
        }

        /// <summary>
        /// Clears the serial buffers
        /// </summary>
        public virtual void ClearBuffers() {
            this.port.DiscardInBuffer();
            this.port.DiscardOutBuffer();
        }

        public override void Dispose() {
            if (this.port.IsOpen) {
                ClearBuffers();
                this.port.Close();
            }

            // dispose streams first, then port
            base.Dispose();
            this.port.Dispose();
        }
    }
}