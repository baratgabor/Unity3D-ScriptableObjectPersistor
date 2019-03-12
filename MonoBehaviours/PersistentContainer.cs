using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LeakyAbstraction.ScriptableObjectPersistor
{
    /// <summary>
    /// Behaviour that persists the state of a customizable list of ScriptableObject instances, using the specified persistence mechanism.
    /// </summary>
    public class PersistentContainer : MonoBehaviour
    {
        [Header("Method of persistence:")]
        [SerializeField]
        [Tooltip("Drag here a persistence mechanism instance that defines how the data will be saved.")]
        protected PersistenceMechanism _persistenceMechanism = default;

        [Header("Automatic save/load settings:")]
        [SerializeField]
        [Tooltip("If enabled, data will be automatically loaded when a scene is loaded.")]
        protected bool _autoLoadOnAwake = true;

        [SerializeField]
        [Tooltip("If enabled, data will be automatically saved when a scene is unloaded.")]
        protected bool _autoSaveOnDestroy = true;

        [Header("ScriptableObject instances to persist:")]
        [SerializeField]
        [Tooltip("Populate this array with the ScriptableObject instances you want to save and load automatically.")]
        protected ScriptableObject[] _persistenceList = default;

        protected string _persistentContainerKey => "PersistentContainer" + GetInstanceID();
        protected Dictionary<int, DataEntity> _dataEntities = new Dictionary<int, DataEntity>();

        protected void Awake()
        {
            ValidatePersistenceMechanism();

            if (_autoLoadOnAwake)
                LoadData();
        }

        protected void OnDestroy()
        {
            if (_autoSaveOnDestroy)
                SaveData();
        }

        protected void ValidatePersistenceMechanism()
        {
            if (_persistenceMechanism == null)
                throw new Exception($"{nameof(PersistentContainer)} dependency '{nameof(_persistenceMechanism)}' not set. Component is unable to load or save data.");
        }

        /// <summary>
        /// Loads and deserializes all persisted state, and populates the current list of persistable ScriptableObject instances with the loaded state.
        /// </summary>
        public void LoadData()
        {
            ValidatePersistenceMechanism();

            // Return if storage is empty
            if (!_persistenceMechanism.Load<DataEntitiesContainer>(_persistentContainerKey, out var dataEntitiesContainer))
                return;

            // Convert Json data into DataEntitiesContainer, read container's array, and convert it to Dictionary
            _dataEntities = dataEntitiesContainer
                .DataEntities
                .ToDictionary(entity => entity.EntityId);

            // Populate all entities in _persistenceList with the loaded data (if they already have saved data associated)
            foreach (var populable in _persistenceList)
            {
                if (populable == null)
                    continue;

                if (_dataEntities.TryGetValue(populable.GetInstanceID(), out var dataEntity))
                    JsonUtility.FromJsonOverwrite(dataEntity.JsonData, populable);
            }
        }

        /// <summary>
        /// Collects, serializes and stores the state of all ScriptableObject instances currently in the list.
        /// Also persists previously saved data that is not currently used by any of the ScriptableObject instances.
        /// </summary>
        public void SaveData()
        {
            ValidatePersistenceMechanism();

            if (_persistenceList.Length == 0)
                return;

            // Overwrite existing entry for given persistable with fresh data, or - if entry didn't exist - create it.
            // Preserves old entries (which are not connected to any current persistable) to avoid data loss.
            foreach (var persistable in _persistenceList)
            {
                if (persistable == null)
                    continue;

                var id = persistable.GetInstanceID();
                _dataEntities[id] = new DataEntity()
                {
                    EntityId = id,
                    JsonData = JsonUtility.ToJson(persistable)
                };
            }

            DataEntitiesContainer container = new DataEntitiesContainer()
            {
                // Convert the values in the dictionary to an array. Dictionary enumeration is slow, but Count is expected to be negligible.
                DataEntities = _dataEntities.Select((kvp, _) => kvp.Value).ToArray()
            };

            // Push the container into the persistence mechanism for storage, under the given key.
            _persistenceMechanism.Save(_persistentContainerKey, container);
        }

        /// <summary>
        /// Deletes all saved state.
        /// </summary>
        public void Purge()
        {
            //TODO
            throw new NotImplementedException();
        }

        // The only reason we need this struct is that JsonUtility is unable to process raw arrays; data has to be contained. Thanks, Unity.
        [Serializable]
        protected struct DataEntitiesContainer
        {
            public DataEntity[] DataEntities;
        }

        [Serializable]
        protected struct DataEntity
        {
            public int EntityId;
            public string JsonData;
        }
    }
}