using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum Tabs
{
    Tab1,
    Tab2,
}

public class Launcher : MonoBehaviour
{
    public SDictionary<Tabs, string> _dic = new SDictionary<Tabs, string>();

    public SList<SList<int>> _list;

    // Start is called before the first frame update
    void Start()
    {
        //_dic.Add(1, "≤‚ ‘1");
        //_dic.Add(2, "≤‚ ‘2");

        foreach (var data in _dic)
        {
            Debug.Log(" ˝æ›:" + data.Key + "-" + data.Value);
        }
        Dictionary<Tabs, string> dic = _dic.ToDictionary();
        Debug.Log(_dic[Tabs.Tab1].ToString());
    }

    // Update is called once per frame
    void Update()
    {

    }
}
