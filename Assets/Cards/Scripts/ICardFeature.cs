using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Card))]
public abstract class CardFeature : MonoBehaviour
{
    public virtual void OnInit() { }
    public virtual List<Card> GetRelatedCards() { return new List<Card>(); }
}