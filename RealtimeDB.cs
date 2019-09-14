using System;

namespace surfm.tool.realtimedb {

    public interface RealtimeDB {
        bool isInited();
        void init(FirebaseAuther fau);
        CallbackListT<RealtimeDB> initDoneCB();
        void put(string path, object val, Action<Exception> exCB = null);
        void putJson(string path, object val, Action<Exception> exCB = null);
        void subscribe(string path,Action<string> val);
    }

    public interface FirebaseAuther {
        FirebaseAuther auth();
        CallbackList authDoneCB();
    }



    public class RealtimeDBFactory {
        private static RealtimeDB instance;
        private static FirebaseAuther auther;

        public static RealtimeDB get() {
            if (instance == null) {
#if !UNITY_WEBGL
                instance = new RealtimeDBOfficial();
#endif
            }
            instance.init(getAuther());
            return instance;
        }

        public static FirebaseAuther getAuther() {
            if (auther == null) {
#if !UNITY_WEBGL
                auther = new FirebaseAuthOfficial();
#else
     TODO
#endif
            }
            return auther;
        }
    }
}
