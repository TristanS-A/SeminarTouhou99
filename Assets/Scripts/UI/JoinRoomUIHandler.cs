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
        if (ClientHandler.instance.mDebugMode)
        {
            AddIP(ClientHandler.instance.mDebugDebugIP, "Debug Room");
        }
    }

    private void AddIP(string ip, string connectionName)
    {
        if (!mJoinableIPs.ContainsKey(ip))
        {
            GameObject newIPDisplay = Instantiate(m_IPDisplay, mResultsContent.transform);

            Button joinB = newIPDisplay.GetComponentInChildren<Button>();
            TextMeshProUGUI joinBText = joinB.GetComponentInChildren<TextMeshProUGUI>();

            EventTrigger trigger = newIPDisplay.GetComponentInChildren<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener((data) => { ClientHandler.instance.JoinHost((BaseEventData)data); });
            trigger.triggers.Add(entry);

            joinB.gameObject.AddComponent<IPStorageAttachment>().IP = ip;

            JoinIPData joinIPData = new() { name = connectionName, joinUIOBJ = newIPDisplay };
            mJoinableIPs.Add(ip, joinIPData);
        }
    }
}
