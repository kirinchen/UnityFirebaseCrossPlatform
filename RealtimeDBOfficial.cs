#if !UNITY_WEBGL

using Firebase.Auth;
using Firebase.Database;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace surfm.tool.realtimedb {
    public class RealtimeDBOfficial : RealtimeDB {
        private CallbackListT<RealtimeDB> initCB;
     


        public RealtimeDBOfficial() {
            initCB = new CallbackListT<RealtimeDB>();
        }

        public void init(FirebaseAuther fau) {
            fau.auth().authDoneCB().add(()=> initCB.done(this));

        }

        public CallbackListT<RealtimeDB> initDoneCB() {
            return initCB;
        }

        public bool isInited() {
            return initCB.isDone();
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
            if (!isInited()) throw new NullReferenceException("Not Login");
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