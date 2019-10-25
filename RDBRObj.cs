using CodeStage.AntiCheat.Storage;
using UniRx;
using UnityEngine;

namespace surfm.tool.realtimedb {

    public abstract class RDBRObj<T> {
        public static readonly string DATA_HASH_PATH = "hash/";
        public static readonly string DATA_PATH = "data/";
        public IReactiveProperty<T> obj { get; private set; }
        public string uid { get; private set; }
        public bool dataSubscribed { get; private set; }
        public InitCallBack onEmptyCB { get; private set; } = new InitCallBack();
        public InitCallBack onFetchedCB { get; private set; } = new InitCallBack();
        public string dev;


        protected abstract string pathPrefix();

        public RDBRObj(string u , string _dev) {
            dev = _dev;
            uid = u;
            obj = new ReactiveProperty<T>(loadData());
            RealtimeDBFactory.get().initDoneCB().add(init);
        }

        public string getPath(string subfix) {
            return pathPrefix() + uid + "/" + subfix;
        }

        private void init(RealtimeDB rdb) {
            string hashPath = getPath(DATA_HASH_PATH);
            Debug.Log("debug:" + dev + " type:" + GetType() + " init:" + rdb + " hashPath:" + hashPath);
            rdb.subscribe(hashPath, onHash);
        }

        private void onHash(string obj) {
            if (string.IsNullOrWhiteSpace(obj)) onEmptyCB.done();
            bool fetched = true;
            if (!dataSubscribed) {
                string orgSha1 = loadHash();
                if (!orgSha1.Equals(obj)) {
                    fetched = false;
                    subscribeData();
                }
            }
            storeSha1(obj);
            if (fetched) onFetchedCB.done();

        }

        private void subscribeData() {
            string dataPath = getPath(DATA_PATH);
            Debug.Log("subscribeData:" + dataPath);
            RealtimeDBFactory.get().subscribe(dataPath, s => {
                replaceData(s);
                dataSubscribed = true;
                onFetchedCB.done();
            });
        }

        private void replaceData(string s) {
            if (!string.IsNullOrWhiteSpace(s)) {
                obj.Value = CommUtils.convertByJson<T>(s);
            }
            storeData(s);
        }

        private T loadData() {
            return CommUtils.convertByJson<T>(ObscuredPrefs.GetString(getPath(DATA_PATH), ""));
        }

        private void storeData(string s) {
            ObscuredPrefs.SetString(getPath(DATA_PATH), s);
        }

        private void storeSha1(string sha1) {
            ObscuredPrefs.SetString(getPath(DATA_HASH_PATH), sha1);
        }

        public string loadHash() {
            return ObscuredPrefs.GetString(getPath(DATA_HASH_PATH), "");
        }
    }
}
