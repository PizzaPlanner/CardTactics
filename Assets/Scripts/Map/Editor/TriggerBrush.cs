#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityEditor.Tilemaps
{
    [CustomGridBrush(true, false, false, "Trigger Brush")]
    public class TriggerBrush : GridBrushBase
    {
        [SerializeField]
        [AssetSelector]
        Trigger Trigger;

        public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            PaintCell(gridLayout, position, brushTarget.transform, Trigger);
        }

        public override void Erase(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            PaintCell(gridLayout, position, brushTarget.transform, null);
        }

        private void PaintCell(GridLayout grid, Vector3Int position, Transform parent, Trigger trigger)
        {
            var existingTile = TileBrush.GetCell<Tile>(grid, parent, position);
            if (existingTile == null) return;
            var hasArea = existingTile.TryGetComponent<TriggerComponent>(out var area);
            if (trigger == null && hasArea)
                Undo.DestroyObjectImmediate(area);
            else if (trigger != null)
            {
                if (!hasArea)
                    area = Undo.AddComponent<TriggerComponent>(existingTile.gameObject);
                Undo.RecordObject(area, "Paint Trigger");
                area.Trigger = trigger;

            }
        }
    }
}
#endif
