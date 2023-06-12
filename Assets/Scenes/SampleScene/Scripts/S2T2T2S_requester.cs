using UnityEngine;
using System.Collections;
using System;
using System.Text;
using UnityEngine.UI;
using Newtonsoft.Json;
using UnityEngine.Networking;

public class S2T2T2S_requester : MonoBehaviour
{
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private int frequency = 16000;
    [SerializeField] public string[] chatJsonFile;
    
    public string url;
    private AudioClip[] audioClips;
    private AudioSource audioSource;
    private bool isRecording = false;
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
        audioClip = Microphone.Start(Microphone.devices[0], true, 15, frequency);
    }

    void StopRecording()
    {
        isRecording = false;
        Microphone.End(Microphone.devices[0]);
        var result = StartCoroutine(ComunicateAPI());
        Debug.Log("requested maybe");
    }
    
    IEnumerator ComunicateAPI()
    {
        // テキストデータのシリアライズ
        string chat_data = JsonConvert.SerializeObject(chatJsonFile);

        byte[] text_binary_Data = Encoding.UTF8.GetBytes(chat_data);
        byte[] TA_delimiter = Encoding.UTF8.GetBytes("===Text_Audio_Delimiter===");
        float[] audioData = new float[audioClip.samples];
        audioClip.GetData(audioData, 0);

        byte[] AS_delimiter = Encoding.UTF8.GetBytes("===Audio_SR_Delimiter===");
        byte[] sampling_rate = BitConverter.GetBytes(frequency);
        byte[] END_delimiter = Encoding.UTF8.GetBytes("===END===");

        byte[] binary_data = new byte[text_binary_Data.Length + TA_delimiter.Length + audioData.Length * 4 + AS_delimiter.Length + sampling_rate.Length];
        int offset = 0;

        Array.Copy(text_binary_Data, 0, binary_data, offset, text_binary_Data.Length);
        offset += text_binary_Data.Length;

        Array.Copy(TA_delimiter, 0, binary_data, offset, TA_delimiter.Length);
        offset += TA_delimiter.Length;

        Buffer.BlockCopy(audioData, 0, binary_data, offset, audioData.Length * 4);
        offset += audioData.Length * 4;

        Array.Copy(AS_delimiter, 0, binary_data, offset, AS_delimiter.Length);
        offset += AS_delimiter.Length;

        Array.Copy(sampling_rate, 0, binary_data, offset, sampling_rate.Length);

        UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
        UploadHandlerRaw uploadHandler = new UploadHandlerRaw(binary_data);
        request.uploadHandler = uploadHandler;
        request.downloadHandler = new DownloadHandlerBuffer();
        //request.SetRequestHeader("Content-Type", "application/octet-stream");

        // リクエストの送信
        yield return request.SendWebRequest();

        var respondedBinaryData = new byte[] { };
        var audioDataBinary = new byte[] { };
        while (!request.isDone)
        {
            byte[] chunk = request.downloadHandler.data;
            Array.Copy(chunk, 0, respondedBinaryData, 0, chunk.Length);

            byte[][] result = SplitBinaryData(respondedBinaryData, END_delimiter);
            if (result is not null)
            {
                // 分割された結果を正しく処理する
                audioDataBinary = result[0];
                respondedBinaryData = result[1];
            }

            byte[][] audioResult = SplitBinaryData(audioDataBinary, AS_delimiter);
            if (audioResult is not null)
            {
                audioDataBinary = audioResult[0];
                byte[] samplingRateBinary = audioResult[1];

                int samplingRate = BitConverter.ToInt32(samplingRateBinary, 0);
                float[] respondedAudioData = DecodeAudioData(audioDataBinary);
                AudioClip[] singleAudioClip = new AudioClip[]{ CreateAudioClip(respondedAudioData, samplingRate)};
                Array.Copy(singleAudioClip,0, audioClips, audioClips.Length,1);
            }

        }
        yield return null;
    }
    IEnumerator PlaySpeech(AudioClip audioClip)
    {
        audioSource.clip = audioClip;
        audioSource.Play();
        
        yield return 0;
    }
    byte[][] SplitBinaryData(byte[] binaryData, byte[] delimiter)
    {
        var binaryList = new byte[2][];
        int delimiterIndex = IndexOfDelimiter(binaryData, delimiter);
        if (delimiterIndex != -1)
        {
            Array.Copy(binaryData, 0, binaryList[1], 0, delimiterIndex + 1);
            Array.Copy(binaryData, 0, binaryList[2], 0, binaryData.Length - delimiterIndex - 1);
            return binaryList;
        }
        else
        {
            return null;
        }

    }

    int IndexOfDelimiter(byte[] data, byte[] delimiter)
    {
        for (int i = 0; i < data.Length - delimiter.Length + 1; i++)
        {
            bool found = true;
            for (int j = 0; j < delimiter.Length; j++)
            {
                if (data[i + j] != delimiter[j])
                {
                    found = false;
                    break;
                }
            }

            if (found)
                return i;
        }

        return -1;
    }


    float[] DecodeAudioData(byte[] audioData)
    {
        // バイナリデータをfloat配列に変換する処理
        float[] decodedAudioData = new float[audioData.Length / 4];
        Buffer.BlockCopy(audioData, 0, decodedAudioData, 0, audioData.Length);
        return decodedAudioData;
    }

    AudioClip CreateAudioClip(float[] audioData,int sampleRate)
    {
        // float配列からAudioClipを生成する処理
        AudioClip audioClip = AudioClip.Create("YourAudioClip", audioData.Length, 1, sampleRate, false);
        audioClip.SetData(audioData, 0);
        return audioClip;
    }
}