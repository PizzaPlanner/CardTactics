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
        list.contentContainer.style.flexDirection = FlexDirection.Column;
        rootVisualElement.Add(list);
        inspector = new ScrollView(ScrollViewMode.Vertical);
        rootVisualElement.Add(inspector);

        rootVisualElement.style.flexDirection = FlexDirection.Row;

        var cardAssetsBasePath = "Assets/Cards/Action";

        foreach (var path in AssetDatabase.GetSubFolders(cardAssetsBasePath))
        {
            var pathThingy = new Foldout { text = path };
            pathThingy.contentContainer.style.flexDirection = FlexDirection.Row;
            pathThingy.contentContainer.style.flexWrap = Wrap.Wrap;
            list.contentContainer.Add(pathThingy);
            // New Card Button
            cardUIPrefab.CloneTree(pathThingy.contentContainer, out var idx, out var _);
            var newcard = pathThingy.contentContainer[idx].Q<CardItem>();
            newcard.Card = null;
            newcard.RegisterCallback<MouseDownEvent>((_) => { var card = Card.CreateInstance<ActionCard>(); MakeCard(card, list.contentContainer); ProjectWindowUtil.CreateAsset(card, $"{path}/newAction.asset"); });
            newcard.Q<Label>("Name").text = "New ActionCard";
            newcard.Q<Label>("Description").text = $"in {path}";
            newcard.transform.scale = new Vector3(0.7f, 0.7f, 0.7f);
            newcard.style.marginBottom = -50;
            newcard.style.marginTop = -50;
            newcard.style.marginLeft = -30;
            newcard.style.marginRight = -30;

            // Add Cards
            foreach (var cardAsset in AssetDatabase.FindAssets("t:Card", new string[] { path }).Select(x => AssetDatabase.LoadAssetAtPath<Card>(AssetDatabase.GUIDToAssetPath(x))))
                MakeCard(cardAsset, pathThingy.contentContainer);
        }
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
        cardItem.transform.scale = new Vector3(0.7f, 0.7f, 0.7f);
        cardItem.style.marginBottom = -50;
        cardItem.style.marginTop = -50;
        cardItem.style.marginLeft = -30;
        cardItem.style.marginRight = -30;
    }

    CardItem edited;

    void Edit(CardItem item)
    {
        edited = item;
        var card = item.Card;
        inspector.contentContainer.Clear();
        inspector.contentContainer.Add(new InspectorElement(card));
        try
        {
            item.Card = card;
        }
        catch { }
    }
}