using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDataSO", menuName = "Data/ItemDataSO")]
public class ItemDataSO : ScriptableObject
{
    public List<ItemData> items = new();
      
    private Dictionary<int, ItemData> dataDict;
      
    public void Initialize()
    {
        dataDict = new Dictionary<int, ItemData>();
        foreach (var item in items)
        {
            dataDict[item.id] = item;
        }
    }
      
    public ItemData Get(int id)
    {
        if (dataDict == null || dataDict.Count == 0)
        {
            Initialize();
        }
        return dataDict.GetValueOrDefault(id);
    }
}
  