using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
public class OllamaRequest
{
    public string model;
    public string prompt;
    public bool stream;
}
[System.Serializable]
public class OllamaResponse
{
    public string model;
    public string created_at;
    public string response;
    public bool done;
}
public enum TypeOfPrompt
{
    None,
    GETTYPE,
    GETANSWER,
    GETSTORE,
}
public struct MsgType
{
    public string prompt;
    public TypeOfPrompt type; 
}
public class OllamaBridge : MonoBehaviour
{
    [SerializeField] MemoryManager m_memoryManager;

    public List<MsgType> ToSend = new List<MsgType>();
    public List<MsgType> ToParse = new List<MsgType>();

    public void Start()
    {

    }
    public void Update()
    {
        if (ToSend.Count > 0)
        {
            MsgType msg = ToSend[0];
            ToSend.RemoveAt(0);
            StartCoroutine(SendToLLM(msg));
        }
        if (ToParse.Count > 0)
        {
            switch(ToParse[0].type)
            {
                case TypeOfPrompt.None:
                    SendDemandType(ToParse[0].prompt);
                    break;
            }
            ToParse.RemoveAt(0);
        }
    }
    public void SendDemandType(string prompt)
    {
        var types = m_memoryManager.GetTypes();
        var shell = new StringBuilder();

        shell.AppendLine("### Role");
        shell.AppendLine("You are a precise data extraction and classification engine.");
        shell.AppendLine("\n### Available Categories");

        foreach (var type in types)
        {
            shell.AppendLine($"- {type}");
        }

        shell.Append(@"
        ### Constraints
        * Return ONLY a valid JSON object.
        * Do NOT include markdown code blocks.
        * If no categories apply, return an empty 'selected_categories' array.
        * Ensure 'confidence_score' is a numerical float.

        ### Output Schema
        {
        ""selected_categories"": [
            {
                ""category_name"": ""string"",
                ""reasoning"": ""string"",
                ""extracted_data"": ""string""
            }
        ],
        ""confidence_score"": 0.0
        }

        ### User Input
        > ");
        shell.Append(prompt);

        MsgType msg;
        msg.type = TypeOfPrompt.GETTYPE;
        msg.prompt = shell.ToString();
        ToSend.Add(msg);
    }
    public IEnumerator SendToLLM(MsgType _msg)
    {
        string json = BuildJson("mistral", _msg.prompt, false);
        UnityEngine.Networking.UnityWebRequest request = CreateRequest(json);

        // This tells Unity to pause this function until the LLM returns the data
        yield return request.SendWebRequest();

        if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            string respons = ParseOllamaResponse(request.downloadHandler.text);
            MsgType msgType;
            msgType.type = _msg.type;
            msgType.prompt = respons;
            ToParse.Add(msgType);
        }
        else
            Debug.LogError("Error: " + request.error);   
    }

    private string BuildJson(string _model, string _prompt, bool _stream)
    {
        return JsonUtility.ToJson(new OllamaRequest
        {
            model = _model,
            prompt = _prompt,
            stream = _stream
        });
    }
    private string ParseOllamaResponse(string json)
    {
        // Convert the JSON string into our C# object
        OllamaResponse data = JsonUtility.FromJson<OllamaResponse>(json);

        // Return just the text part
        return data != null ? data.response : "Error: Could not parse response";
    }
    private UnityEngine.Networking.UnityWebRequest CreateRequest(string json)
    {
        var request = new UnityEngine.Networking.UnityWebRequest("http://127.0.0.1:11434/api/generate", "POST");
        request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        return request;
    }
}
