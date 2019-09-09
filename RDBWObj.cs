using surfm.tool;
using System;

namespace surfm.tool.realtimedb {
    public abstract class RDBWObj<T> {

        public RDBRObj<T> rObj { get; private set; }
        public CallbackList fetchDoneCb { get; private set; } = new CallbackList();

        public T obj { get; private set; }

        public RDBWObj(RDBRObj<T> ro) {
            rObj = ro;
            rObj.onEmptyCB.add(() => {
                obj = initObj();
                fetchDoneCb.done();
            });
        }


        internal abstract T initObj();

        public void post(T o) {
            string nShal = getHash(o);
            string oldSha1 = rObj.loadHash();
            if (nShal.Equals(oldSha1)) return;
            obj = o;
        }

        private string getHash(T obj) {
            if (obj == null) throw new NullReferenceException("obj is null");
            string json = CommUtils.toJson(obj);
            string sha1 = CommUtils.getSha1(json + "@fskdjfjis8rue8dnj");
            return sha1.Substring(0, 16);
        }


    }
}
