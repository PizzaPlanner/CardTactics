using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class AttackSkill : ISkill
{
    [ValidateInput("@((int)$value.Tile & (int)TileFilter.Empty) == 0")]
    public SelectionFilter Filter;
    public int MinRange;
    public int MaxRange;
    [SerializeField] AudioClip Sound;
    [SerializeField] bool confirmOnePossibleTile;
    public float Damage;

    public string Description => $"Attack {Damage} {(MaxRange > 1 ? $"(Range {MaxRange})" : "")}";
    public bool AutoSelectOnOnePossibleTile => !confirmOnePossibleTile;

    public IEnumerable<ITilePattern> CreatePatterns(SkillPlanContext ctx)
    {
        return new RangePatternBuilder
        {
            MinRange = MinRange,
            MaxRange = MaxRange
        }.Patterns(Map.Instance, ctx.SourceTile, Filter);
    }

    public IEnumerator ExecuteServer(SkillExecuteContext ctx)
    {
        ctx.OutTile.Unit.Damage(Damage, ctx.OriginTile.Unit);
        yield return null;
    }

    public IEnumerator ExecuteClient(SkillExecuteContext ctx)
    {
        Animator sourceAnimator = ctx.Unit.GetComponentInChildren<Animator>();
        Animator targetAnimator = ctx.OutTile.Unit.GetComponentInChildren<Animator>();
        ctx.SourceTile.Face(Tile.GetFacing(ctx.SourceTile, ctx.OutTile));
        if (GameSettings.Instance.UseAnimations)
        {
            if (sourceAnimator != null)
            {
                sourceAnimator.SetInteger("TriggerNumber", 4);
                sourceAnimator.SetInteger("Action", 5);
                sourceAnimator.SetTrigger("Trigger");
            }
            if (Sound != null)
            {
                ctx.SourceTile.Unit.GetComponent<AudioSource>().PlayOneShot(Sound);
            }
            yield return new WaitForSeconds(0.3f);
            if (targetAnimator != null)
            {
                targetAnimator.SetInteger("Action", 1);
                targetAnimator.SetTrigger("GetHitTrigger");
            }
        }
    }

    public void Serialize(Mirror.NetworkWriter writer)
    {
        writer.Write(Damage);
        writer.WriteObject(Sound);
    }
    public ISkill Deserialize(Mirror.NetworkReader reader)
    {
        Damage = reader.Read<float>();
        Sound = reader.ReadObject<AudioClip>();
        return this;
    }
}