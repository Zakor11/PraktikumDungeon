using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TimeSceneTransition : MonoBehaviour {

    private readonly float TRANSITION_TIME = 5.0f;
    private readonly string TRANSITION_TEXT = "Rückkehr zum Menü in: ";
    private float timePassed = 0;
    public int sceneToLoad;
    public Text loadScreenLabel;
    private bool loadScene;

    private void Update()
    {
        timePassed += Time.deltaTime;
        Debug.Log("Time passed = " + timePassed);
        if (timePassed > TRANSITION_TIME) {
            LoadScene();
        }

        //loadScreenLabel.color = new Color(loadScreenLabel.color.r, loadScreenLabel.color.g, loadScreenLabel.color.b, Mathf.PingPong(Time.time, 1));
        loadScreenLabel.text = TRANSITION_TEXT + ((int)(TRANSITION_TIME - timePassed)).ToString();
    
    }


    public void LoadScene()
    {

        if (!loadScene)
        {
            Debug.Log("Scene Loading!");
            loadScene = true;

            loadScreenLabel.text = "Loading...";

            StartCoroutine(LoadNewScene());
        }

    }

    IEnumerator LoadNewScene()
    {

        // This line waits for 3 seconds before executing the next line in the coroutine.
        // This line is only necessary for this demo. The scenes are so simple that they load too fast to read the "Loading..." text.
        yield return new WaitForSeconds(1);

        AsyncOperation async = SceneManager.LoadSceneAsync(sceneToLoad);
        // Start an asynchronous operation to load the scene that was passed to the LoadNewScene coroutine.

        // While the asynchronous operation to load the new scene is not yet complete, continue waiting until it's done.
        while (!async.isDone)
        {
            yield return null;
        }

    }
}
