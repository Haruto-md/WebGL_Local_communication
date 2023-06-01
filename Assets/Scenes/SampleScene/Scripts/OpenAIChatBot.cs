using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.IO;

public class OpenAIChatBot : MonoBehaviour
{
    public string openai_api_key { get; private set; }


    IEnumerator LoadTextFile(string filePath)
    {
        string url = Path.Combine(Application.streamingAssetsPath, filePath);
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string text = www.downloadHandler.text;
            // テキストファイルの内容を使用する処理をここに記述する
            Debug.Log(text);
        }
        else
        {
            Debug.Log("[ERROR]Failed to load text file: " + www.error);
        }
    }
    void Start()
    {
#if UNITY_EDITOR
        // Unity Editorのときの処理
        // ここにUnity Editorでの特定の挙動を記述します
        openai_api_key = "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";
#else
    // 実行時の処理
    // ここに実行時の挙動を記述します
        string apiKey_filePath = Path.Combine(Application.streamingAssetsPath, "openai_api_key.text");
        Debug.Log(apiKey_filePath);
        StartCoroutine(LoadTextFile(apiKey_filePath));
#endif
    }
}

public class ChatNoter
{
    private List<OpenAIChatCompletionAPI.Message> messages = new List<OpenAIChatCompletionAPI.Message>(){};
    public ChatNoter()
    {

    }
    public ChatNoter(OpenAIChatCompletionAPI.Message messages)
    {
        this.messages.Add(messages);
    }
    public ChatNoter(List<OpenAIChatCompletionAPI.Message> messages)
    {
        this.messages = messages;
    }

    public void addDialogue(OpenAIChatCompletionAPI.Message message)
    {
        this.messages.Add(message);
    }

    public List<OpenAIChatCompletionAPI.Message> getDialogues()
    {
        return this.messages;
    }
    public void deleteConversation(int index)
    {
        this.messages.RemoveAt(index);
    }
}


// https://platform.openai.com/docs/api-reference/chat/create
public class OpenAIChatCompletionAPI
{
    const string API_URL = "https://api.openai.com/v1/chat/completions";
    private string apiKey;
    JsonSerializerSettings settings = new JsonSerializerSettings();


    public OpenAIChatCompletionAPI(string apiKey)
    {
        this.apiKey = apiKey;
        settings.NullValueHandling = NullValueHandling.Ignore;
    }

    public RequestHandler CreateCompletionRequest(RequestData requestData)
    {
        var json = JsonConvert.SerializeObject(requestData, settings);

        byte[] data = System.Text.Encoding.UTF8.GetBytes(json);

        var request = new UnityWebRequest(API_URL, "POST");
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        request.SetRequestHeader("Content-Type", "application/json");
        request.uploadHandler = new UploadHandlerRaw(data);
        request.downloadHandler = new DownloadHandlerBuffer();

        return new RequestHandler(request);
    }

    public class RequestHandler
    {
        public bool IsCompleted { get; private set; }
        public bool IsError => Error != null;
        public string Error { get; private set; }
        public ResponseData Response { get; private set; }

        UnityWebRequest request;

        public RequestHandler(UnityWebRequest request)
        {
            this.request = request;
        }

        public IEnumerator Send()
        {
            using (request)
            {
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Error = "[OpenAIChatCompletionAPI] " + request.error + "\n\n" + request.downloadHandler.text;
                }
                else
                {
                    Response = JsonConvert.DeserializeObject<ResponseData>(request.downloadHandler.text);
                }
            }
        }
    }

    [System.Serializable]
    public class RequestData
    {
        public string model = "gpt-3.5-turbo";
        public List<Message> messages;
        public float? temperature = 1.2f; // [0.0 - 2.0]
        public float? top_p = null;
        public int? n = 4;
        public bool? stream = null;
        public List<string> stop = null;
        public int? max_tokens = 150;
        public float? presence_penalty = null;
        public float? frequency_penalty = null;
        public Dictionary<int, int> logit_bias = null;
        public string user = null;
    }

    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    public class Usage
    {
        public int prompt_tokens;
        public int completion_tokens;
        public int total_tokens;
    }

    [System.Serializable]
    public class Choice
    {
        public Message message;
        public string finish_reason;
        public int index;
    }

    [System.Serializable]
    public class ResponseData
    {
        public string id;
        public string @object;
        public int created;
        public string model;
        public Usage usage;
        public List<Choice> choices;
    }
}
