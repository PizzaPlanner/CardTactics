#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityEditor.Tilemaps
{
    [CustomGridBrush(true, false, true, "Tile Brush")]
    public class TileBrush : GridBrushBase
    {
        [AssetList]
        [SerializeField]
        Tile Tile;

        public override void Paint(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            PaintCell(gridLayout, position, brushTarget.transform);
        }

        public override void Erase(GridLayout gridLayout, GameObject brushTarget, Vector3Int position)
        {
            EraseCell(gridLayout, position, brushTarget.transform);
        }

        private void PaintCell(GridLayout grid, Vector3Int position, Transform parent)
        {
            var existingTile = GetCell<Tile>(grid, parent, position);
            if (existingTile != null) return;

            if (!PrefabUtility.IsPartOfPrefabAsset(Tile))
                return;

            var instance = (Tile)PrefabUtility.InstantiatePrefab(Tile, parent != null ? parent.root.gameObject.scene : SceneManager.GetActiveScene());
            instance.transform.SetParent(parent);

            Undo.RegisterCreatedObjectUndo(instance, "Paint Tile");
            var gpos = instance.GetComponent<GridPosition>();
            gpos.Position = position;
            gpos.SetPosition();
        }

        void EraseCell(GridLayout grid, Vector3Int position, Transform parent)
        {
            var existingTile = GetCell<Tile>(grid, parent, position);
            if (existingTile == null) return;

            Undo.DestroyObjectImmediate(existingTile.gameObject);
        }

        public static T GetCell<T>(GridLayout grid, Transform parent, Vector3Int position) where T : Component
        {
            int childCount;
            GameObject[] sceneChildren = null;
            if (parent == null)
            {
                var scene = SceneManager.GetActiveScene();
                sceneChildren = scene.GetRootGameObjects();
                childCount = scene.rootCount;
            }
            else
            {
                childCount = parent.childCount;
            }

            for (var i = 0; i < childCount; i++)
            {
                var child = sceneChildren == null ? parent.GetChild(i) : sceneChildren[i].transform;
                if (!child.TryGetComponent<GridPosition>(out var gpos)) continue;
                if (position == gpos.Position && child.TryGetComponent<T>(out var tile))
                    return tile;
            }
            return null;
        }
    }
}
#endif
