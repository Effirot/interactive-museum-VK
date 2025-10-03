using UnityEngine;
using UnityEngine.SceneManagement;

public class Play : MonoBehaviour
{
    public void OpenScene(string sceneName)
    {
        SceneManager.LoadScene("SampleScene");
    }

}
