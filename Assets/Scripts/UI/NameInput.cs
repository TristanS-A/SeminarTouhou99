using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NameInput : MonoBehaviour
{
    [SerializeField] private TMP_InputField mNameField;
    [SerializeField] private GameObject mJoinOptions;
    [SerializeField] private GameObject mNameTextOBJ;
    [SerializeField] private GameObject mNameButtonOBJ;

    // Start is called before the first frame update
    void Start()
    {
        mNameField.onSubmit.AddListener(submit);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void submit(string name)
    {
        mJoinOptions.SetActive(true);
        PlayerInfo.PlayerName = name;
        mNameTextOBJ.SetActive(false);
        mNameButtonOBJ.SetActive(false);
    }
}
