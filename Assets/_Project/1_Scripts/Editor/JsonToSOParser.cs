using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

public static class JsonToSOParser
{
    private const string JSON_PATH = "Assets/_Project/Resources/Data/JSON";
    private const string SO_PATH = "Assets/_Project/Resources/Data/SO";

    [MenuItem("Tools/Data/Parse JSON To SO All")]
    public static void ParseAll()
    {
        // JSON 폴더의 모든 .json 파일 가져오기
        if (!Directory.Exists(JSON_PATH))
        {
            CDebug.LogWarning($"[JsonToSOParser] JSON folder not found: {JSON_PATH}");
            return;
        }

        var jsonFiles = Directory.GetFiles(JSON_PATH, "*.json");

        if (jsonFiles.Length == 0)
        {
            CDebug.LogWarning("[JsonToSOParser] No JSON files found");
            return;
        }

        int successCount = 0;

        foreach (var jsonFile in jsonFiles)
        {
            // 파일명에서 데이터 이름 추출 (ItemData.json → ItemData)
            string dataName = Path.GetFileNameWithoutExtension(jsonFile);

            if (ParseData(dataName))
            {
                successCount++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[JsonToSOParser] ✓ Parse completed! ({successCount}/{jsonFiles.Length})");
    }

    private static bool ParseData(string dataName)
    {
        string jsonPath = $"{JSON_PATH}/{dataName}.json";
        string soRootPath = $"{SO_PATH}/{dataName}";

        // 타입 찾기
        Type dataType = FindType(dataName);
        Type soType = FindType($"{dataName}SO");

        if (dataType == null || soType == null)
        {
            Debug.LogWarning($"[JsonToSOParser] Type not found: {dataName} or {dataName}SO");
            return false;
        }

        // JSON 파일 읽기
        var jsonFile = AssetDatabase.LoadAssetAtPath<TextAsset>(jsonPath);
        if (jsonFile == null)
        {
            Debug.LogWarning($"[JsonToSOParser] JSON not found: {jsonPath}");
            return false;
        }

        // 제네릭 메서드 호출
        MethodInfo method = typeof(JsonToSOParser).GetMethod(
            nameof(ImportDataGeneric),
            BindingFlags.NonPublic | BindingFlags.Static
        );

        MethodInfo genericMethod = method.MakeGenericMethod(dataType, soType);
        return (bool)genericMethod.Invoke(null, new object[] { jsonFile.text, soRootPath, dataName });
    }

    private static bool ImportDataGeneric<TData, TSO>(string jsonText, string soRootPath, string dataName)
        where TSO : ScriptableObject
    {
        try
        {
            // JSON 파싱
            var wrappedJson = $"{{\"items\":{jsonText}}}";
            var wrapper = JsonConvert.DeserializeObject<ListWrapper<TData>>(wrappedJson);

            if (wrapper?.items == null || wrapper.items.Count == 0)
            {
                Debug.LogError($"[JsonToSOParser] Failed to parse or empty data: {dataName}");
                return false;
            }

            // SO 폴더 생성 (한 번만)
            string directoryPath = $"{SO_PATH}/{dataName}";
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
                AssetDatabase.Refresh();
            }

            // 첫 번째 필드 캐싱 (파일명으로 사용)
            var dataFields = typeof(TData).GetFields();
            FieldInfo firstField = dataFields.Length > 0 ? dataFields[0] : null;

            // TSO 필드 캐싱
            var soFields = typeof(TSO).GetFields(BindingFlags.Public | BindingFlags.Instance);

            for (int i = 0; i < wrapper.items.Count; i++)
            {
                var item = wrapper.items[i];

                // SO 파일명 결정 (첫 번째 필드 값 사용)
                string fileName;
                if (firstField != null)
                {
                    var firstValue = firstField.GetValue(item);
                    fileName = firstValue != null ? firstValue.ToString() : i.ToString();
                }
                else
                {
                    fileName = i.ToString();
                }

                string itemSOPath = $"{directoryPath}/{fileName}.asset";

                // SO 로드 또는 생성
                var so = AssetDatabase.LoadAssetAtPath<TSO>(itemSOPath);
                if (so == null)
                {
                    so = ScriptableObject.CreateInstance<TSO>();
                    AssetDatabase.CreateAsset(so, itemSOPath);
                }

                // 필드 복사 (이름 기준)
                foreach (var soField in soFields)
                {
                    var dataField = typeof(TData).GetField(soField.Name);
                    if (dataField == null)
                        continue;

                    var value = dataField.GetValue(item);
                    soField.SetValue(so, value);
                }

                EditorUtility.SetDirty(so);
            }

            Debug.Log($"[JsonToSOParser] ✓ Parsed {dataName} ({wrapper.items.Count} items)");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[JsonToSOParser] Error parsing {dataName}: {e}");
            return false;
        }
    }

    private static Type FindType(string typeName)
    {
        // Generated 네임스페이스 안에서만 검색
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.Name == typeName && t.Namespace == "Generated");
    }

    [Serializable]
    private class ListWrapper<T>
    {
        public List<T> items;
    }
}