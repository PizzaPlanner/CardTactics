using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class MainMenu : MonoBehaviour
{
    [SerializeField] Map Tutorial;
    [SerializeField] Map Campaign;
    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        root.Q<Button>("Tutorial").clicked += () => LoadMap(Tutorial);
        root.Q<Button>("Campaign").clicked += () => LoadMap(Campaign);
    }

    void LoadMap(Map map)
    {
        LoadingScreen.PlayMap(map);
    }
}