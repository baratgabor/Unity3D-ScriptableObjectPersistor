using System;
using UnityEngine;

namespace LeakyAbstraction.ScriptableObjectPersistor
{
    /// <summary>
    /// Stores and retrieves an object - using Json - with Unity's integrated PlayerPrefs system.
    /// </summary>
    [CreateAssetMenu(menuName = "Persistence Mechanisms/PlayerPrefs")]
    public class PlayerPrefsPersistenceMechanism : PersistenceMechanism
    {
        public override bool Load<T>(string key, out T obj)
        {
            var json = PlayerPrefs.GetString(key);
            if (string.IsNullOrEmpty(json))
            {
                obj = default;
                return false;
            }

            obj = FromJson<T>(json);
            return true;
        }

        public override bool Save(string key, object obj)
        {
            var json = ToJson(obj);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
            return true; // Always true, unless an exception is thrown, which should be left bubbling up.
        }

        protected string ToJson(object obj)
        {
            try
            {
                return JsonUtility.ToJson(obj);
            }
            catch (Exception e)
            {
                throw new Exception($"Json serialization of type '{obj.GetType()}' failed: {e.Message}", e);
            }
        }

        protected T FromJson<T>(string json)
        {
            try
            {
                return JsonUtility.FromJson<T>(json);
            }
            catch (Exception e)
            {
                throw new Exception($"Json deserialization into type '{nameof(T)}' failed: {e.Message}", e);
            }
        }
    }
}