using Firebase.Storage;
using System.Threading.Tasks;
using UnityEngine;

namespace surfm.tool.realtimedb {
    public class StorageUtils {

        private static StorageUtils _instance;
        public CallbackList authDoneCB;
        private string bucketUrl = ConstantRepo.getInstance().get<string>("Firebase.Storage.bucketUrl");

        private StorageUtils() {
            authDoneCB = RealtimeDBFactory.getAuther().auth().authDoneCB();
        }

        public void uploadAutoHash(byte[] custom_bytes, string dirFormat) {
            string sha1 = CommUtils.getSha1(custom_bytes);
            string path = string.Format(dirFormat, sha1);
            upload(custom_bytes, path);
        }

        public void upload(byte[] custom_bytes, string fbPath) {
            authDoneCB.add(() => {
                FirebaseStorage storage = FirebaseStorage.DefaultInstance;
                Firebase.Storage.StorageReference storage_ref = storage.GetReferenceFromUrl(bucketUrl);
                Firebase.Storage.StorageReference rivers_ref = storage_ref.Child(fbPath);
                // Upload the file to the path "images/rivers.jpg"
                rivers_ref.PutBytesAsync(custom_bytes)
                  .ContinueWith((Task<StorageMetadata> task) => {
                      if (task.IsFaulted || task.IsCanceled) {
                          Debug.Log(task.Exception.ToString());
                          // Uh-oh, an error occurred!
                      } else {
                          // Metadata contains file metadata such as size, content-type, and download URL.
                          Firebase.Storage.StorageMetadata metadata = task.Result;
                          string download_url = metadata.Path;
                          Debug.Log("Finished uploading...");
                          Debug.Log("download url = " + download_url);
                      }
                  });
            });
        }

        public static StorageUtils instance
        {
            get
            {
                if (_instance == null) {
                    _instance = new StorageUtils();
                }
                return _instance;
            }
        }
    }
}
