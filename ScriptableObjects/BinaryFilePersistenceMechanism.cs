using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace LeakyAbstraction.ScriptableObjectPersistor
{
    /// <summary>
    /// Stores object state in a file, in binary format.
    /// </summary>
    [CreateAssetMenu(menuName = "Persistence Mechanisms/Binary File")]
    public class BinaryFilePersistenceMechanism : PersistenceMechanism
    {
        [SerializeField]
        protected string _fileName = "filename.dat";

        protected string _filePath => Path.Combine(Application.persistentDataPath, _fileName);
        protected BinaryFormatter _binaryFormatter = new BinaryFormatter();

        public override bool Save(string _, object obj)
        {
            Debug.Log(_filePath);

            using (FileStream fileStream = new FileStream(_filePath, FileMode.Create)) // New or overwrite
            {
                _binaryFormatter.Serialize(fileStream, obj);
            }
            return true;
        }

        public override bool Load<T>(string _, out T obj)
        {
            if (!File.Exists(_filePath))
            {
                obj = default;
                return false;
            }

            using (FileStream fileStream = new FileStream(_filePath, FileMode.Open))
            {
                obj = (T)_binaryFormatter.Deserialize(fileStream);
            }
            return true;
        }
    }
}
