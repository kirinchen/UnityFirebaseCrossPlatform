using Firebase.Database;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace surfm.tool.realtimedb {

    public enum ListenerKind {
        Value, ChildAdd
    }


    public class RealDBListener {

        private Query query;
        private string path;



        public class Bundle {
            private Query query;
            public ListenerKind kind { get; private set; }
            public bool listened;
            public List<object> cbList = new List<object>();

            internal Bundle(Query q, ListenerKind kind) {
                query = q;
                this.kind = kind;
            }
            //TODO 這樣後加的CB 會無法執行
            public void handle(Query q) {
                switch (kind) {
                    case ListenerKind.Value:
                        q.ValueChanged += valueHandler;
                        return;
                    case ListenerKind.ChildAdd:
                        q.ChildAdded += childAddHandler;
                        return;
                }
                throw new Exception("Not Support :" + kind);
            }

            public void valueHandler(object sender, ValueChangedEventArgs e) {
                UnityMainThreadDispatcher.uniRxRun(() => {
                    cbList.ForEach(cb => {
                        Action<string> _cb = (Action<string>)cb;
                        _cb(e.Snapshot.GetRawJsonValue());
                    });
                });
            }

            public void childAddHandler(object sender, ChildChangedEventArgs e) {
                UnityMainThreadDispatcher.uniRxRun(() => {
                    cbList.ForEach(cb => {
                        ChildCB childCB = (ChildCB)cb;
                        childCB(e.Snapshot.Key, e.Snapshot.GetRawJsonValue());
                    });
                });
            }

            internal void add(object cb) {
                if (cbList.Contains(cb)) {
                    return;
                }
                cbList.Add(cb);
                if (!listened) {
                    listened = true;
                    handle(query);
                }
            }
        }

        private List<Bundle> bundles = new List<Bundle>();



        internal RealDBListener(Query q, string p) {
            path = p;
            query = q;
            bundles.Add(new Bundle(q, ListenerKind.Value));
            bundles.Add(new Bundle(q, ListenerKind.ChildAdd));

        }


        public void add(ListenerKind k, object cb) {
            Bundle b = bundles.Find(_b => _b.kind == k);
            b.add(cb);
        }



    }
}
