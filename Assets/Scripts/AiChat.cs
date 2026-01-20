using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Windows.Speech;



public class AiChat : MonoBehaviour
{
    public TMP_Text aiTextDisplay;
    public DataSavingSystem dataSystem;
    private DictationRecognizer dictationRecognizer;
    private bool isListening = false;
    private string lastRecognizedText = "";

    [System.Serializable]
    public class OllamaRequest
    {
        public string model;
        public string prompt;
        public bool stream;
    }

    void Start()
    {
        dictationRecognizer = new DictationRecognizer();

        dictationRecognizer.DictationResult += (text, confidence) =>
        {
            lastRecognizedText = text;
            Debug.Log("STT: " + text);
        };

        dictationRecognizer.DictationComplete += (cause) =>
        {
            if (!string.IsNullOrWhiteSpace(lastRecognizedText))
            {
                aiTextDisplay.text += "\n\nYou: " + lastRecognizedText;
                StartCoroutine(SendTextToOllama(lastRecognizedText));
                lastRecognizedText = "";
            }
        };

        dictationRecognizer.DictationError += (error, hresult) =>
        {
            Debug.LogError("Dictation error: " + error);
        };

        StartListening();
    }

    void Update()
    {


        if (Input.GetKeyDown(KeyCode.V) && !isListening)
        {
            StartListening();
        }

        if (Input.GetKeyUp(KeyCode.V) && isListening)
        {
            StopListening();
        }
    }

    void StartListening()
    {
        dictationRecognizer.Start();
        isListening = true;
        Debug.Log("Listening...");
    }
    void StopListening()
    {
        dictationRecognizer.Stop();
        isListening = false;
        Debug.Log("Stopped listening");
    }
    IEnumerator SendTextToOllama(string text)
    {
        string type = "misc";

        yield return StartCoroutine(GetMessageType(text, t => type = t));

        var memories = dataSystem.RetrieveByType(type);

        string memoryContext = "";
        foreach (var m in memories)
            memoryContext += "- " + m + "\n";

        string prompt =
            "You are an NPC in a game.\n" +
            "Relevant memory:\n" +
            memoryContext +
            "\nPlayer: " + text +
            "\nReply briefly.";

        var req = new OllamaRequest
        {
            model = "mistral",
            prompt = prompt,
            stream = false
        };

        string json = JsonUtility.ToJson(req);


        using (var www = new UnityEngine.Networking.UnityWebRequest(
            "http://127.0.0.1:11434/api/generate", "POST"))
        {
            www.uploadHandler =
                new UnityEngine.Networking.UploadHandlerRaw(
                    System.Text.Encoding.UTF8.GetBytes(json));
            www.downloadHandler =
                new UnityEngine.Networking.DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result ==
                UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                AIResponse resp =
                    JsonUtility.FromJson<AIResponse>(www.downloadHandler.text);

                aiTextDisplay.text += "\nAI: " + resp.response;

                yield return StartCoroutine(JudgeAndStore(text, type));
            }
        }
    }


    IEnumerator GetMessageType(string userMessage,System.Action<string> onResult)
    {
        string types = string.Join(", ", dataSystem.AllTypes());

        string prompt =
            "Existing categories: " + types + "\n" +
            "Message: \"" + userMessage + "\"\n" +
            "Return only the best category name. " +
            "If none fits, return 'misc'.";

        var req = new OllamaRequest
        {
            model = "mistral",
            prompt = prompt,
            stream = false
        };

        string json = JsonUtility.ToJson(req);


        using (var www = new UnityEngine.Networking.UnityWebRequest(
            "http://127.0.0.1:11434/api/generate", "POST"))
        {
            www.uploadHandler =
                new UnityEngine.Networking.UploadHandlerRaw(
                    System.Text.Encoding.UTF8.GetBytes(json));
            www.downloadHandler =
                new UnityEngine.Networking.DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                AIResponse resp =   JsonUtility.FromJson<AIResponse>(www.downloadHandler.text);
                onResult(resp.response.Trim().ToLower());
            }
            else
            {
                Debug.LogError(www.error);
                onResult("misc");
            }
        }
    }
    IEnumerator JudgeAndStore(string userMessage, string type)
    {
        string prompt =
            "Analyze the following player message:\n" +
            "\"" + userMessage + "\"\n" +
            "Return:\n" +
            "Importance (0 to 1):\n" +
            "Normalized memory text:";

        var req = new OllamaRequest
        {
            model = "mistral",
            prompt = prompt,
            stream = false
        };

        string json = JsonUtility.ToJson(req);


        using (var www = new UnityEngine.Networking.UnityWebRequest(
            "http://127.0.0.1:11434/api/generate", "POST"))
        {
            www.uploadHandler =
                new UnityEngine.Networking.UploadHandlerRaw(
                    System.Text.Encoding.UTF8.GetBytes(json));
            www.downloadHandler =
                new UnityEngine.Networking.DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result ==
                UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                string raw = www.downloadHandler.text;

                // VERY basic parsing for now
                float importance = 0.5f;
                string memory = raw;

                dataSystem.AddData(type, memory, importance);
            }
        }
    }

    void OnDestroy()
    {
        if (dictationRecognizer != null)
        {
            if (dictationRecognizer.Status == SpeechSystemStatus.Running)
                dictationRecognizer.Stop();

            dictationRecognizer.Dispose();
        }
    }

    [System.Serializable]
    public class AIResponse
    {
        public string response;
    }
}
