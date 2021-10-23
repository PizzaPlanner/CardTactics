#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityEditor.Tilemaps
{
    [CustomGridBrush(true, false, false, "Area Brush")]
    public class AreaBrush : GridBrushBase
    {
        [SerializeField]
        int Area;

        public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            PaintCell(gridLayout, position, brushTarget.transform, Area);
        }

        public override void Erase(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            PaintCell(gridLayout, position, brushTarget.transform, -1);
        }

        private void PaintCell(GridLayout grid, Vector3Int position, Transform parent, int height)
        {
            var existingTile = TileBrush.GetCell<Tile>(grid, parent, position);
            if (existingTile == null) return;
            var hasArea = existingTile.TryGetComponent<Area>(out var area);
            if (height < 0 && hasArea)
                Undo.DestroyObjectImmediate(area);
            else if (height >= 0)
            {
                if (!hasArea)
                    area = Undo.AddComponent<Area>(existingTile.gameObject);
                Undo.RecordObject(area, "Paint Area");
                area.Id = height;
            }
        }
    }
}
#endif
