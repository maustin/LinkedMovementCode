using System.Collections.Generic;
using UnityEngine;

namespace LinkedMovement.UI.Utils {
    static class Texture2DPool {
        private static int maxTextures = 50;
        private static List<Texture2D> textures;
        private static int currentIndex = 0;

        static Texture2DPool() {
            textures = new List<Texture2D>();
            currentIndex = 0;
        }

        public static Texture2D GetTexture() {
            if (textures.Count == currentIndex) {
                textures.Add(new Texture2D(1, 1));
            }
            var texture = textures[currentIndex];

            IncrementIndex();
            return texture;
        }

        private static void IncrementIndex() {
            currentIndex++;
            if (currentIndex == maxTextures) {
                currentIndex = 0;
            }
        }
    }
}
