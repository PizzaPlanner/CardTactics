[System.Serializable]
public abstract class Passive
{
    public Card Source;
    public abstract string Description { get; }
    public void OnCardRemoved(CardContext ctx) { OnRemoved(ctx); }

    protected virtual void OnRemoved(CardContext ctx) { }

    public virtual void OnTurnStart(CardContext ctx) { }
    public virtual void OnTurnEnd(CardContext ctx) { }
}