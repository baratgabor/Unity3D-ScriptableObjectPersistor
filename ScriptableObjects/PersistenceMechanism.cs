using UnityEngine;

namespace LeakyAbstraction.ScriptableObjectPersistor
{
    /// <summary>
    /// Abstract base class for deriving concrete persistence mechanisms.
    /// </summary>
    public abstract class PersistenceMechanism : ScriptableObject
    {
        /// <summary>
        /// Saves the specified object with the specified key.
        /// </summary>
        /// <param name="key">The key that identifies the data.</param>
        /// <param name="obj">The object containing the data to save.</param>
        /// <returns>Returns True if saving was successful.</returns>
        public abstract bool Save(string key, object obj);

        /// <summary>
        /// Loads the data associated with the provided key, and returns an instance of T populated with the given data.
        /// </summary>
        /// <param name="key">The key that identifies the data.</param>
        /// <param name="obj">The instance containing the loaded data.</param>
        /// <returns>Returns True if loading was successful.</returns>
        public abstract bool Load<T>(string key, out T obj);
    }
}