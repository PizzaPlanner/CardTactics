using UnityEngine;

public class UnitCard : Card
{
    public override Sprite Icon => UnitReference.Icon;
    public override string Name => UnitReference.Name;
    public Unit UnitReference;

    protected override void OnInit()
    {
        ExtendWith(new SkillInfo { Skill = new PlaceSkill() }, 0);
    }

    protected override CardState StateOnPlayed => CardState.Active;
}
