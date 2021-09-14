using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingScreen : MonoBehaviour
{
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

    public static void CreateGame(GameSettings settings)
    {
        var loader = new GameObject();
        loader.AddComponent<FakeBehaviour>().StartCoroutine(LoadMap(loader, settings));
    }

    static IEnumerator LoadMap(GameObject loader, GameSettings settings)
    {
        DontDestroyOnLoad(loader);
        yield return null;
        SceneManager.LoadScene("LoadingScreen", LoadSceneMode.Single);
        while (FindObjectOfType<LoadingScreen>() == null)
        {
            yield return new WaitForEndOfFrame();
        }
        var lbar = FindObjectOfType<LoadingScreen>();
        var op = SceneManager.LoadSceneAsync("Board", LoadSceneMode.Additive);
        while (!op.isDone)
        {
            lbar.Progress = op.progress;
            yield return new WaitForEndOfFrame();
        }
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("Board"));
        GameObject.FindObjectOfType<Board>().Initialize(settings);

        SceneManager.UnloadSceneAsync("LoadingScreen").completed += (_) =>
        {
            GameObject.FindObjectOfType<TurnPlate>().StartTurn();
            Destroy(loader);
        };
    }

    public static void GoToMenu()
    {
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }
}
