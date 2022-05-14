namespace REghZyPackets.Systems.Handling {
    /// <summary>
    /// The priority for packet handlers and listeners
    /// </summary>
    public enum Priority {
        /// <summary>
        /// Highest priority; packet must be received first before everything else
        /// </summary>
        Highest = 0,

        /// <summary>
        /// High priority; packet must be received before normal handlers
        /// </summary>
        High = 1,

        /// <summary>
        /// Normal priority is the default
        /// </summary>
        Normal = 2,

        /// <summary>
        /// Low priority
        /// </summary>
        Low = 3,

        /// <summary>
        /// The lowest priority
        /// </summary>
        Lowest = 4,

        /// <summary>
        /// Used for monitoring the outcome of a packet
        /// </summary>
        Monitor = 5
    }
}