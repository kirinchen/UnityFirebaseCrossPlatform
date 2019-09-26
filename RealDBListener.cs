using Firebase.Database;
using System;
using System.Collections.Generic;

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
            public List<Action<string>> list = new List<Action<string>>();

            internal Bundle(Query q,ListenerKind kind) {
                query = q;
                this.kind = kind;
            }

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
                list.ForEach(cb => UnityMainThreadDispatcher.uniRxRun(() => { cb(e.Snapshot.GetRawJsonValue()); }));
            }

            public void childAddHandler(object sender, ChildChangedEventArgs e) {
                list.ForEach(cb => UnityMainThreadDispatcher.uniRxRun(() => { cb(e.Snapshot.GetRawJsonValue()); }));
            }

            internal void add(Action<string> cb) {
                if (list.Contains(cb)) return;
                list.Add(cb);
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
            bundles.Add(new Bundle(q,ListenerKind.Value));
            bundles.Add(new Bundle(q,ListenerKind.ChildAdd));

        }


        public void add(ListenerKind k,Action<string > cb) {
            Bundle b = bundles.Find(_b=> _b.kind == k);
            b.add(cb);
        }



    }
}
