using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using UnityEngine;

namespace LeakyAbstraction.ScriptableObjectPersistor
{
    /// <summary>
    /// Stores object state in a file, in AES-128 encrypted binary format.
    /// </summary>
    [CreateAssetMenu(menuName = "Persistence Mechanisms/Encrypted binary file")]
    public class EncryptedFilePersistenceMechanism : PersistenceMechanism
    {
        [SerializeField]
        protected string _fileName = "filename.aes";

        protected string _filePath => Path.Combine(Application.persistentDataPath, _fileName);
        protected BinaryFormatter _binaryFormatter = new BinaryFormatter();

        protected const int ByteLength = 16;
        protected static readonly byte[] KeyGenSalt = new byte[] { 10, 20, 30, 40, 50, 60, 70, 80 };

        /// <summary>
        /// Loads saved state from encrypted file into instance of T.
        /// </summary>
        public override bool Load<T>(string key, out T obj)
        {
            if (!File.Exists(_filePath))
            {
                obj = default;
                return false;
            }

            using (var aes = Rijndael.Create())
            {
                aes.Key = GenerateEncryptionKey(key);

                using (FileStream fileStream = new FileStream(_filePath, FileMode.Open))
                {
                    // Recover IV by reading it from the beginning of the file stream
                    var iv = new byte[ByteLength];
                    fileStream.Read(iv, 0, iv.Length); // This moves stream position to the start of encrypted data

                    using (CryptoStream cryptoStream = new CryptoStream(fileStream, aes.CreateDecryptor(aes.Key, iv), CryptoStreamMode.Read))
                    {
                        obj = (T)_binaryFormatter.Deserialize(cryptoStream); // Deserialize the saved state through decryption stream
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Saves state of instance of T into file via encryption.
        /// </summary>
        public override bool Save(string key, object obj)
        {
            Debug.Log(_filePath);

            using (var aes = Rijndael.Create())
            {
                aes.Key = GenerateEncryptionKey(key);
                aes.GenerateIV();

                using (FileStream fileStream = new FileStream(_filePath, FileMode.Create))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(fileStream, aes.CreateEncryptor(aes.Key, aes.IV), CryptoStreamMode.Write))
                    {
                        fileStream.Write(aes.IV, 0, aes.IV.Length); // Prepend initialization vector in cleartext
                        _binaryFormatter.Serialize(cryptoStream, obj); // Serialize object into file stream through encryption transform
                        // If padding errors occur when reading, try cryptoStream.FlushFinalBlock() - supposedly it can solve some problems
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Derives a byte array based encryption key from a string key.
        /// </summary>
        protected byte[] GenerateEncryptionKey(string key)
        {
            var keyGenerator = new Rfc2898DeriveBytes(key, KeyGenSalt);
            return keyGenerator.GetBytes(ByteLength);
        }
    }
}