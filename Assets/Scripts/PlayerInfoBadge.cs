using System.Collections.Generic;
using UnityEngine.UIElements;

public class PlayerInfoBadge : VisualElement
{
    public PlayerInfo Player;

    public new class UxmlFactory : UxmlFactory<PlayerInfoBadge, UxmlTraits> { }

    public new class UxmlTraits : VisualElement.UxmlTraits
    {
        public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
        {
            get { yield break; }
        }

        public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
        {
            base.Init(ve, bag, cc);
        }
    }
}