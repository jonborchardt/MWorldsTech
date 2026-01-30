using UnityEngine;
using GameNet.Core;

namespace GameNet.UnityAdapters
{
    /// <summary>
    /// Unity adapter for JSON serialization using JsonUtility.
    /// </summary>
    public class UnityJsonSerializer : IJsonSerializer
    {
        public T Deserialize<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }

        public string Serialize<T>(T obj, bool prettyPrint = false)
        {
            return JsonUtility.ToJson(obj, prettyPrint);
        }
    }
}
