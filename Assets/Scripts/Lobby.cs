using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

public class Lobby
{

    private Lobby()
    {
        Players = new List<HumanPlayer>();
        achievements = new();
    }

    public static void Setup(Hubworld hub)
    {
        Current = new Lobby();
        Current.Join(true);
        Current.hubworld = hub;
        Current.Return();
    }

    public static void Setup(Map map)
    {
        Current = new Lobby();
        Current.Join(true);
        LoadingScreen.CreateGame(map, Current);
    }

    Hubworld hubworld;

    public void Return()
    {
        foreach (var player in Players)
            player.Player = null;

        if (hubworld != null)
            LoadingScreen.LoadHub(hubworld);
        else
            LoadingScreen.GoToMenu();

    }

    public static Lobby Current;

    public class HumanPlayer
    {
        public Deck Deck;
        public bool IsLocal;
        public List<string> Achievements;
        public PlayerInfo Player;
    }

    public HumanPlayer LocalPlayer => Players.First(x => x.IsLocal);
    public IEnumerable<HumanPlayer> AllPlayers => Players;

    List<HumanPlayer> Players;
    public int PlayerCount => Players.Count;
    public IEnumerable<string> Achievements => achievements;

    [ShowInInspector] List<string> achievements;

    public void Join(bool isLocal)
    {
        Players.Add(new HumanPlayer { IsLocal = isLocal, Achievements = new List<string>() });
    }

    public void Leave()
    {
        Current = null;
        LoadingScreen.GoToMenu();
    }

    public void Award(string achievement)
    {
        if (achievements == null)
            achievements = new List<string>();
        achievements.Add(achievement);
    }

    public void Award(string achievement, PlayerInfo player)
    {
        var p = Players.FirstOrDefault(x => x.Player == player);
        if (p == null)
            return;

        if (p.Achievements == null)
            p.Achievements = new List<string>();
        p.Achievements.Add(achievement);
    }

    internal bool AssignPlayer(PlayerInfo playerInfo)
    {
        var p = Players.FirstOrDefault(x => x.Player == null);
        if (p == null) return false;
        p.Player = playerInfo;
        return true;
    }

    internal Deck DeckOf(PlayerInfo playerInfo)
    {
        return Players.FirstOrDefault(x => x.Player == playerInfo)?.Deck;
    }
}