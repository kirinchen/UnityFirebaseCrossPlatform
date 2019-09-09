using System;
using CodeStage.AntiCheat.ObscuredTypes;
using surfm.tool;
using surfm.tool.realtimedb;
using UniRx;

namespace surfm.tool.realtimedb {

    public abstract class RDBRObj<T> {
        public static readonly string DATA_HASH_PATH = "hash/";
        public static readonly string DATA_PATH = "data/";
        public IReactiveProperty<T> obj { get; private set; } 
        public string uid { get; private set; }
        public bool dataSubscribed { get; private set; }
        public CallbackList onEmptyCB { get; private set; } = new CallbackList();


        protected abstract string pathPrefix();

        public RDBRObj(string u) {
            uid = u;
            obj =  new ReactiveProperty<T>(loadData());
            RealtimeDBFactory.get().initDoneCB().add(init);
        }

        private string getPath() {
            return pathPrefix() + uid + "/";
        }

        private void init(RealtimeDB rdb) {
            string hashPath = getPath() + DATA_HASH_PATH;
            rdb.subscribe(hashPath, onSha1Change);
        }

        private void onSha1Change(string obj) {
            if (string.IsNullOrWhiteSpace(obj)) onEmptyCB.done();
            if (!dataSubscribed) {
                string orgSha1 = loadHash();
                if (!orgSha1.Equals(obj)) {
                    subscribeData();
                }
            }
            storeSha1(obj);
        }

        private void subscribeData() {
            string dataPath = getPath() + DATA_PATH;
            RealtimeDBFactory.get().subscribe(dataPath, s => {
                replaceData(s);
                dataSubscribed = true;
            });
        }

        private void replaceData(string s) {
            obj.Value = CommUtils.convertByJson<T>(s);
            storeData(s);
        }

        private T loadData() {
            return CommUtils.convertByJson<T>(ObscuredPrefs.GetString(getPath() + DATA_PATH, ""));
        }

        private void storeData(string s) {
            ObscuredPrefs.SetString(getPath() + DATA_PATH, s);
        }

        private void storeSha1(string sha1) {
            ObscuredPrefs.SetString(getPath() + DATA_HASH_PATH, sha1);
        }

        public string loadHash() {
            return ObscuredPrefs.GetString(getPath() + DATA_HASH_PATH,"");
        }
    }
}
