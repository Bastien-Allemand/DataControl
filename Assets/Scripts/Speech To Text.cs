using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Collections.Generic;

public class SpeechToText : MonoBehaviour
{
    private DictationRecognizer dictationRec;
    private List<string> results = new List<string>();

    void Start()
    {
        dictationRec = new DictationRecognizer();

        dictationRec.DictationResult += (text, confidence) =>
        {
            results.Add(text);
            Debug.Log("Recognized: " + text);
        };

        dictationRec.Start();
    }

    public string FetchText()
    {
        if (results.Count == 0) return null;

        string text = results[0];
        results.RemoveAt(0);
        return text;
    }

    private void OnDestroy()
    {
        if (dictationRec != null)
        {
            dictationRec.Stop();
            dictationRec.Dispose();
        }
    }
}
