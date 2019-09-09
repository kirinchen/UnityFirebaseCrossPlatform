using Firebase.Database;
using System;
using System.Collections.Generic;

namespace surfm.tool.realtimedb {
    public class RealDBListener {

        private string path;
        private List<Action<string>> list = new List<Action<string>>();

        internal RealDBListener(string p) {
            path = p;
        }

        public void eventHandler(object sender, ValueChangedEventArgs e) {
            list.ForEach(cb=> cb(e.Snapshot.GetRawJsonValue()));
        }

        public void add(Action<string> cb) {
            if (list.Contains(cb)) return;
            list.Add(cb);
        }

    }
}
