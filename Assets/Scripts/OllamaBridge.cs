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

public class OllamaBridge : MonoBehaviour
{

    [SerializeField] MemoryManager m_memoryManager;

    private string m_TypePrompt;
    private string m_mainPrompt;
    private string m_JudgePrompt;
    //information
    private string m_contentlist;
    private string m_promptAnswer;
    private string m_prompt;



    public void Start()
    {
        GetTypelist();
    }

    public string GetTypelist()
    {
        List<string> types = m_memoryManager.GetTypes();
        return "These are all the existing types of data:\n" + string.Join("\n", types) + "\n";

    }
    public string SendPrompt(string prompt)
    {
        //goes throught the protocole for all the prompts
        string temp;
        temp = FindType(prompt);
        List<string> typesneeded = new List<string>(temp.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));

        List<string> data = m_memoryManager.GetDataOfTypes(typesneeded);

    }
    public string FindType(string _demand)
    {
        m_TypePrompt = GetTypelist() + "With these what would be helpful to respond to" + _demand + "Respond only with types using the format: type\n ";
        string temp = await SendPromptAsync(m_TypePrompt);
        return temp;
    }
    public  string Answer(string _data, string _demand)
    {
        m_contentlist = "These are the data you have:\n" + _data + "\n";
        m_promptAnswer = "Using the above data, answer the following demand: " + _demand + "\n";
        m_prompt = m_contentlist + m_promptAnswer;
        return SendPrompt(m_prompt);
    }
    public string Judge(string _answer, string _demand)
    {
        m_JudgePrompt = "Given the demand: " + _demand + "\n and the answer: " + 
            _answer + "\n select importante information that you would like to store using the format: data,importance(between 0 and 10),type of information\n" +
            "Here are the existing types of information if the one given isnt in the list a new type will be created" + GetTypelist();
        SendPrompt(m_JudgePrompt);
        return 
            
    }
    public Task<string> SendPromptAsync(string prompt)
    {
        var tcs = new TaskCompletionSource<string>();

        // Must be started on main thread
        StartCoroutine(SendPromptCoroutine(prompt, tcs));

        return tcs.Task;
    }
    private IEnumerator SendPromptCoroutine(string prompt,System.Threading.Tasks.TaskCompletionSource<string> tcs)
    {
        string json = BuildJson("mistral", prompt, false);
        var request = CreateRequest(json);

        yield return request.SendWebRequest();

        if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            tcs.TrySetResult(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError(request.error);
            tcs.TrySetException(new Exception(request.error));
        }
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

    private UnityEngine.Networking.UnityWebRequest CreateRequest(string json)
    {
        var request = new UnityEngine.Networking.UnityWebRequest("http://127.0.0.1:11434/api/generate", "POST");
        request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
        request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        return request;
    }
}
