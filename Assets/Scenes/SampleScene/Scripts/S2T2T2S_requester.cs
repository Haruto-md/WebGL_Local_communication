using UnityEngine;
using System.Collections;
using System;
using System.Text;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Networking;
using My.Communication;

public class S2T2T2S_requester : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private int frequency = 16000;
    [SerializeField] public TextAsset textAsset;
    
    public string url;
    public List<AudioClip> receivedAudioClips = null;
    private AudioSource audioSource;
    private bool isRecording = false;
    public bool isRequesting = false;
    public Button recordButton;

    // Start is called before the first frame update
    void Start()
    {
        recordButton.onClick.AddListener(ToggleRecording);
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame

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
        Debug.Log(Microphone.devices[0]);
        audioClip = Microphone.Start(Microphone.devices[0], true, 15, frequency);
    }

    void StopRecording()
    {
        isRecording = false;
        recordButton.interactable = false;
        Microphone.End(Microphone.devices[0]);
        StartCoroutine(ComunicateAPI());
        Debug.Log("requested maybe");
    }
    
    IEnumerator ComunicateAPI()
    {
        float[] audioData = new float[audioClip.samples];
        audioClip.GetData(audioData, 0);
        // バイト配列に変換
        byte[] audioBinaryData = new byte[audioData.Length * 4];  // floatは4バイト
        Buffer.BlockCopy(audioData, 0, audioBinaryData, 0, audioBinaryData.Length);

        var form = new WWWForm();
        form.AddField("role1","user");
        form.AddField("content1", textAsset.text);
        form.AddField("sampling_rate", audioClip.frequency);

        form.AddBinaryData("audio_data", audioBinaryData, "audio.wav", "audio/wav");

        // UnityWebRequestを作成し、FormDataを設定
        UnityWebRequest request = UnityWebRequest.Post(url, form);
        request.downloadHandler = new StreamingDownloadHandler(this.gameObject);

        // リクエストを送信し、レスポンスを待機
        isRequesting = true;
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            isRequesting = false;
            Debug.Log("request complete!");
            recordButton.interactable = true;
        }
    }

    public IEnumerator playAudioSequentially()
    {
        yield return null;

        //1.Loop through each AudioClip
        for (int i = 0;isRequesting==true&&i< receivedAudioClips.Count; i++)
        {
            Debug.Log("Playing a Clip, index: " + i);
            var currentAudioClip = receivedAudioClips[i];
            audioSource.clip = currentAudioClip;
            //3.Play Audio
            audioSource.Play();

            //4.Wait for it to finish playing
            while (audioSource.isPlaying)
            {
                yield return null;
            }
            while (receivedAudioClips.Count - 1 <= i && isRequesting)
            {
                yield return null;
            }
            
        }
        Debug.Log("Finish Playing");
        receivedAudioClips = null;
}
}
namespace My.Communication
{
    [System.Serializable]
    public class AudioClipData
    {
        public float[] audio_sample_data;
    }
    public class StreamingDownloadHandler : DownloadHandlerScript
    {
        S2T2T2S_requester requester;
        bool isStartedPlayList = false;
        public StreamingDownloadHandler(GameObject gameObject)
        {
            this.requester = gameObject.GetComponent<S2T2T2S_requester>();
        }
        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            float[] samples = new float[data.Length / 4];
            Buffer.BlockCopy(data,0,samples,0,data.Length);
            int samplingRate = 20500;
            AudioClip singleAudioClip = CreateAudioClip(samples, samplingRate);
            requester.receivedAudioClips.Add(singleAudioClip);

            Debug.Log("Extend audioClips.");
            if (!isStartedPlayList)
            {
                requester.StartCoroutine(requester.playAudioSequentially());
                isStartedPlayList = true;
            }
            return base.ReceiveData(data, dataLength);
        }
        AudioClip CreateAudioClip(float[] audioData, int samplingRate)
        {
            // float配列からAudioClipを生成する処理
            AudioClip audioClip = AudioClip.Create("receivedAudioClip_" + Time.time, audioData.Length, 1, samplingRate, false);
            audioClip.SetData(audioData, 0);
            return audioClip;
        }
    }
}