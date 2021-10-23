#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityEditor.Tilemaps
{
    [CustomGridBrush(true, false, false, "Marker Brush")]
    public class MarkerBrush : GridBrushBase
    {
        [SerializeField]
        int Id;

        public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            PaintCell(gridLayout, position, brushTarget.transform, Id);
        }

        public override void Erase(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            PaintCell(gridLayout, position, brushTarget.transform, -1);
        }

        private void PaintCell(GridLayout grid, Vector3Int position, Transform parent, int height)
        {
            var existingTile = TileBrush.GetCell<Tile>(grid, parent, position);
            if (existingTile == null) return;
            var hasArea = existingTile.TryGetComponent<Marker>(out var area);
            if (height < 0 && hasArea)
                Undo.DestroyObjectImmediate(area);
            else if (height >= 0)
            {
                if (!hasArea)
                    area = Undo.AddComponent<Marker>(existingTile.gameObject);
                Undo.RecordObject(area, "Paint Marker");
                area.Id = height;
            }
        }
    }
}
#endif
