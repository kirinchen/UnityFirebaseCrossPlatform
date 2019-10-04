using Firebase.Database;
using System;
using System.Collections.Generic;

namespace surfm.tool.realtimedb {

    public enum ListenerKind {
        Value, ChildAdd
    }


    public static class ListenerEx {

        public static Event genEvent(this ListenerKind kind,Query q,object cb) {
            switch (kind) {
                case ListenerKind.Value:
                    return new ValueEvent(q,cb);
                case ListenerKind.ChildAdd:
                    return new ChildAddEvent(q,cb);
            }
            throw new NullReferenceException("Not Support kind:"+kind);
        }

        public static bool exist(this List<Event> events, object cb) {
            return events.Exists(e=> e.cb.Equals(cb));
        }

    }

    public abstract class Event {
        private Query query;
        public object cb { get; private set; }
        internal Event(Query q, object c) {
            query = q;
            cb = c;
            inject(query);
        }
        internal abstract void inject(Query q);
    }

    public class ValueEvent : Event {
        public ValueEvent(Query q, object c) : base(q, c) { }

        internal override void inject(Query q) {
            q.ValueChanged += valueHandler;
        }

        public void valueHandler(object sender, ValueChangedEventArgs e) {
            UnityMainThreadDispatcher.uniRxRun(() => {
                Action<string> _cb = (Action<string>)cb;
                _cb(e.Snapshot.GetRawJsonValue());
            });
        }
    }

    public class ChildAddEvent : Event {
        public ChildAddEvent(Query q, object c) : base(q, c) { }

        internal override void inject(Query q) {
            q.ChildAdded += childAddHandler;
        }

        public void childAddHandler(object sender, ChildChangedEventArgs e) {
            UnityMainThreadDispatcher.uniRxRun(() => {
                ChildCB childCB = (ChildCB)cb;
                childCB(e.Snapshot.Key, e.Snapshot.GetRawJsonValue());
            });

        }
    }

    public class RealDBListener {

        private Query query;
        private string path;



        public class Bundle {
            private Query query;
            public ListenerKind kind { get; private set; }
            public List<Event> cbList = new List<Event>();

            internal Bundle(Query q, ListenerKind kind) {
                query = q;
                this.kind = kind;
            }

            internal void add(object cb) {
                if (cbList.exist(cb)) {
                    return;
                }
                Event e= kind.genEvent(query,cb);
                cbList.Add(e);
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
