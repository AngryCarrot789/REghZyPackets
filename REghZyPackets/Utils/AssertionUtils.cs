using System;
using REghZyPackets.Exceptions;

namespace REghZyPackets.Utils {
    public class AssertionUtils {
        public static void ensureNotDisposed(bool isDisposed) {
            if (isDisposed) {
                throw new ObjectDisposedException("Instance is disposed");
            }
        }

        public static void ensureConnectionState(bool connected, bool shouldBeConnected) {
            if (connected && !shouldBeConnected) {
                throw new ConnectionStatusException("Already connected!", true);
            }

            if (!connected && shouldBeConnected) {
                throw new ConnectionStatusException("Already disconnected!", false);
            }
        }
    }
}