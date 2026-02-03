using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public struct Content
{
    string data;
    float importance;
    int timeStamp;
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
}

public class MemoryManager : MonoBehaviour {
    public MemoryData memoryData = new MemoryData();
    private string m_filePath;
    private int m_timeStamp = 0;

    private void Start()
    {
        m_filePath = Path.Combine(Application.persistentDataPath, "Memory");
        Directory.CreateDirectory(m_filePath);

        Extract();
    }
    /// <summary>
    /// Extract existing memory from file
    /// </summary>
    private void Extract()
    {
        string file = Path.Combine(m_filePath, "memory.json");

        if (!File.Exists(file))
        {
            memoryData = new MemoryData();
            Store(); // optional: create the file immediately
            return;
        }

        string json = File.ReadAllText(file);
        memoryData = JsonUtility.FromJson<MemoryData>(json);
    }

    /// <summary>
    /// Store current memory to file
    /// </summary>
    private void Store()
    {
        string json = JsonUtility.ToJson(memoryData, true); // pretty print
        string file = Path.Combine(m_filePath, "memory.json");

        File.WriteAllText(file, json);
    }

    public void AddType(string name)
    {
        if (memoryData.types.Exists(t => t.name == name))
        {
            Debug.LogError($"Type {name} already exists.");
            return;
        }
        Category newCategory = new Category { name = name, contents = new List<Content>() };
        memoryData.types.Add(newCategory);
        Store();
    }

    public void AddContent(string type, string data, int importance)
    {
        if (!m_types.Contains(type))
        {
            Debug.LogError($"Type {type} not registered.");
            return;
        }

        string path = Path.Combine(m_filePath, type);
        File.AppendAllText(path, $"{data}/{importance}/{m_timeStamp}\n");

        IncrementTime();
    }

    public void AddImportance(string type, string data, int importance)
    {
        string path = Path.Combine(m_filePath, type);
        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].StartsWith(data + "/"))
            {
                lines[i] = $"{data}/{importance}/{m_timeStamp}";
                break;
            }
        }

        File.WriteAllLines(path, lines);
        IncrementTime();
    }

    private void IncrementTime()
    {
        m_timeStamp++;
        string[] index = File.ReadAllLines(m_indexPath);
        index[0] = m_timeStamp.ToString();
        File.WriteAllLines(m_indexPath, index);
    }

    public List<string> GetTypes() => new List<string>(m_types);
    public int GetTimeStamp() => m_timeStamp;
}
