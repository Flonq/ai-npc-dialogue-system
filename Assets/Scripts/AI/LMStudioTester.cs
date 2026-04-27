using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class LMStudioTester : MonoBehaviour
{
    [SerializeField] private string endpoint = "http://localhost:1234/v1/chat/completions";
    [SerializeField] private string model = "qwen2.5-0.5b-instruct";
    [TextArea(2, 4)]
    [SerializeField] private string userMessage = "Say hello in one short sentence.";

    private void Start()
    {
        StartCoroutine(SendRequest());
    }

    private IEnumerator SendRequest()
    {
        var requestBody = new ChatRequest
        {
            model = model,
            max_tokens = 60,
            temperature = 0.7f,
            messages = new[]
            {
                new ChatMessage { role = "user", content = userMessage }
            }
        };

        string json = JsonConvert.SerializeObject(requestBody);
        byte[] bodyBytes = Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest req = new UnityWebRequest(endpoint, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(bodyBytes);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = 60;

            Debug.Log("[LMStudio] Request sent...");
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[LMStudio] Request failed: {req.error}\n{req.downloadHandler.text}");
                yield break;
            }

            string responseJson = req.downloadHandler.text;
            Debug.Log($"[LMStudio] Raw response:\n{responseJson}");

            try
            {
                var parsed = JsonConvert.DeserializeObject<ChatResponse>(responseJson);
                if (parsed != null && parsed.choices != null && parsed.choices.Length > 0)
                {
                    string reply = parsed.choices[0].message.content;
                    Debug.Log($"[LMStudio] Reply: {reply}");
                }
                else
                {
                    Debug.LogWarning("[LMStudio] Response parsed but no choices found.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[LMStudio] Parse error: {ex.Message}");
            }
        }
    }

    [System.Serializable]
    private class ChatRequest
    {
        public string model;
        public ChatMessage[] messages;
        public int max_tokens;
        public float temperature;
    }

    [System.Serializable]
    private class ChatMessage
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    private class ChatResponse
    {
        public Choice[] choices;
    }

    [System.Serializable]
    private class Choice
    {
        public ChatMessage message;
    }
}