#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityEditor.Tilemaps
{
    [CustomGridBrush(true, false, false, "Spawn Brush")]
    public class SpawnBrush : GridBrushBase
    {
        [SerializeField]
        Facing Facing;

        public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            PaintCell(gridLayout, position, brushTarget.transform, Facing);
        }

        public override void Erase(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            PaintCell(gridLayout, position, brushTarget.transform, Facing.Preserve);
        }

        private void PaintCell(GridLayout grid, Vector3Int position, Transform parent, Facing facing)
        {
            var existingTile = TileBrush.GetCell<Tile>(grid, parent, position);
            if (existingTile == null) return;
            var hasArea = existingTile.TryGetComponent<SpawnLocation>(out var area);
            if (facing == Facing.Preserve && hasArea)
                Undo.DestroyObjectImmediate(area);
            else if (facing != Facing.Preserve)
            {
                if (!hasArea)
                    area = Undo.AddComponent<SpawnLocation>(existingTile.gameObject);
                Undo.RecordObject(area, "Paint Spawn");
                area.Facing = facing;
            }
        }
    }
}
#endif
