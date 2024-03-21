using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum Tabs
{
    Tab1,
    Tab2,
}

public class Launcher : MonoBehaviour
{
    [FormerlySerializedAs("Dic")] public SDictionary<Tabs, string> dic = new SDictionary<Tabs, string>();

    [FormerlySerializedAs("List")] public SList<SList<int>> list;

    // Start is called before the first frame update
    void Start()
    {
        //_dic.Add(1, "≤‚ ‘1");
        //_dic.Add(2, "≤‚ ‘2");

        foreach (var data in this.dic)
        {
            Debug.Log(" ˝æ›:" + data.Key + "-" + data.Value);
        }
        Dictionary<Tabs, string> dictionary = this.dic.ToDictionary();
        Debug.Log(dictionary[Tabs.Tab1]);
    }

    // Update is called once per frame
}
