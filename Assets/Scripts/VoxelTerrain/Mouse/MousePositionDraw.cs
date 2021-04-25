using UnityEngine;
using VoxelTerrain.Grid;
using VoxelTerrain.Voxel;
using VoxelTerrain.Voxel.InfoData;

namespace VoxelTerrain.Interactions
{
    public class MousePositionDraw : MonoBehaviour
    {
        [SerializeField] private VoxelEngine _engine;
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
