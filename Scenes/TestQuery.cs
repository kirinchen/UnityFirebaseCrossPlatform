using Firebase.Database;
using surfm.tool.realtimedb;
using UnityEngine;

public class TestQuery : MonoBehaviour {

    private RealtimeDB realtimeDB;

    void Start() {
        RealtimeDBFactory.get().initDoneCB().add(i => {
            realtimeDB = i;
            realtimeDB.subscribe("", s => { Debug.Log("subscribe:" + s); });
            onTestQuery();
        });
    }


    public void onTestQuery() {
        Debug.Log("onTestQuery!");

        DatabaseReference f = FirebaseDatabase.DefaultInstance.RootReference.Child("numbers");
        Query qf = f.OrderByChild("value").StartAt(3);
        qf.ChildAdded += (s, e) => {
            string json = e.Snapshot.GetRawJsonValue();
            Debug.Log("ChildAdded json:" + json);
        };
        qf.ValueChanged += (s, e) => {
            string json = e.Snapshot.GetRawJsonValue();
            Debug.Log("ValueChanged json:" + json);
        };
        //f.OrderByChild("name").EqualTo("one").ChildAdded += (s, e) => {
        //    string json = e.Snapshot.GetRawJsonValue();
        //    Debug.Log("json:" + json);
        //};
    }



}
