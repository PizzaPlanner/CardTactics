#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityEditor.Tilemaps
{
    [CustomGridBrush(true, false, false, "Owner Brush")]
    public class OwnerBrush : GridBrushBase
    {
        [SerializeField]
        int Owner;

        public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            PaintCell(gridLayout, position, brushTarget.transform, Owner);
        }

        public override void Erase(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            PaintCell(gridLayout, position, brushTarget.transform, -1);
        }

        private void PaintCell(GridLayout grid, Vector3Int position, Transform parent, int height)
        {
            var existingTile = TileBrush.GetCell<Tile>(grid, parent, position);
            if (existingTile == null) return;
            var hasArea = existingTile.TryGetComponent<Owner>(out var area);
            if (height < 0 && hasArea)
                Undo.DestroyObjectImmediate(area);
            else if (height >= 0)
            {
                if (!hasArea)
                    area = Undo.AddComponent<Owner>(existingTile.gameObject);
                Undo.RecordObject(area, "Paint Owner");
                area.Id = height;
            }
        }
    }
}
#endif
