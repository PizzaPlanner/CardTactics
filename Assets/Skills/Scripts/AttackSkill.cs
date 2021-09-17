using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Attack")]
public class AttackSkill : Skill
{
    public TargetType Filter;
    public int MinRange;
    public int MaxRange;
    public RangeType Type;
    [SerializeField] AudioClip Sound;
    public float Damage;

    Unit previewUnit;
    Facing previewFacing;

    protected override Tile[] OnActivate(GameSettings settings, GameSettings.PlayerInfo player, Card card)
    {
        if ((Filter & TargetType.Empty) > 0) return new Tile[0];
        return settings.Map.ShowRange(SourceTile, MinRange, MaxRange, Filter, Type, player);
    }

    protected override IEnumerator OnPreview()
    {
        Animator sourceAnimator = SourceTile.Unit.GetComponentInChildren<Animator>();
        Animator targetAnimator = TargetTile.Unit.GetComponentInChildren<Animator>();
        previewFacing = SourceTile.Facing;
        SourceTile.Face(Tile.GetFacing(SourceTile, TargetTile));
        sourceAnimator.SetInteger("Action", 1);
        sourceAnimator.SetTrigger("AttackTrigger");
        if (Sound != null)
            SourceTile.Unit.GetComponent<AudioSource>().PlayOneShot(Sound);
        yield return new WaitForSeconds(0.3f);
        previewUnit = TargetTile.Unit;
        previewUnit.Damage(Damage, true);
        targetAnimator.SetInteger("Action", 1);
        targetAnimator.SetTrigger("GetHitTrigger");
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

    protected override void OnCancel()
    {
        if (previewUnit.LifePreview <= 0)
        {
            previewUnit.gameObject.SetActive(true);
            TargetTile.Unit = previewUnit;
        }
        TargetTile.Unit.Damage(-Damage, true);
        SourceTile.Face(previewFacing);
    }
}