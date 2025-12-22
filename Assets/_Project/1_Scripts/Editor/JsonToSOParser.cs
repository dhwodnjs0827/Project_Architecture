using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class JsonToSOParser
{
    private const string JSON_PATH = "Assets/_Project/Resources/Data/JSON";
    private const string SO_PATH = "Assets/_Project/Resources/Data/SO";

    [MenuItem("Tools/Data/Parse All")]
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
        string soPath = $"{SO_PATH}/{dataName}SO.asset";

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
        return (bool)genericMethod.Invoke(null, new object[] { jsonFile.text, soPath, dataName });
    }

    private static bool ImportDataGeneric<TData, TSO>(string jsonText, string soPath, string dataName)
        where TSO : ScriptableObject
    {
        try
        {
            // JSON 파싱
            var wrappedJson = $"{{\"items\":{jsonText}}}";
            var wrapper = JsonUtility.FromJson<ListWrapper<TData>>(wrappedJson);

            if (wrapper?.items == null)
            {
                Debug.LogError($"[JsonToSOParser] Failed to parse: {dataName}");
                return false;
            }

            // SO 폴더 생성
            string directory = Path.GetDirectoryName(soPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh();
            }

            // SO 로드 또는 생성
            var so = AssetDatabase.LoadAssetAtPath<TSO>(soPath);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<TSO>();
                AssetDatabase.CreateAsset(so, soPath);
            }

            // 데이터 설정
            var itemsField = typeof(TSO).GetField("items");
            if (itemsField != null)
            {
                itemsField.SetValue(so, wrapper.items);
            }

            EditorUtility.SetDirty(so);

            Debug.Log($"[JsonToSOParser] ✓ Parsed: {dataName} ({wrapper.items.Count} items)");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[JsonToSOParser] Error parsing {dataName}: {e.Message}");
            return false;
        }
    }

    private static Type FindType(string typeName)
    {
        // 현재 어셈블리에서 타입 찾기
        return AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.Name == typeName);
    }

    [Serializable]
    private class ListWrapper<T>
    {
        public List<T> items;
    }
}