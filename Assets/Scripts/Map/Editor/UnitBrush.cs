#if UNITY_EDITOR
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityEditor.Tilemaps
{
    [CustomGridBrush(true, false, false, "Unit Brush")]
    public class UnitBrush : GridBrushBase
    {
        [SerializeField]
        Facing Facing;

        [AssetList]
        [SerializeField]
        Unit Prefab;

        public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            PaintCell(gridLayout, position, brushTarget.transform, Prefab, Facing);
        }

        public override void Erase(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            PaintCell(gridLayout, position, brushTarget.transform, null, Facing.Preserve);
        }

        private void PaintCell(GridLayout grid, Vector3Int position, Transform parent, Unit unit, Facing facing)
        {
            var existingTile = TileBrush.GetCell<Tile>(grid, parent, position);
            if (existingTile == null) return;
            var hasArea = existingTile.TryGetComponent<UnitSpawn>(out var area);
            if (unit == null && hasArea)
                Undo.DestroyObjectImmediate(area);
            else if (unit != null)
            {
                if (!hasArea)
                    area = Undo.AddComponent<UnitSpawn>(existingTile.gameObject);
                Undo.RecordObject(area, "Paint Unit");
                area.Facing = facing;
                area.Prefab = unit;
            }
        }
    }
}
#endif
