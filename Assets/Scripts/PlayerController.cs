using System.Collections.Generic;
using Mirror;

public class PlayerController : NetworkBehaviour
{
    public List<Unit> ControlledUnits;

    [SyncVar] public Deck Deck;

    [Command(requiresAuthority = true)]
    public void SelectDeck(Deck deck)
    {
        Deck = Instantiate(deck);
    }
}