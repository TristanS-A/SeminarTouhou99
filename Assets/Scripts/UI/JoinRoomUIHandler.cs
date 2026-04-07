using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ClientHandler;

public class JoinRoomUIHandler : MonoBehaviour
{
    [SerializeField] private Transform mResultsContent_T;
    [SerializeField] private GameObject m_IPDisplay;

    private List<Tuple<string, string>> mRoomsToAddToUI = new();


    //Joinable IP storage for lobby searching scene
    private Dictionary<string, JoinIPData> mJoinableIPs;

    private bool mIPsToProcess = false;
    private bool mProcessingIPs = false;
    private bool mCanAddIPs = true;

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

        mJoinableIPs = new Dictionary<string, JoinIPData>();
    }

    private void AddIP(string ip, string connectionName)
    {
        mCanAddIPs = false;
        if (!mJoinableIPs.ContainsKey(ip) && !mProcessingIPs)
        {
            Tuple<string, string>  ipInfo = new Tuple<string, string>(ip, connectionName);

            mRoomsToAddToUI.Add(ipInfo); 

            mIPsToProcess = true;
        }

        mCanAddIPs = true;
    }

    private void Update()
    {
        if (mIPsToProcess && mCanAddIPs)
        {
            mProcessingIPs = true;
            AddToIPList();
            mIPsToProcess = false;
            mProcessingIPs = false;
        }
    }

    //This function is seperate from the AddIP because you cannot Instantiate a gameobject from an event call 
    private void AddToIPList()
    {
        foreach (Tuple<string, string> ipInfo in mRoomsToAddToUI)
        {
            GameObject newIPDisplay = Instantiate(m_IPDisplay, mResultsContent_T);

            TextMeshProUGUI joinName = newIPDisplay.GetComponentInChildren<TextMeshProUGUI>();
            Button joinB = newIPDisplay.GetComponentInChildren<Button>();
            TextMeshProUGUI joinBText = joinB.GetComponentInChildren<TextMeshProUGUI>();

            //Sets the name of the room in UI
            joinName.text = ipInfo.Item2;

            //Adds Join Host listener to join button
            EventTrigger trigger = newIPDisplay.GetComponentInChildren<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerDown;
            entry.callback.AddListener((data) => { ClientHandler.instance.JoinHost((BaseEventData)data); });
            trigger.triggers.Add(entry);

            //Adds IP data to join button
            joinB.gameObject.AddComponent<IPStorageAttachment>().IP = ipInfo.Item1;

            JoinIPData joinIPData = new() { name = ipInfo.Item2, joinUIOBJ = newIPDisplay };
            mJoinableIPs.Add(ipInfo.Item1, joinIPData);
        }

        mRoomsToAddToUI.Clear();
    }
}
