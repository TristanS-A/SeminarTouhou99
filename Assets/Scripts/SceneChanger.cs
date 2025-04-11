using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour {
    public void ChangeScene(int sceneIndex) {
        SceneManager.LoadScene(sceneIndex);
    }

    public void ChangeSceneByName(string name) {
        SceneManager.LoadScene(name);
    }
}
