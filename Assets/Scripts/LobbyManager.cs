using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public void OpenMapLobby(Map map) => Lobby.Setup(map);
    public void OpenHubLobby(Hubworld hub) => Lobby.Setup(hub);
}