using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class HumanPlayer
{
    public Deck Deck;
    public bool IsLocal;
}

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

    public static bool CreateGame(Map map, Lobby lobby)
    {
        if (lobby.Players.Count < map.MinPartySize || lobby.Players.Count > map.MaxPartySize)
        {
            Debug.LogError("Can not start this map with Party Size " + lobby.Players.Count);
            return false;
        }
        var loader = new GameObject();
        loader.AddComponent<FakeBehaviour>().StartCoroutine(LoadMap(loader, map, lobby));
        return true;
    }

    public static void LoadHub(Hubworld hub, Lobby lobby)
    {
    }

    static IEnumerator LoadMap(GameObject loader, Map map, Lobby lobby)
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
        var mapInstance = Instantiate(map);
        GameObject.FindObjectOfType<Board>().Initialize(mapInstance, lobby);

        SceneManager.UnloadSceneAsync("LoadingScreen").completed += (_) =>
        {
            GameObject.FindObjectOfType<Board>().StartGame();
            Destroy(loader);
        };
    }

    public static void GoToMenu()
    {
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }
}
