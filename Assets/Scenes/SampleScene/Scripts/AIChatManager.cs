using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;


public class AIChatManager : MonoBehaviour
{
    public GameObject ChatBot;
    public GameObject ChatLogBox;
    public GameObject text2Speech;
    private OpenAIChatBot chatBot;
    private ChatNoter chatNoter;
    public TextAsset instruction;

    private void Start()
    {
        chatBot = ChatBot.GetComponent<OpenAIChatBot>();
        chatNoter = new ChatNoter();

        chatNoter.addDialogue(new OpenAIChatCompletionAPI.Message() { role = "system", content = instruction.text });
    }
    public void OnChatSubmitted(string inputText)
    {
        chatNoter.addDialogue(new OpenAIChatCompletionAPI.Message() { role = "user", content = inputText });
        ChatLogBox.GetComponent<ChatLogBoxManager>().displayMessages(chatNoter.getDialogues());
        StartCoroutine(OnEndEdit(inputText));
    }
    public IEnumerator OnEndEdit(string inputText)
    {
        Debug.Log("Sent Data.");
        // 入力されたテキストを取得して処理

        var chatCompletionAPI = new OpenAIChatCompletionAPI(chatBot.openai_api_key);

        var request = chatCompletionAPI.CreateCompletionRequest(
            new OpenAIChatCompletionAPI.RequestData() { messages = chatNoter.getDialogues() }
        );

        yield return request.Send();
        Debug.Log("Received Response.");
        if (request.IsError == true)
        {
            Debug.Log(request.Error);
        }
        else
        {
            Debug.Log(request.Response.choices[0].message.content);
            OpenAIChatCompletionAPI.Message message = request.Response.choices[0].message;
            chatNoter.addDialogue(new OpenAIChatCompletionAPI.Message() { role = "assistant", content = message.content });
            ChatLogBox.GetComponent<ChatLogBoxManager>().displayMessages(chatNoter.getDialogues());
            text2Speech.GetComponent<LocalPythonCommunicater>().SendRequest(message.content);
        }
    }
}
