using System;
using UnityEngine;
using VoxelTerrain.Editor.Grid;

namespace VoxelTerrain.Editor.Mouse
{
    public class MousePositionDraw : MonoBehaviour
    {
        [SerializeField] private VoxelEngine _engine;
        private float offset => _engine ? _engine.ChunkInfo.VoxelSize / 2 : 0.25f;
        private float size => _engine ? _engine.ChunkInfo.VoxelSize : 0.5f;
        private Camera CamMain => Camera.main;

        private void Awake()
        {
            transform.localScale = new Vector3(size,size,size);
        }

        private void Update()
        {
            var ray = CamMain.ScreenPointToRay(Input.mousePosition);
            
            if (!Physics.Raycast(ray, out var hit)) return;

            transform.position = GridSnapper.SnapToGrid(hit.point, size, offset);
        }
    }
}
