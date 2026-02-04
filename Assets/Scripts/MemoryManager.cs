using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public struct Content
{
    public string data;
    public float importance;
    public ulong timeStamp;
}
public struct Category
{
    public string name;
    public List<Content> contents;
}
[System.Serializable]
public class MemoryData
{
    public List<Category> types = new List<Category>();
    public ulong m_timeStamp;
}

public class MemoryManager : MonoBehaviour {
    public MemoryData memoryData = new MemoryData();
    private string m_filePath;
    private float m_saveTime;
    private void Start() {

        m_filePath = Path.Combine(Application.persistentDataPath, "Memory");
        Directory.CreateDirectory(m_filePath);
        Extract();
    }
    private void Update()
    {
        m_saveTime += Time.deltaTime;
        AutoSaveRoutine();
    }
    /// <summary>
    /// Extract existing memory from file
    /// </summary>
    private void Extract() {

        string file = Path.Combine(m_filePath, "memory.json");

        if (!File.Exists(file))
        {
            memoryData = new MemoryData();
            return;
        }
        Debug.Log("Extracting memory data from file...");
        string json = File.ReadAllText(file);
        memoryData = JsonUtility.FromJson<MemoryData>(json);
        Debug.Log("Memory data extracted successfully.");
    }

    /// <summary>
    /// Store current memory to file
    /// </summary>
    private void Store() {

        string json = JsonUtility.ToJson(memoryData, true);
        string file = Path.Combine(m_filePath, "memory.json");

        File.WriteAllText(file, json);
        Debug.Log("Data Saved");
    }

    public void AddType(string _name) {

        if (memoryData.types.Exists(t => t.name == _name))
        {
            Debug.LogError($"Type {_name} already exists.");
            return;
        }
        Category newCategory = new Category { name = _name, contents = new List<Content>() };
        memoryData.types.Add(newCategory);
    }

    public void AddContent(string _type, string _data, float _importance) {

        Category category = memoryData.types.Find(t => t.name == _type);
        if (category.name == null)
        {
            Debug.LogError($"Type {_type} does not exist.");
            return;
        }
        Content newContent = new Content { data = _data, importance = _importance, timeStamp = memoryData.m_timeStamp };

        category.contents.Add(newContent);

        IncrementTime();
    }

    public void AddImportance(string type, string data, float importance) {

        Category category = memoryData.types.Find(t => t.name == type);
        if (category.name == null) {

            Debug.LogError($"Type {type} does not exist.");
            return;
        }
        Content content = category.contents.Find(c => c.data == data);
        if (content.data == null) {

            Debug.LogError($"Content {data} does not exist in type {type}.");
            return;
        }
        content.importance += importance;

        IncrementTime();
    }

    private void IncrementTime() {

        memoryData.m_timeStamp = (memoryData.m_timeStamp == UInt64.MaxValue) ? 0 : memoryData.m_timeStamp + 1;
    }

    public List<string> GetTypes() {

        List<string> result = new List<string>();
        foreach (var category in memoryData.types){
            result.Add(category.name);
        }
        return result;
    }
    public ulong GetTimeStamp() => memoryData.m_timeStamp;

    //save system as much as possible to prevent data loss
    private void AutoSaveRoutine() {
        if (m_saveTime > 600000) {
            Debug.Log("Auto Saving");
            Store();
        }
    }
    private void OnApplicationQuit()
    {
        Store();
    }
    private void OnApplicationFocus(bool focus)
    {
        if (!focus)
        {
            Store();
        }
    }
    private void OnApplicationPause(bool pause) {
        if (pause) {
            Store();
        }
    }
}
