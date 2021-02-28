using UnityEngine;

namespace VoxelTerrain.Editor.Voxel
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class MonoChunk : MonoBehaviour
    {
        [HideInInspector] public VoxelEngine _engine;
        public MeshCollider MeshCollider => GetComponent<MeshCollider>();
        public MeshFilter MeshFilter => GetComponent<MeshFilter>();
        private Vector3 Position => new Vector3(transform.position.x, 0, transform.position.z);
        // Start is called before the first frame update
        void Awake()
        {
            _engine = FindObjectOfType<VoxelEngine>();
        }

        private void Update()
        {
            if (_engine.WithinRange(Position)) return;
        
            _engine.RemoveChunkAt(Position);
        }
    }
}
