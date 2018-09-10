using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadSceneFromMenu : MonoBehaviour {

    public int sceneToLoad;
    public Text loadScreenLabel;
    private bool loadScene;

    public void LoadScene()
    {

        if (!loadScene)
        {

            loadScene = true;

            loadScreenLabel.text = "Loading...";

            StartCoroutine(LoadNewScene());
        }

    }

    void Update()
    {

        if (loadScene)
        {
            loadScreenLabel.color = new Color(loadScreenLabel.color.r, loadScreenLabel.color.g, loadScreenLabel.color.b, Mathf.PingPong(Time.time, 1));
        }

    }

    // The coroutine runs on its own at the same time as Update() and takes an integer indicating which scene to load.
    IEnumerator LoadNewScene()
    {

        // This line waits for 3 seconds before executing the next line in the coroutine.
        // This line is only necessary for this demo. The scenes are so simple that they load too fast to read the "Loading..." text.
        yield return new WaitForSeconds(1);

        // Start an asynchronous operation to load the scene that was passed to the LoadNewScene coroutine.
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneToLoad);

        // While the asynchronous operation to load the new scene is not yet complete, continue waiting until it's done.
        while (!async.isDone)
        {
            yield return null;
        }

    }

}
