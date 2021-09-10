using UnityEngine;
using UnityEngine.UI;

public class CardReference : MonoBehaviour
{
    [SerializeReference] public Card Reference;

    public Image Icon;
    public Text Text;
}