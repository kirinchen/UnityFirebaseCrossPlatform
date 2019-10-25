using System;
using UniRx;
using UnityEngine;

namespace surfm.tool.realtimedb {
    public abstract class RDBWObj<T> {

        public RDBRObj<T> rObj { get; private set; }
        public InitCallBack fetchDoneCb { get; private set; } = new InitCallBack();
        public T obj { get; private set; }
        public Action<T> postedCB = t=> { };
        public string uid { get { return rObj.uid; } }

        public RDBWObj(RDBRObj<T> ro) {
            rObj = ro;
            rObj.onEmptyCB.add(() => {
                obj = initObj();
                post();
                fetchDoneCb.done();
            });
            rObj.onFetchedCB.add(()=> {
                fetchDoneCb.done();
            });
            rObj.obj.Where(o => o != null).Subscribe(v => {
                obj = v;
            });
        }


        internal abstract T initObj();

        public void postMe() {
            fetchDoneCb.add(post);
        }

        private void post() {
            string nShal = getHash();
            string oldSha1 = rObj.loadHash();
            if (nShal.Equals(oldSha1)) return;
            Map<string, object> _da = new Map<string, object>();
            _da.put("hash", nShal);
            _da.put("data", obj);
            RealtimeDBFactory.get().putJson(rObj.getPath(""), _da);
            postedCB(obj);

        }

        private string getHash() {
            if (obj == null) throw new NullReferenceException("obj is null");
            string json = CommUtils.toJson(obj);
            string sha1 = CommUtils.getSha1(json + "@fskdjfjis8rue8dnj");
            return sha1.Substring(0, 16);
        }


    }
}
