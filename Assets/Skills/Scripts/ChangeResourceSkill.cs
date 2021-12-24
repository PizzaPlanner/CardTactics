using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;

[System.Serializable]
public class ChangeResourceSkill : ISkill
{
    [HorizontalGroup("Split")]
    [LabelWidth(50)]
    public bool Convert;

    [HorizontalGroup("Split")]
    [HideLabel]
    public int Amount;

    [HorizontalGroup("Split")]
    [ShowIf("Convert")]
    [SuffixLabel("to")]
    [HideLabel]
    [PreviewField(24)]
    public CardClass FromClass;

    [HorizontalGroup("Split")]
    [HideLabel]
    [PreviewField(24)]
    public CardClass Class;


    public string Description => $"Gain {Amount} {Class}";

    public bool AutoSelectOnOnePossibleTile => true;

    public IEnumerable<ITilePattern> CreatePatterns(SkillPlanContext ctx)
    {
        if (ctx.SourceTile.Unit == null)
            return new ITilePattern[0];
        return new GlobalPatternBuilder().Patterns(Map.Instance, ctx.SourceTile, SelectionFilters.Self);
    }

    public IEnumerator ExecuteServer(SkillExecuteContext ctx)
    {
        if (Convert)
        {
            if (Amount < 0)
                ctx.Unit.Resources.Add(FromClass, -Amount);
            else
                ctx.Unit.Resources.Remove(FromClass, Amount);
        }
        if (Amount < 0)
            ctx.Unit.Resources.Remove(Class, -Amount);
        else
            ctx.Unit.Resources.Add(Class, Amount);
        yield return null;
    }

    public void Serialize(Mirror.NetworkWriter writer) { }
    public ISkill Deserialize(Mirror.NetworkReader reader) { return this; }
}