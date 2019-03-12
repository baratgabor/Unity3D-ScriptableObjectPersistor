# Persistent ScriptabeObject container for Unity3D

## *Save into PlayerPrefs, JSON file, binary file or AES-encrypted file*

This is a submodule I developed for my own projects. It's a relatively new addition; still under development. But if you find it useful, feel free to use it for any purpose.

'Transparent' means you don't have to call any of save or load methods:

- **Automatic state save:** The states of the selected  `ScriptableObjects` are automatically serialized and stored through the selected persistence mechanism.
  - **When?** When `OnDestroy()` triggers on the container.
- **Automating state restore:** The state is automatically restored when the persistent container is instantiated.
  - **When?** When `Awake()` triggers on the container.

(But optionally you can disable this automatic save and restore, and call the publicly exposed simple `SaveData()` and `LoadData()` methods on the `PersistentContainer` instance.)

The *persistence mechanism* mentioned above is essentially a `ScriptableObject`-based plug-and-play component you can drag into the appropriate slot on the `PersistentContainer`. This component determines how and where the state of your `ScriptableObjects` will be saved.

## Usage 101

1. #### Add the `PersistentContainer` component to a gameobject.

2. #### Drop some `ScriptableObjects` into the container.

## Usage detailed

1. **Create a container:** Add the `PersistentContainer` component to a `GameObject` in your scene (for example to the object responsible for scene management, or an object marked with `DontDestroyOnLoad`).
2. **Create a persistence mechanism:** Create a `PersistenceMechanism` by right clicking in the project tree, and selecting an item from the **Create** > **Persistence Mechanisms** menu. The following persistence mechanisms are available:
   - PlayerPrefs
   - JSON file
   - Binary file
   - Encrypted binary file
3. **Set a filename (optional):** If you created a file-based persistence mechanism, you can specify the **filename** to use on the Inspector pane of the persistence mechanism itself.
4. **Fill the container:** Drag the `ScriptableObjects` you want to save into the **Persistence List** array of the `PersistentContainer` component.

## Implementation details

### Internal identification of ScriptableObjects

The most important thing to mention is that **instance identification is based on Unity's internal InstanceID** (the ID you can see in the `meta` files, and in the debug mode of the inspector). If you use your classes irresponsibly, and this instance ID changes, the persistence system won't be able correlate the saved state with the given instance, and the data will stop being restored.

But this is the exact same 'gracefulness' Unity itself handles object states with. :D So I suppose you're already familiar with this.

I actually made some efforts to add a **fail-safe against potential data loss**: The persistence system *does not delete orphaned saved data*; it continues to store it.

The same mechanism will apply if you remove `ScriptableObjects` from the `PersistentContainer` list: the removed instances' state won't be deleted, so if you later put these instances back to the container, their state can restore again (provided that the instance ID is still the same).

I don't know what would be the best way to granularly handle this orphaned data; what I know that I'll soon implement at least a `Purge()` method to clear the storage.

### Internal identification of the container itself

When you use the `PlayerPrefsPersistenceMechanism`, the container data is saved under a key that is currently automatically derived from the `InstanceID` of the `PersistentContainer` component. This means if your component's `InstanceID` changes, it won't find the saved data. *(In this case the `InstanceID` is the Id you can see in `.asset` files , or what is saved in the `.scene` file internally, if I'm not mistaken.)*

When you use file-based persistence mechanism, you can specify the filename in the persistence mechanism's Inspector pane. In case of the JSON and binary file format, no key is used. In case of the AES-128 encrypted file format, the system derives an encryption key from the `InstanceId` of the `PersistenceContainer` component.









