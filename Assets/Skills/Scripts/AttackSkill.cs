using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

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

    protected override Tile[] OnActivate(Map map, PlayerInfo player, Card card)
    {
        return map.ShowRange(SourceTile, MinRange, MaxRange, Filter, player);
    }

    protected override IEnumerator OnPreview()
    {
        Animator sourceAnimator = SourceTile.Unit.GetComponentInChildren<Animator>();
        Animator targetAnimator = TargetTile.Unit.GetComponentInChildren<Animator>();
        previewFacing = SourceTile.Facing;
        SourceTile.Face(Tile.GetFacing(SourceTile, TargetTile));
        if (sourceAnimator != null)
        {
            sourceAnimator.SetInteger("Action", 1);
            sourceAnimator.SetTrigger("AttackTrigger");
        }
        if (Sound != null)
            SourceTile.Unit.GetComponent<AudioSource>().PlayOneShot(Sound);
        yield return new WaitForSeconds(0.3f);
        previewUnit = TargetTile.Unit;
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
            TargetTile.Unit = null;
        }
    }

    protected override void OnUse()
    {
        previewUnit.Damage(Damage, false);
    }
}