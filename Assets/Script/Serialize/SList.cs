using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[Serializable]
public class SList<TValue> : ISerializationCallbackReceiver, IList<TValue>
{
    [SerializeField] private List<SValue<TValue>> list = new List<SValue<TValue>>();


    public SList()
    {

    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {

    }



    public int Count => list.Count;
    public bool IsReadOnly => false;

    public TValue this[int index]
    {
        get => list[index].Value;
        set
        {
            if (index < list.Count)
            {
                SValue<TValue> v = list[index];
                v.Value = value;
                list[index] = v;
            }
            else
            {
                list.Add(new SValue<TValue>(value));
            }
        }
    }


    #region IEnumerable <KeyValuePair<TKey, TValue>>

    public IEnumerator<TValue> GetEnumerator()
    {
        return list.Select(ToKeyValuePair).GetEnumerator();

        static TValue ToKeyValuePair(SValue<TValue> sb)
        {
            return sb.Value;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    public int IndexOf(TValue item)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].Value.Equals(item))
            {
                return i;
            }
        }
        return -1;
    }

    public void Insert(int index, TValue item)
    {
        list.Insert(index, new SValue<TValue>(item));
    }

    public void RemoveAt(int index)
    {
        list.RemoveAt(index);
    }

    public void Add(TValue item)
    {
        list.Add(new SValue<TValue>(item));
    }

    public void Clear()
    {
        list.Clear();
    }

    public bool Contains(TValue item)
    {
        int res = IndexOf(item);
        return res != -1;
    }

    public void CopyTo(TValue[] array, int arrayIndex)
    {
        SList<TValue> newList = new SList<TValue>();
        foreach (var data in list)
        {
            newList.Add(data.Value);
        }
    }

    public bool Remove(TValue item)
    {
        int res = IndexOf(item);
        if (res != -1)
        {
            list.RemoveAt(res);
            return true;
        }
        return false;
    }

    public List<TValue> ToList()
    {
        List<TValue> l = new List<TValue>();
        foreach (var item in list)
        {
            l.Add(item.Value);
        }
        return l;
    }
}
