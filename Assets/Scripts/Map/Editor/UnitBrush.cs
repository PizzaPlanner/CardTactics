#if UNITY_EDITOR
using UnityEngine;
using Sirenix.OdinInspector;

namespace UnityEditor.Tilemaps
{
    [CustomGridBrush(true, false, false, "Unit Brush")]
    public class UnitBrush : GridBrushBase
    {
        [AssetList]
        [SerializeField]
        Unit Unit;

        [SerializeField]
        Facing Facing;

        public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            PaintCell(gridLayout, position, brushTarget.transform, Unit, Facing);
        }

        public override void Erase(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            RemoveCell(gridLayout, position, brushTarget.transform);
        }

        void RemoveCell(GridLayout grid, Vector3Int position, Transform parent)
        {
            var existingTile = TileBrush.GetCell<Tile>(grid, parent, position);
            if (existingTile == null || existingTile.Unit == null) return;

            Undo.DestroyObjectImmediate(existingTile.Unit.gameObject);
        }

        private void PaintCell(GridLayout grid, Vector3Int position, Transform parent, Unit unit, Facing facing)
        {
            var existingTile = TileBrush.GetCell<Tile>(grid, parent, position);
            if (existingTile == null || unit == null) return;
            if (existingTile.Unit != null)
            {
                Undo.DestroyObjectImmediate(existingTile.Unit);
            }
            var instance = Instantiate(unit);
            existingTile.Place(instance, facing);
            Undo.RegisterCreatedObjectUndo(instance, "Paint Unit");
        }
    }
}
#endif
