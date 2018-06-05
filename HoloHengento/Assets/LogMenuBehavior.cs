using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogMenuBehavior : MonoBehaviour {

    [SerializeField]
    private Text m_textUI = null;
    private Text appText = null;
    string preTxt = "";

    bool textFlg = true;

    private void Awake()
    {
        m_textUI = transform.Find("Text").GetComponent<Text>();
        m_textUI.text = "";
        appText = transform.Find("AppText").GetComponent<Text>();
        appText.text = "";
        Application.logMessageReceived += OnLogMessage;
        //transform.Find("Text").gameObject.SetActive(textFlg);
    }

    private void OnDestroy()
    {
        Application.logMessageReceived += OnLogMessage;
    }

    private void OnLogMessage(string i_logText, string i_stackTrace, LogType i_type)
    {
        if (!textFlg || string.IsNullOrEmpty(i_logText))
        {
            return;
        }
        //if (preTxt != i_logText)
        //{
            //m_textUI.text += i_logText + "\n";
            m_textUI.text += i_logText + "\n";
        //}
        preTxt = i_logText;
    }

    public void UpdateAppText(string logText)
    {
        appText.text = logText;
    }

}
