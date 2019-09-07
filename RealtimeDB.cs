using System;

namespace surfm.tool.realtimedb {

    public interface RealtimeDB {
        bool isInited();
        void init();
        CallbackListT<RealtimeDB> initDoneCB();
        void put(string path, object val, Action<Exception> exCB = null);
        void putJson(string path, object val, Action<Exception> exCB = null);
        void subscribe(string path,Action<string> val);


    }



    public class RealtimeDBFactory {
        private static RealtimeDB instance;

        public static RealtimeDB get() {
            if (instance == null) {
#if !UNITY_WEBGL
                instance = new RealtimeDBOfficial();
#endif
            }
            return instance;
        }
    }
}
