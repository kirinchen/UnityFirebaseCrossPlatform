#if !UNITY_WEBGL

using Firebase.Auth;
using Firebase.Database;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace surfm.tool.realtimedb {
    public class RealtimeDBOfficial : RealtimeDB {
        private UnityMainThreadDispatcher unityMainThread;
        private CallbackListT<RealtimeDB> initCB;
        private Firebase.Auth.FirebaseUser user;
        private string email = ConstantRepo.getInstance().get<string>("RealtimeDB.email");
        private string pass = ConstantRepo.getInstance().get<string>("RealtimeDB.pass");

        public RealtimeDBOfficial() {
            initCB = new CallbackListT<RealtimeDB>();
            init();
        }

        public void init() {
            unityMainThread = UnityMainThreadDispatcher.instance();
            FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
            auth.SignInWithEmailAndPasswordAsync(email, pass).ContinueWith(task => {
                if (task.IsCanceled) {
                    Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                    return;
                }
                if (task.IsFaulted) {
                    Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    return;
                }

                user = task.Result;
                unityMainThread.Enqueue(() => {
                    initCB.done(this);
                });
                Debug.LogFormat("Firebase user login successfully: {0} ({1})",
                    user.DisplayName, user.UserId);


            });

        }

        public CallbackListT<RealtimeDB> initDoneCB() {
            return initCB;
        }

        public bool isInited() {
            return user != null;
        }

        public void put(string path, object v2, Action<Exception> exCB = null) {
            if (!isInited()) throw new NullReferenceException("Not Login");
            DatabaseReference f = getPath(path);
            Task t = f.SetValueAsync(v2);
            
            t.ContinueWith(task => {
                exCB?.Invoke(task.Exception);
            });
            
        }

        private DatabaseReference getPath(string path) {
            string[] ps = path.Split('/');
            DatabaseReference f = FirebaseDatabase.DefaultInstance.RootReference;
            foreach (string p in ps) {
                f = f.Child(p);
            }
            return f;
        }

        private Map<string, RealDBListener> subscribeMap = new Map<string, RealDBListener>();


        public void subscribe(string path, Action<string> val) {
            if (!isInited()) throw new NullReferenceException("Not Login");
            DatabaseReference f = getPath(path);
            RealDBListener l = null;
            if (!subscribeMap.ContainsKey(path)) {
                l = subscribeMap.get(path, new RealDBListener(path));
                f.ValueChanged += l.eventHandler;
            } else {
                l = subscribeMap[path];
            }
            l.add(val);
        }

        public void putJson(string path, object val, Action<Exception> exCB = null) {
            DatabaseReference f = getPath(path);
            string json = CommUtils.toJson(val);
            Task t = f.SetRawJsonValueAsync(json);
            t.ContinueWith(task => {
                exCB?.Invoke(task.Exception);
            });
        }
    }
}

#endif