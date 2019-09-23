using Firebase.Database;
using surfm.tool;
using surfm.tool.realtimedb;
using System.Collections.Generic;
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
        Query qf = f.OrderByChild("x").StartAt(30);
        qf = qf.OrderByChild("y").StartAt(30);
        qf.ChildAdded += (s, e) => {
            string json = e.Snapshot.GetRawJsonValue();
            Debug.Log("ChildAdded json:" + json);
        };
        /*qf.ValueChanged += (s, e) => {
            string json = e.Snapshot.GetRawJsonValue();
            Debug.Log("ValueChanged json:" + json);
        }; */
        //f.OrderByChild("name").EqualTo("one").ChildAdded += (s, e) => {
        //    string json = e.Snapshot.GetRawJsonValue();
        //    Debug.Log("json:" + json);
        //};
    }

    public class TestData {
        public float x;
        public float y;
        public string name;
    }

    public void putTestData() {
        DatabaseReference f = FirebaseDatabase.DefaultInstance.RootReference.Child("numbers");
        List<TestData> list = new List<TestData>();
        for (int i=0;i<20;i++) {
            list.Add(new TestData() {
                x = Random.Range(1, 100),
                y = Random.Range(1, 100),
                name = "test_"+i
            });
        }
        f.SetRawJsonValueAsync(CommUtils.toJson(list) );


    }



}
