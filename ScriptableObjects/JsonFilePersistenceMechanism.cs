using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using UnityEngine;

namespace LeakyAbstraction.ScriptableObjectPersistor
{
    /// <summary>
    /// Stores object state in a file, in cleartext Json format.
    /// </summary>
    [CreateAssetMenu(menuName = "Persistence Mechanisms/Json File")]
    public class JsonFilePersistenceMechanism : PersistenceMechanism
    {
        [SerializeField]
        protected string _fileName = "filename.json";

        protected string _filePath => Path.Combine(Application.persistentDataPath, _fileName);

        public override bool Load<T>(string key, out T obj)
        {
            if (!File.Exists(_filePath))
            {
                obj = default;
                return false;
            }

            using (StreamReader streamReader = File.OpenText(_filePath))
            {
                var json = streamReader.ReadToEnd();
                obj = FromJson<T>(json);
            }
            return true;
        }

        public override bool Save(string key, object obj)
        {
            Debug.Log(_filePath);

            var json = ToJson(obj);
            using (StreamWriter streamWriter = File.CreateText(_filePath)) // New file or overwrite existing
            {
                streamWriter.Write(json);
            }
            return true;
        }

        protected string ToJson(object obj)
            => JsonUtility.ToJson(obj);

        protected T FromJson<T>(string json)
            => JsonUtility.FromJson<T>(json);
    }
}