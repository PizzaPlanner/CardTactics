[System.Serializable]
public abstract class Passive
{
    public Card Source { get; private set; }
    public abstract string Description { get; }
    public void OnCardCreated(Card card)
    {
        Source = card;
    }
    public void OnCardRemoved(Unit unit) { OnRemoved(unit); }

    protected virtual void OnRemoved(Unit unit) { }

    public virtual void OnTurnStart(Unit unit) { }
    public virtual void OnTurnEnd(Unit unit) { }
}