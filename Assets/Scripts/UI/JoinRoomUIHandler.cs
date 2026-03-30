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
        EventSystem.ipReceived += AddIP;
    }

    private void OnDisable()
    {
        EventSystem.ipReceived -= AddIP;
    }

    private void Start()
    {
        if (ClientHandler.instance != null)
        {
            ClientHandler.instance.RunClientSetUp();
        }
    }

    private void AddIP(string ip, string connectionName)
    {
        if (!mJoinableIPs.ContainsKey(ip))
        {
            mIPInfo = new Tuple<string, string>(ip, connectionName);

            JoinIPData joinIPData = new() { name = connectionName, joinUIOBJ = null };
            mJoinableIPs.Add(ip, joinIPData);

            GameObject newIPDisplay = Instantiate(m_IPDisplay, mResultsContent.transform);

            TextMeshProUGUI joinName = newIPDisplay.GetComponentInChildren<TextMeshProUGUI>();
            Button joinB = newIPDisplay.GetComponentInChildren<Button>();
            TextMeshProUGUI joinBText = joinB.GetComponentInChildren<TextMeshProUGUI>();

            //Sets the name of the room in UI
            joinName.text = mIPInfo.Item2;

            //Adds Join Host listener to join button
            EventTrigger trigger = newIPDisplay.GetComponentInChildren<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener((data) => { ClientHandler.instance.JoinHost((BaseEventData)data); });
            trigger.triggers.Add(entry);

            //Adds IP data to join button
            joinB.gameObject.AddComponent<IPStorageAttachment>().IP = mIPInfo.Item1;
        }
    }
}
