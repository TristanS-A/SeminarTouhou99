using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ClientHandler;

public class JoinRoomUIHandler : MonoBehaviour
{
    [SerializeField] private GameObject mResultsContent;
    [SerializeField] private GameObject m_IPDisplay;

    //Joinable IP storage for lobby searching scene
    private Dictionary<string, JoinIPData> mJoinableIPs = new Dictionary<string, JoinIPData>();

    private Tuple<string, string> mIPInfo;

    private void OnEnable()
    {
        Debug.Log("HERE");
        EventSystem.ipReceived += AddIP;
    }

    private void OnDisable()
    {
        EventSystem.ipReceived -= AddIP;
    }

    private void Start()
    {
        ClientHandler.instance.RunClientSetUp();

        if (ClientHandler.instance.mDebugMode)
        {
            AddIP(ClientHandler.instance.mDebugDebugIP, "Debug Room");
        }
    }

    private void AddIP(string ip, string connectionName)
    {
        if (!mJoinableIPs.ContainsKey(ip))
        {
            mIPInfo = new Tuple<string, string>(ip, connectionName);

            JoinIPData joinIPData = new() { name = connectionName, joinUIOBJ = null };
            mJoinableIPs.Add(ip, joinIPData);
        }
    }

    private void Update()
    {
        if (mIPInfo != null)
        {
            GameObject newIPDisplay = Instantiate(m_IPDisplay, mResultsContent.transform);

            TextMeshProUGUI joinName = newIPDisplay.GetComponentInChildren<TextMeshProUGUI>();
            Button joinB = newIPDisplay.GetComponentInChildren<Button>();
            TextMeshProUGUI joinBText = joinB.GetComponentInChildren<TextMeshProUGUI>();

            joinName.text = mIPInfo.Item2;

            EventTrigger trigger = newIPDisplay.GetComponentInChildren<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener((data) => { ClientHandler.instance.JoinHost((BaseEventData)data); });
            trigger.triggers.Add(entry);

            joinB.gameObject.AddComponent<IPStorageAttachment>().IP = mIPInfo.Item1;

            JoinIPData joinIPData = new() { name = mIPInfo.Item2, joinUIOBJ = newIPDisplay };
            mJoinableIPs[mIPInfo.Item1] = joinIPData;

            mIPInfo = null;
        }
    }
}
