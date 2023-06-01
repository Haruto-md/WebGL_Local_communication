using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Speech2TextBuffer : MonoBehaviour
{
    private Text textComponent;
    public Button enterButten;
    public GameObject chatInterface;
    private void Start()
    {
        textComponent = GetComponent<Text>();
        textComponent.enabled = false;
        enterButten.onClick.AddListener(OnClick);
    }
    public void setText(string text)
    {
        textComponent.enabled = true;
        textComponent.text = text;
    }
    private void OnClick()
    {
        chatInterface.GetComponent<AIChatManager>().OnChatSubmitted(textComponent.text);
        textComponent.text = "";
        textComponent.enabled = false;
    }
}
