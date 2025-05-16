namespace LinkedMovement.AltUI {
    class WindowIDManager {
        private static int NextID = 404;

        public static int GetNextID() => NextID++;
    }
}
