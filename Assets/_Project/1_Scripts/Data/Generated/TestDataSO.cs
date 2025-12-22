using System.Collections.Generic;
  using UnityEngine;

  [CreateAssetMenu(fileName = "TestDataSO", menuName = "Data/TestDataSO")]
  public class TestDataSO : ScriptableObject
  {
      public List<TestData> items = new();
      
      private Dictionary<int, TestData> dataDict;
      
      public void Initialize()
      {
          dataDict = new Dictionary<int, TestData>();
          foreach (var item in items)
          {
              dataDict[item.id] = item;
          }
      }
      
      public TestData Get(int id)
      {
          if (dataDict == null || dataDict.Count == 0)
          {
              Initialize();
          }
          return dataDict.GetValueOrDefault(id);
      }
  }
  