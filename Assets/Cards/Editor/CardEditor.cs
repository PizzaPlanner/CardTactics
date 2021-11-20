using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class CardEditor : EditorWindow
{
    VisualTreeAsset cardUIPrefab;
    const string UI_PREFAB_PATH = "Assets/Cards/UI/CardBig.uxml";

    VisualElement inspector;

    [MenuItem("Window/Cards")]
    static void Init()
    {
        CardEditor window = (CardEditor)EditorWindow.GetWindow(typeof(CardEditor));
        window.Show();
    }

    void OnEnable()
    {
        cardUIPrefab = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(UI_PREFAB_PATH);
        var list = new ScrollView(ScrollViewMode.Vertical);

        list.contentContainer.style.flexDirection = FlexDirection.Row;
        list.contentContainer.style.flexWrap = Wrap.Wrap;
        inspector = new ScrollView(ScrollViewMode.Vertical);
        rootVisualElement.Add(list);
        rootVisualElement.style.flexDirection = FlexDirection.Row;
        var newcard = new Button(() => { var card = Card.CreateInstance<ActionCard>(); MakeCard(card, list.contentContainer); ProjectWindowUtil.CreateAsset(card, "Assets/Cards/Action/newAction.asset"); }) { text = "New ActionCard" };
        list.contentContainer.Add(newcard);
        foreach (var cardAsset in AssetDatabase.FindAssets("t:Card").Select(x => AssetDatabase.LoadAssetAtPath<Card>(AssetDatabase.GUIDToAssetPath(x))))
            MakeCard(cardAsset, list.contentContainer);
        rootVisualElement.Add(inspector);
    }

    void MakeCard(Card card, VisualElement parent)
    {
        cardUIPrefab.CloneTree(parent, out var idx, out var _);
        var cardItem = parent[idx].Q<CardItem>();
        cardItem.RegisterCallback<MouseDownEvent>((_) => Edit(cardItem));
        try
        {

            cardItem.Card = card;
        }
        catch { }
        cardItem.transform.scale = new Vector3(0.9f, 0.9f, 0.9f);
    }

    CardItem edited;

    void Edit(CardItem item)
    {
        edited = item;
        var card = item.Card;
        inspector.contentContainer.Clear();
        if (card.Icon != null)
        {
            var img = new VisualElement();
            img.style.backgroundImage = new StyleBackground(card.Icon);
            img.style.height = 64;
            img.style.width = 64;
            inspector.contentContainer.Add(img);
        }
        inspector.contentContainer.Add(new Label(card.Name));
        inspector.contentContainer.Add(new InspectorElement(card));
        try
        {
            item.Card = card;
        }
        catch { }
    }
}