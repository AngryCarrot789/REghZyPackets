namespace REghZyPackets.Memory.Networking {
    public class SharedStreamPair {
        private readonly SharedStream streamA;
        private readonly SharedStream streamB;

        /// <summary>
        /// Stream A, which is already paired to Stream B
        /// </summary>
        public SharedStream StreamA => this.streamA;

        /// <summary>
        /// Stream B, which is already paired to Stream A
        /// </summary>
        public SharedStream StreamB => this.streamB;

        public SharedStreamPair(int initialCapacity = 128) {
            this.streamA = new SharedStream(initialCapacity);
            this.streamB = new SharedStream(initialCapacity);
            this.streamA.Stream = this.streamB;
            this.streamB.Stream = this.streamA;
        }
    }
}