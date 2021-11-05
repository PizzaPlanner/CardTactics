using System.Collections.Generic;
using UnityEngine;

public class Lobby : MonoBehaviour
{
    public List<HumanPlayer> Players;

    [SerializeField] Hubworld HubworldPrefab;

    public void PlayMap(Map prefab)
    {
        LoadingScreen.CreateGame(prefab, this);
    }

    public void GoToHub(Dictionary<HumanPlayer, bool> outcomes)
    {
        if (HubworldPrefab == null)
            LoadingScreen.GoToMenu();
        else
            LoadingScreen.LoadHub(HubworldPrefab, this);
    }
}