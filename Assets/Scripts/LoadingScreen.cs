using System;
using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
    const string SCENE_BOARD = "Board";
    const string SCENE_MENU = "Menu";
    const string SCENE_EMPTY = "Empty";
    const string SCENE_LOADING = "LoadingScreen";

    class FakeBehaviour : MonoBehaviour { }

    [ShowInInspector]
    [PropertyRange(0f, 1f)]
    public float Progress
    {
        get => progress; set
        {
            progress = value;
            GetComponent<RectTransform>().anchorMax = new Vector2(value, 1);
        }
    }
    float progress = 0;

    public static bool CreateGame(Map map)
    {

        ChangeScene(
        SCENE_BOARD,
        () =>
        {
            var mapInstance = Instantiate(map);
            GameObject.FindObjectOfType<Board>().Initialize(mapInstance);
        },
        () => GameObject.FindObjectOfType<Board>().StartGame());
        return true;
    }

    public static void LoadHub(Hubworld hubPrefab)
    {
        ChangeScene(
        SCENE_EMPTY,
        () =>
        {
            var hub = Instantiate(hubPrefab);
        }, null);
    }

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

    static void ChangeScene(string targetSceneName, Action onLoaded = null, Action onFinished = null)
    {
        var loader = new GameObject();
        loader.AddComponent<FakeBehaviour>().StartCoroutine(LoadMap(loader, targetSceneName, onLoaded, onFinished));
    }

    public static void GoToMenu()
    {
        ChangeScene(SCENE_MENU);
    }
}
