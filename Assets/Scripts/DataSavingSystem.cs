using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class MemoryEntry
{
    public string data;
    public float importance;
}

[System.Serializable]
public class MemoryCategory
{
    public List<MemoryEntry> entries = new List<MemoryEntry>();
}

[System.Serializable]
public class MemoryStore
{
    public Dictionary<string, MemoryCategory> categories =
        new Dictionary<string, MemoryCategory>();
}
public class DataSavingSystem : MonoBehaviour
{
    public MemoryStore store = new MemoryStore();
    private string path;

    void Awake()
    {
        path = Path.Combine(Application.persistentDataPath, "memory.json");
        Load();
    }

    public List<string> AllTypes()
    {
        return new List<string>(store.categories.Keys);
    }

    public List<MemoryEntry> RetrieveByType(string type)
    {
        if (store.categories.ContainsKey(type))
            return store.categories[type].entries;

        return new List<MemoryEntry>();
    }

    public void AddData(string type, string data, float importance)
    {
        if (!store.categories.ContainsKey(type))
            store.categories[type] = new MemoryCategory();

        store.categories[type].entries.Add(new MemoryEntry
        {
            data = data,
            importance = importance
        });

        Save();
    }

    void Save()
    {
        string json = JsonUtility.ToJson(store, true);
        File.WriteAllText(path, json);
    }

    void Load()
    {
        if (!File.Exists(path)) return;

        string json = File.ReadAllText(path);
        store = JsonUtility.FromJson<MemoryStore>(json);
    }
}