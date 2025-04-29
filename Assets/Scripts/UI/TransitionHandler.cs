using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionHandler : MonoBehaviour
{
    public int sceneToTransitionTo = 0;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(transform.parent.parent);
    }

    private void Transition()
    {
        SceneManager.LoadScene(sceneToTransitionTo);
    }

    private void Destroy()
    {
        Destroy(transform.parent.parent.gameObject);
    }
}
