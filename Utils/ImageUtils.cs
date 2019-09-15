using System;
using UnityEngine;

namespace surfm.tool.realtimedb {
    public class ImageUtils {

        private static ImageUtils _instance;

        private float imgWidth = ConstantRepo.getInstance().get<float>("Firebase.Image.width");
        private float imgHeight = ConstantRepo.getInstance().get<float>("Firebase.Image.height");


        public void uploadFile(string filePath,string fbDir,Action<string> cb) {
            Texture2D t = resize(filePath);
            StorageUtils.instance.uploadAutoHash(ImageConversion.EncodeToJPG(t), fbDir+"/{0}.jpg",cb);
        }

        public Texture2D resize(string path) {
            Texture2D orgT = NativeToolkit.LoadImageFromFile(path);
            return resize(orgT);
        }

        public Texture2D resize(Texture2D t2d) {
            TextureScale.fitBoundary(t2d, new Vector2Int((int)imgWidth, (int)imgHeight));
            return t2d;
        }

        public static ImageUtils instance
        {
            get
            {
                if (_instance == null) {
                    _instance = new ImageUtils();
                }
                return _instance;
            }
        }

    }
}
