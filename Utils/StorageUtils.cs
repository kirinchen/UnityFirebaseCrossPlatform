using Firebase.Storage;
using System;
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

        public void getDownloadUrl(string fbPath, Action<Uri> cb) {
            authDoneCB.add(() => {
                StorageReference reference = getByPath(fbPath);
                reference.GetDownloadUrlAsync().ContinueWith(tk =>
                        UnityMainThreadDispatcher.uniRxRun(() => {
                            cb(tk.Result);
                        })
                 );
            });
        }

        public void uploadAutoHash(byte[] custom_bytes, string dirFormat, Action<string> cb) {
            string sha1 = CommUtils.getSha1(custom_bytes);
            string path = string.Format(dirFormat, sha1);
            upload(custom_bytes, path, cb);
        }

        private StorageReference getByPath(string path) {
            FirebaseStorage storage = FirebaseStorage.DefaultInstance;
            Firebase.Storage.StorageReference storage_ref = storage.GetReferenceFromUrl(bucketUrl);
            return storage_ref.Child(path);
        }

        public void upload(byte[] custom_bytes, string fbPath, Action<string> cb) {
            authDoneCB.add(() => {
                StorageReference rivers_ref = getByPath(fbPath);
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
                          UnityMainThreadDispatcher.uniRxRun(() => {
                              cb(metadata.Path);
                              Debug.Log("Finished uploading...");
                          });

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
