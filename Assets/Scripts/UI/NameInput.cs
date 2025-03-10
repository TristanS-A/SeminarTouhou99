using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class NameInput : MonoBehaviour
{
    [SerializeField] private GameObject mJoinOptions;

    // Start is called before the first frame update
    void Start()
    {
        TMP_InputField nameField = GetComponent<TMP_InputField>();
        nameField.onSubmit.AddListener(submit);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void submit(string name)
    {
        mJoinOptions.SetActive(true);
        PlayerInfo.PlayerName = name;
        Destroy(gameObject);
    }
}
