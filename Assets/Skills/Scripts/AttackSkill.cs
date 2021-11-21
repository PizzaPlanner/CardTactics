using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class AttackSkill : Skill
{
    [ValidateInput("@((int)$value.Tile & (int)TileFilter.Empty) == 0")]
    public SelectionFilter Filter;
    public int MinRange;
    public int MaxRange;
    [SerializeField] AudioClip Sound;
    [SerializeField] bool confirmOnePossibleTile;
    public float Damage;

    Unit previewUnit;
    Facing previewFacing;

    public override string Description => $"Attack {Damage} {(MaxRange > 1 ? $"(Range {MaxRange})" : "")}";
    protected override bool AutoSelectOnOnePossibleTile => !confirmOnePossibleTile;

    protected override IEnumerable<ITilePattern> OnActivate(Map map, PlayerInfo player, Card card)
    {
        return new RangePatternBuilder { MinRange = MinRange, MaxRange = MaxRange }.Patterns(map, SourceTile, player, Filter);
    }

    protected override IEnumerator OnPreview(ITilePattern targetPattern)
    {
        OutTile = targetPattern.Tiles.First();
        Animator sourceAnimator = SourceTile.Unit.GetComponentInChildren<Animator>();
        Animator targetAnimator = OutTile.Unit.GetComponentInChildren<Animator>();
        previewFacing = SourceTile.Facing;
        SourceTile.Face(Tile.GetFacing(SourceTile, OutTile));
        if (sourceAnimator != null)
        {
            sourceAnimator.SetInteger("Action", 1);
            sourceAnimator.SetTrigger("AttackTrigger");
        }
        if (Sound != null)
            SourceTile.Unit.GetComponent<AudioSource>().PlayOneShot(Sound);
        yield return new WaitForSeconds(0.3f);
        previewUnit = OutTile.Unit;
        previewUnit.Damage(Damage, true);
        if (targetAnimator != null)
        {
            targetAnimator.SetInteger("Action", 1);
            targetAnimator.SetTrigger("GetHitTrigger");
        }
        if (previewUnit.LifePreview <= 0)
        {
            yield return new WaitForSeconds(0.5f);
            previewUnit.gameObject.SetActive(false);
            OutTile.Unit = null;
        }
    }

    protected override void OnUse()
    {
        previewUnit.Damage(Damage, false);
    }
}