using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatLogBoxManager : MonoBehaviour
{
    public void displayMessages(List<OpenAIChatCompletionAPI.Message> messages)
    {
        string resultMessage="";
        int index = 0;
        foreach (var message in messages)
        {
            if (index == 0)
            {
            }
            else {
                string role = "user";
                if (message.role == "assistant")
                {
                    role = "ghost";
                }
                    resultMessage = resultMessage + role + ":\n" + message.content + "\n\n"; 
            }
            index++;
        }
        GetComponent<Text>().text = resultMessage;
    }
}
