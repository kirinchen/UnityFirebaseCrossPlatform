using Firebase.Auth;
using surfm.tool.realtimedb;
using System;
using UniRx;
using UnityEngine;

public class TestFirebase : MonoBehaviour {

    private FirebaseAuth auth;

    void Start() {
        RealtimeDBFactory.get().initDoneCB().add(i => {
            i.subscribe("qq/22", s => {
                Debug.Log("val:" + s);
                Debug.Log("Count");
            });
        });
    }

    private void OnGUI() {
        if (GUILayout.Button("TEST")) {
            putToDB();
        }
        if (GUILayout.Button("TEST2")) {
            RealtimeDBFactory.get().initDoneCB().add(i => {
                putAll(i);
            });
        }
    }
    private int testI;
    private void putAll(RealtimeDB dB) {
        IObservable<int>[] ios = new IObservable<int>[1000];
        for (int idx = 0; idx < ios.Length; idx++) {
            ios[idx] = Observable.Start(() => {
                // heavy method...
                System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
                string msg = "aaaa_" + testI++;
                dB.put("qq/22", msg);
                Debug.Log("putToDB:" + msg);
                return 10;
            });
        }

        Observable.WhenAll(ios)
        .ObserveOnMainThread() // return to main thread
        .Subscribe(xs => {
            Debug.Log("DONE");
        });

    }

    private void putToDB() {
        RealtimeDBFactory.get().initDoneCB().add(i => {
            int ti = (int)Time.time;
            string msg = "aaaa_" + ti;
            i.put("qq/22", msg);
            Debug.Log("putToDB:" + msg);
        });
    }

}
