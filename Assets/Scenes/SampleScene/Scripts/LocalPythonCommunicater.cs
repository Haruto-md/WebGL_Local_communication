using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using System.IO;

public class LocalPythonCommunicater : MonoBehaviour
{
    private const string targetURL = "http://localhost:8000/myapp/models/TTS/"; // API�̃G���h�|�C���g��URL���w��
    private AudioSource audioSource;

    public string textData;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void SendRequest(string textData)
    {
        string requestData = "{\"input_text\": \"" + textData + "\"}"; // ���N�G�X�g�f�[�^�̍쐬

        StartCoroutine(PostRequest(requestData));
    }

    private IEnumerator PostRequest(string requestData)
    {
        // ���N�G�X�g�̍쐬
        UnityWebRequest request = new UnityWebRequest(targetURL, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(requestData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        // ���N�G�X�g�̑��M
        yield return request.SendWebRequest();

        // ���X�|���X�̏���
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("API���N�G�X�g�����s���܂���: " + request.error);
        }
        else
        {
            // ���X�|���X�̎擾
            string responseText = request.downloadHandler.text;
            // �󂯎���������f�[�^�̏����i��: AudioClip�ɕϊ����čĐ��j
            ProcessAudioData(responseText);
        }
    }

    private void ProcessAudioData(string audioData)
    {
        // JSON�f�[�^�̃f�V���A���C�Y
        AudioData audio = JsonUtility.FromJson<AudioData>(audioData);

        // �����f�[�^�̎擾
        float[] audioSamples = audio.audio_data;
        int samplingRate = audio.sampling_rate;

        // AudioClip�̍쐬
        AudioClip audioClip = AudioClip.Create("AudioClip", audioSamples.Length, 1, samplingRate, false);
        audioClip.SetData(audioSamples, 0);

        // AudioClip�̍Đ��Ȃǂ̏������s��
        // �Ⴆ�΁A�I�[�f�B�I�\�[�X���쐬���čĐ�����ꍇ�F
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
