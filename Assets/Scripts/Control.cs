using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Control : MonoBehaviour
{
    [SerializeField] MemoryManager m_memoryManager;
    [SerializeField] OllamaBridge m_ollamaBridge;
    [SerializeField] SpeechToText m_speechToText;

    public List<string> m_prompts;
    public List<string> m_responses;

    private void Start()
    {
        m_prompts = new List<string>();

        Task.Run(SendPrompt);

    }
    private void Update()
    {
        string text;
        while (!string.IsNullOrEmpty(text = m_speechToText.FetchText()))
        {
            m_prompts.Add(text);
            Debug.Log("Stored: " + text);
        }

    }

    void SendPrompt()
    {
        while (true)
        {
            while (m_prompts.Count != 0)
            {
                m_ollamaBridge.
            }
        }
    }
}
