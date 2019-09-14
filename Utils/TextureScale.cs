
using System.Threading;
using UnityEngine;
namespace surfm.tool.realtimedb {
    public class TextureScale {
        public class ThreadData {
            public int start;
            public int end;
            public ThreadData(int s, int e) {
                start = s;
                end = e;
            }
        }

        private static Color[] texColors;
        private static Color[] newColors;
        private static int w;
        private static float ratioX;
        private static float ratioY;
        private static int w2;
        private static int finishCount;
        private static Mutex mutex;

        public static void Point(Texture2D tex, int newWidth, int newHeight) {
            ThreadedScale(tex, newWidth, newHeight, false);
        }

        public static void Bilinear(Texture2D tex, int newWidth, int newHeight) {
            ThreadedScale(tex, newWidth, newHeight, true);
        }

        private static void ThreadedScale(Texture2D tex, int newWidth, int newHeight, bool useBilinear) {
            texColors = tex.GetPixels();
            newColors = new Color[newWidth * newHeight];
            if (useBilinear) {
                ratioX = 1.0f / ((float)newWidth / (tex.width - 1));
                ratioY = 1.0f / ((float)newHeight / (tex.height - 1));
            } else {
                ratioX = ((float)tex.width) / newWidth;
                ratioY = ((float)tex.height) / newHeight;
            }
            w = tex.width;
            w2 = newWidth;
            var cores = Mathf.Min(SystemInfo.processorCount, newHeight);
            var slice = newHeight / cores;

            finishCount = 0;
            if (mutex == null) {
                mutex = new Mutex(false);
            }
            if (cores > 1) {
                int i = 0;
                ThreadData threadData;
                for (i = 0; i < cores - 1; i++) {
                    threadData = new ThreadData(slice * i, slice * (i + 1));
                    ParameterizedThreadStart ts = useBilinear ? new ParameterizedThreadStart(BilinearScale) : new ParameterizedThreadStart(PointScale);
                    Thread thread = new Thread(ts);
                    thread.Start(threadData);
                }
                threadData = new ThreadData(slice * i, newHeight);
                if (useBilinear) {
                    BilinearScale(threadData);
                } else {
                    PointScale(threadData);
                }
                while (finishCount < cores) {
                    Thread.Sleep(1);
                }
            } else {
                ThreadData threadData = new ThreadData(0, newHeight);
                if (useBilinear) {
                    BilinearScale(threadData);
                } else {
                    PointScale(threadData);
                }
            }

            tex.Resize(newWidth, newHeight);
            tex.SetPixels(newColors);
            tex.Apply();

            texColors = null;
            newColors = null;
        }

        public static void BilinearScale(System.Object obj) {
            ThreadData threadData = (ThreadData)obj;
            for (var y = threadData.start; y < threadData.end; y++) {
                int yFloor = (int)Mathf.Floor(y * ratioY);
                var y1 = yFloor * w;
                var y2 = (yFloor + 1) * w;
                var yw = y * w2;

                for (var x = 0; x < w2; x++) {
                    int xFloor = (int)Mathf.Floor(x * ratioX);
                    var xLerp = x * ratioX - xFloor;
                    newColors[yw + x] = ColorLerpUnclamped(ColorLerpUnclamped(texColors[y1 + xFloor], texColors[y1 + xFloor + 1], xLerp),
                                                           ColorLerpUnclamped(texColors[y2 + xFloor], texColors[y2 + xFloor + 1], xLerp),
                                                           y * ratioY - yFloor);
                }
            }

            mutex.WaitOne();
            finishCount++;
            mutex.ReleaseMutex();
        }

        public static void PointScale(System.Object obj) {
            ThreadData threadData = (ThreadData)obj;
            for (var y = threadData.start; y < threadData.end; y++) {
                var thisY = (int)(ratioY * y) * w;
                var yw = y * w2;
                for (var x = 0; x < w2; x++) {
                    newColors[yw + x] = texColors[(int)(thisY + ratioX * x)];
                }
            }

            mutex.WaitOne();
            finishCount++;
            mutex.ReleaseMutex();
        }

        private static Color ColorLerpUnclamped(Color c1, Color c2, float value) {
            return new Color(c1.r + (c2.r - c1.r) * value,
                              c1.g + (c2.g - c1.g) * value,
                              c1.b + (c2.b - c1.b) * value,
                              c1.a + (c2.a - c1.a) * value);
        }

        public static Vector2Int fitScaled(Vector2Int imgSize, Vector2Int boundary) {

            int original_width = imgSize.x;
            int original_height = imgSize.y;
            int bound_width = boundary.x;
            int bound_height = boundary.y;
            int new_width = original_width;
            int new_height = original_height;

            // first check if we need to scale width
            if (original_width > bound_width) {
                //scale width to fit
                new_width = bound_width;
                //scale height to maintain aspect ratio
                new_height = (new_width * original_height) / original_width;
            }

            // then check if we need to scale even with the new height
            if (new_height > bound_height) {
                //scale height to fit instead
                new_height = bound_height;
                //scale width to maintain aspect ratio
                new_width = (new_height * original_width) / original_height;
            }

            return new Vector2Int(new_width, new_height);
        }

        public static void fitBoundary(Texture2D tex, Vector2Int boundary) {
            Vector2Int wh = fitScaled(new Vector2Int(tex.width, tex.height), boundary);
            ThreadedScale(tex, wh.x, wh.y, true);
        }


    }
}
