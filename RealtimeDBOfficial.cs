#if !UNITY_WEBGL

using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
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
            fau.auth().authDoneCB().add(onAuthed);

        }

        private void onAuthed() {
            initCB.done(this);
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
            DatabaseReference f = FirebaseDatabase.DefaultInstance.RootReference;
            return f.Child(path);
        }

        private Map<string, RealDBListener> subscribeMap = new Map<string, RealDBListener>();


        public void subscribe(string path, Action<string> val) {
            if (!isInited()) throw new NullReferenceException("Not Login");
            DatabaseReference f = getPath(path);
            RealDBListener l = getAndInjectListener(path,f);
            l.add(ListenerKind.Value,val);
        }

        private RealDBListener getAndInjectListener(string path , Query f) {
            RealDBListener l = null;
            if (!subscribeMap.ContainsKey(path)) {
                l = subscribeMap.get(path, new RealDBListener(f,path));
            } else {
                l = subscribeMap[path];
            }
            return l;
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

        public string query(string path, string child, string start, string end, ChildCB childCB) {
            DatabaseReference f = FirebaseDatabase.DefaultInstance.RootReference.Child(path);
            Query qf = f.OrderByChild(child).StartAt(start).EndAt(end);
            string pathUid = path + "_" + child + "_" + start + "_" + end;
            RealDBListener l = getAndInjectListener(pathUid, qf);
            l.add(ListenerKind.ChildAdd,childCB);
            return pathUid;
        }
    }
}

#endif