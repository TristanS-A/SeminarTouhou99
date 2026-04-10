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

    private Queue<Tuple<string, string>> mRoomsToAddToUI = new Queue<Tuple<string, string>>();


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
        Tuple<string, string> ipInfo = new Tuple<string, string>(ip, connectionName);
        mRoomsToAddToUI.Enqueue(ipInfo);
    }

    private void Update()
    {
        while (mRoomsToAddToUI.Count > 0)
        {
            Tuple<string, string> ipInfo = mRoomsToAddToUI.Dequeue();

            if (!mJoinableIPs.ContainsKey(ipInfo.Item1))
            {

                JoinIPData joinIPData = new() { name = ipInfo.Item2, joinUIOBJ = null };
                mJoinableIPs.Add(ipInfo.Item1, joinIPData);

                GameObject newIPDisplay = Instantiate(m_IPDisplay, mResultsContent_T);

                joinIPData = new() { name = ipInfo.Item2, joinUIOBJ = newIPDisplay };
                mJoinableIPs[ipInfo.Item1] = joinIPData;

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
            }
        }
    }
}
