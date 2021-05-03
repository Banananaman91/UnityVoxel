using System.Collections;
using UnityEngine;
using VoxelTerrain.Interactions;
using VoxelTerrain.Voxel;
using VoxelTerrain.Voxel.Dependencies;

namespace VoxelTerrain
{
    public class PlayerPlacement : MonoBehaviour
    {
        [SerializeField] private VoxelEngine _engine;

        private void Awake()
        {
            StartCoroutine(SpawnHeadquarters());
        }

        private IEnumerator SpawnHeadquarters()
        {
            var chunkPoint = transform.position;

            chunkPoint = _engine.NearestChunk(chunkPoint);

            Chunk chunk;
            var chunkId = new ChunkId(chunkPoint.x, chunkPoint.y, chunkPoint.z);

            //Wait until the chunk is generated
            //Do not force load
            do
            {
                chunk = _engine.ChunkAt(chunkId, false);
                yield return null;
            } while (chunk == null);
            
            //Set both x and z positions to the centre of the chunk
            var xz = Chunk.ChunkSize / 2;
            
            int yCounter = 0;

            //get the current voxel at the centre, lowest point
            byte voxel = chunk[xz, yCounter, xz];

            //continuously move up until we find an empty space to set the object position
            while (voxel != 0)
            {
                yCounter++;
                voxel = chunk[xz, yCounter, xz];
                yield return null;
            }

            //set position
            transform.position = new Vector3(xz,chunkPoint.y + yCounter, xz);
        }
    }
}
