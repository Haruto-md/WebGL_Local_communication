using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.IO;

public class LocalPythonCommunicater : MonoBehaviour
{
    private const string targetURL = "http://localhost:8000/myapp/models/TTS/"; // APIのエンドポイントのURLを指定
    private AudioSource audioSource;

    public string textData;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void SendRequest(string textData)
    {
        string requestData = "{\"input_text\": \"" + textData + "\"}"; // リクエストデータの作成

        StartCoroutine(PostRequest(requestData));
    }

    private IEnumerator PostRequest(string requestData)
    {
        // リクエストの作成
        UnityWebRequest request = new UnityWebRequest(targetURL, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(requestData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // リクエストの送信
        yield return request.SendWebRequest();

        // レスポンスの処理
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("APIリクエストが失敗しました: " + request.error);
        }
        else
        {
            // レスポンスの取得
            string responseText = request.downloadHandler.text;
            // 受け取った音声データの処理（例: AudioClipに変換して再生）
            ProcessAudioData(responseText);
        }
    }

    private void ProcessAudioData(string audioData)
    {
        // JSONデータのデシリアライズ
        AudioData audio = JsonUtility.FromJson<AudioData>(audioData);

        // 音声データの取得
        float[] audioSamples = audio.audio_data;
        int samplingRate = audio.sampling_rate;

        // AudioClipの作成
        AudioClip audioClip = AudioClip.Create("AudioClip", audioSamples.Length, 1, samplingRate, false);
        audioClip.SetData(audioSamples, 0);

        // AudioClipの再生などの処理を行う
        // 例えば、オーディオソースを作成して再生する場合：
        audioSource.clip = audioClip;
        audioSource.Play();
    }

    [System.Serializable]
    private class AudioData
    {
        public float[] audio_data;
        public int sampling_rate;
    }
}
