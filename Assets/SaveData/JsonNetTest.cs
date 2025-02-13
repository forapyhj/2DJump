using Newtonsoft.Json;
using UnityEngine;

public class JsonNetTest : MonoBehaviour
{
    void Start()
    {
        var data = new { name = "Player", score = 100 };
        string json = JsonConvert.SerializeObject(data);
        Debug.Log(json); // 应输出 {"name":"Player","score":100}
    }
}