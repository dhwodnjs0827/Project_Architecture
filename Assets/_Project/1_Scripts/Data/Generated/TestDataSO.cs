using System.Collections.Generic;
using UnityEngine;

namespace Generated
{
    [CreateAssetMenu(fileName = "TestDataSO", menuName = "Data/TestDataSO")]
    public class TestDataSO : ScriptableObject
    {
        public int id; // 아이디
public float attack; // 공격력
public string description; // 설명
public string eventType; // 이벤트 종류
public List<int> weapons; // 사용할 무기들
public int[] armors; // 사용할 방어구
public float[] spawnPos; // 스폰 위치
    }
}
  