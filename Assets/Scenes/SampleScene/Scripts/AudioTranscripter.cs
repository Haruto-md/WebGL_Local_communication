using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.IO;
using System.Net.Http;
using UnityEngine.Networking;

public class AudioTranscripter : MonoBehaviour
{
    private AudioClip audioClip;
    public string audioFilePath;
    private int frequency = 16000;
    private bool isRecording = false;
    public Button recordButton;
    public GameObject textBuffer;
    private string openai_api_key;

    void Start()
    {
        recordButton.onClick.AddListener(ToggleRecording);
    }
    void ToggleRecording()
    {
        if (!isRecording)
        {
            StartRecording();
        }
        else
        {
            StopRecording();
        }
    }

    void StartRecording()
    {
        isRecording = true;
        audioClip = Microphone.Start(null, true, 15, frequency);
    }

    void StopRecording()
    {
        isRecording = false;
        Microphone.End(null);
#if UNITY_EDITOR
        // Unity Editor�̂Ƃ��̏���
        // ������Unity Editor�ł̓���̋������L�q���܂�
        openai_api_key = "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";
#else
    // ���s���̏���
    // �����Ɏ��s���̋������L�q���܂�
        string apiKey_filePath = Path.Combine(Application.streamingAssetsPath, "openai_api_key.text");
        StartCoroutine(LoadTextFile(apiKey_filePath));
#endif

        var whisperAPI = new WhisperAPI(openai_api_key);
        var respondedText = whisperAPI.SendWhisperRequest("temp.wav");
        textBuffer.GetComponent<Speech2TextBuffer>().setText(respondedText);
        whisperAPI.respondedText = null;

    }

    public static byte[] AudioClipToByteArray(AudioClip clip)
    {
        float[] samples = new float[clip.samples];
        clip.GetData(samples, 0);

        // float�^�̔z����o�C�g�z��ɕϊ�����
        byte[] byteArray = new byte[samples.Length * 4];
        Buffer.BlockCopy(samples, 0, byteArray, 0, byteArray.Length);

        return byteArray;
    }
    // AudioClip��byte[]�ɕϊ�����
}

public class WhisperAPI
{

    // Whisper API�G���h�|�C���gURL
    private const string whisperEndpoint = "https://api.openai.com/v1/audio/transcriptions";
    // ���f����
    private string modelName = "whisper-1";

    public string respondedText { get; set; }

    public string openai_api_key { get; private set; }

    public WhisperAPI(string key)
    {
        openai_api_key = key;
        Debug.Log(openai_api_key);
    }


// API���N�G�X�g�𑗐M����֐�
public string SendWhisperRequest(string filePath)
    {
        string path = Path.Combine(Application.streamingAssetsPath, filePath);
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, whisperEndpoint);
        request.Headers.Add("Authorization", "Bearer "+ openai_api_key);
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(File.ReadAllBytes(path)), "file", Path.GetFileName(path));
        content.Add(new StringContent(modelName), "model");
        content.Add(new StringContent("ja"), "language");
        request.Content = content;
        var response = client.SendAsync(request).Result;
        try
        {
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException e)
        {
            Debug.LogError($"HTTP Request Exception: {e.Message}");
        }
        respondedText = response.Content.ReadAsStringAsync().Result;
        int startIndex = respondedText.IndexOf(":") + 2; // 2��"��:�̕�
        int endIndex = respondedText.LastIndexOf("\"") - startIndex;
        respondedText = respondedText.Substring(startIndex, endIndex);
        Debug.Log(respondedText);

        return respondedText;
    }
}