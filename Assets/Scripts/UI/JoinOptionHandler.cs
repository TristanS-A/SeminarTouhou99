using UnityEngine;

public class JoinOptionHandler : MonoBehaviour
{
    [SerializeField] private GameObject mHostOrJoinOptions;
    [SerializeField] private GameObject mJoinOptions;

    public void GoToJoinOptions()
    {
        mJoinOptions.SetActive(true);
        mHostOrJoinOptions.SetActive(false);
    }
}
