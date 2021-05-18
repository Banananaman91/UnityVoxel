using UnityEngine;
using VoxelTerrain.Engine;
using VoxelTerrain.Engine.InfoData;
using VoxelTerrain.Grid;

namespace VoxelTerrain.Interactions
{
    public class MousePositionDraw : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private VoxelEngine _engine;
#pragma warning restore 0649
        private float offset => _engine ? _engine.ChunkInfo.VoxelSize / 2 : GetComponent<ChunkInfo>().VoxelSize / 2;
        private float size => _engine ? _engine.ChunkInfo.VoxelSize : GetComponent<ChunkInfo>().VoxelSize;
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
