using System.IO;
using UnityEngine;
using System.Collections.Generic;

public class MemoryManager : MonoBehaviour
{
    private List<string> m_types = new List<string>();
    private string m_folderPath;
    private string m_indexPath;
    private int m_timeStamp = 0;

    private void Start()
    {
        m_folderPath = Path.Combine(Application.persistentDataPath, "Memory");
        Directory.CreateDirectory(m_folderPath);

        m_indexPath = Path.Combine(m_folderPath, "Index.me");

        if (!File.Exists(m_indexPath))
        {
            File.WriteAllLines(m_indexPath, new string[] { "0" });
        }

        ParseIndex();
    }

    private void ParseIndex()
    {
        m_types.Clear();

        string[] lines = File.ReadAllLines(m_indexPath);
        m_timeStamp = int.Parse(lines[0]);

        for (int i = 1; i < lines.Length; i++)
        {
            m_types.Add(lines[i]);
        }
    }

    public void AddType(string name)
    {
        if (m_types.Contains(name)) return;

        m_types.Add(name);
        File.AppendAllText(m_indexPath, name + "\n");

        string typeFile = Path.Combine(m_folderPath, name);
        if (!File.Exists(typeFile))
            File.Create(typeFile).Close();
    }

    public void AddContent(string type, string data, int importance)
    {
        if (!m_types.Contains(type))
        {
            Debug.LogError($"Type {type} not registered.");
            return;
        }

        string path = Path.Combine(m_folderPath, type);
        File.AppendAllText(path, $"{data}/{importance}/{m_timeStamp}\n");

        IncrementTime();
    }

    public void AddImportance(string type, string data, int importance)
    {
        string path = Path.Combine(m_folderPath, type);
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
