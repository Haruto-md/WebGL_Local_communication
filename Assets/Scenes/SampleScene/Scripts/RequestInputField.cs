using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RequestInputField : MonoBehaviour
{
    public GameObject requester;

    private InputField inputField;
    // Start is called before the first frame update
    void Start()
    {
        inputField = GetComponent<InputField>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnSubmitted()
    {
        requester.GetComponent<LocalPythonCommunicater>().SendRequest(inputField.text);
    }
}
