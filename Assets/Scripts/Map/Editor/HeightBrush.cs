#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityEditor.Tilemaps
{
    [CustomGridBrush(true, false, false, "Height Brush")]
    public class HeightBrush : GridBrushBase
    {
        [SerializeField]
        int Height;

        public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            PaintCell(gridLayout, position, brushTarget.transform, Height);
        }

        public override void Erase(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            PaintCell(gridLayout, position, brushTarget.transform, 0);
        }

        private void PaintCell(GridLayout grid, Vector3Int position, Transform parent, int height)
        {
            var existingTile = TileBrush.GetCell<Tile>(grid, parent, position);
            if (existingTile == null) return;
            Undo.RecordObject(existingTile.gameObject, "Paint Height");
            var gpos = existingTile.GetComponent<GridPosition>();
            gpos.Height = Height;
            gpos.SetPosition();
        }
    }
}
#endif
