using System;
using System.Collections;
using Mirror;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreen : NetworkBehaviour
{
    public const string SCENE_BOARD = "Board";
    public const string SCENE_MENU = "Menu";
    public const string SCENE_EMPTY = "Empty";
    public const string SCENE_LOADING = "LoadingScreen";

    class FakeBehaviour : MonoBehaviour { }

    public float Progress
    {
        get => progress; set
        {
            progress = value;
            GetComponent<RectTransform>().anchorMax = new Vector2(value, 1);
        }
    }
    float progress = 0;

    static IEnumerator LoadMap(GameObject loader, string scene, Action onLoaded, Action onFinished)
    {
        DontDestroyOnLoad(loader);
        yield return null;
        SceneManager.LoadScene(SCENE_LOADING, LoadSceneMode.Single);
        while (FindObjectOfType<LoadingScreen>() == null)
        {
            yield return new WaitForEndOfFrame();
        }
        var lbar = FindObjectOfType<LoadingScreen>();
        var op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        while (!op.isDone)
        {
            lbar.Progress = op.progress;
            yield return new WaitForEndOfFrame();
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(scene));
        onLoaded?.Invoke();

        SceneManager.UnloadSceneAsync(SCENE_LOADING).completed += (_) =>
        {
            onFinished?.Invoke();
            Destroy(loader);
        };
    }

    [Server]
    public static void ChangeSceneServer(string targetSceneName, Action onLoaded = null, Action onFinished = null)
    {
        NetworkManager.singleton.ServerChangeScene(targetSceneName);
        // RoomNetworkManager.OnSceneChange = () => { onLoaded?.Invoke(); onFinished?.Invoke(); };
    }
}
