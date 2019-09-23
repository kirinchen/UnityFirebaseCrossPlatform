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
            realtimeDB.subscribe("geo", s => { onReslut(s); });
          
        });
    }


    private void onReslut(string s) {
        List<TestData> list = CommUtils.convertByJson<List<TestData>>(s);
        TestData t11 = list.Find(d=> d.x == 1 && d.y==1);
        TestData t55 = list.Find(d => d.x == 5 && d.y == 5);
        if (t11 == null) return;
        if (t55 == null) return;
        Debug.Log("Go Query");
        onTestQuery(t11.hash,t55.hash);
    }

    public void onTestQuery(string st,string end) {
        Debug.Log("onTestQuery! st:"+ st+" end:"+ end);

        DatabaseReference f = FirebaseDatabase.DefaultInstance.RootReference.Child("geo");
        //1 1  s00twy                4 4   s0dyg0
        Query qf = f.OrderByChild("hash").StartAt(st).EndAt(end);
        qf.ChildAdded += (s, e) => {
            string json = e.Snapshot.GetRawJsonValue();
            Debug.Log("ChildAdded json:" + json);
        };
    }

    public class TestData {
        public float x;
        public float y;
        public string name;
        public string hash;
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

    public void testGeoHash() {
        DatabaseReference f = FirebaseDatabase.DefaultInstance.RootReference.Child("geo");
        Map<string, Vector2Int> map = new Map<string, Vector2Int>();
        List<TestData> list = new List<TestData>();
        for (int i=-10;i<10;i++) {
            for (int j = -10; j < 10; j++) {
                Vector2Int v = new Vector2Int(i,j);
                string hash = GeoHashUtils.instance.calcGeoHash(i,j);
                list.Add(new TestData() {
                    x =i,
                    y=j,
                    name= "GEO_"+i+"_"+j,
                    hash = hash
                });
            }
        }
        f.SetRawJsonValueAsync(CommUtils.toJson(list));
    }



}
