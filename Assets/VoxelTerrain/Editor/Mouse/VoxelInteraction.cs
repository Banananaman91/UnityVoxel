using System;
using UnityEngine;
using VoxelTerrain.Editor.Grid;
using VoxelTerrain.Editor.Voxel;

namespace VoxelTerrain.Editor.Mouse
{
    public class VoxelInteraction : MonoBehaviour
    {
        [SerializeField] private VoxelEngine engine;
        [SerializeField] private VoxelType _setVoxelType;
        [SerializeField] private float _xRadius = 5;
        [SerializeField] private float _zRadius = 5;
        [SerializeField] private float _circleRadius = 5;
        [SerializeField] private float _flattenHeight = 2;
        [SerializeField] private FlattenShape _shape = FlattenShape.Single;
        private Camera CamMain => Camera.main;
        private float offset => engine.ChunkInfo.VoxelSize / 2;
        private float size => engine.ChunkInfo.VoxelSize;

        // Update is called once per frame
        void Update()
        {
            switch (_shape)
            {
                case FlattenShape.Single:
                    if (Input.GetMouseButton(0)) DestroyVoxel();
                    else if (Input.GetMouseButton(1)) CreateVoxel();
                    break;
                case FlattenShape.Square:
                    if (Input.GetMouseButtonDown(0)) DestroyVoxel();
                    else if (Input.GetMouseButtonDown(1)) CreateVoxel();
                    break;
                case FlattenShape.Circular:
                    if (Input.GetMouseButtonDown(0)) DestroyVoxel();
                    else if (Input.GetMouseButtonDown(1)) CreateVoxel();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DestroyVoxel()
        {
            var ray = CamMain.ViewportPointToRay(CamMain.ScreenToViewportPoint(Input.mousePosition));
        
            if (!Physics.Raycast(ray, out var hit)) return;

            var hitPos = GridSnapper.SnapToGrid(hit.point, size, offset);

            hitPos.y -= size;
        
            UpdateChunks(hitPos, VoxelType.Default);
        }
    
        private void CreateVoxel()
        {
            var ray = CamMain.ViewportPointToRay(CamMain.ScreenToViewportPoint(Input.mousePosition));
        
            if (!Physics.Raycast(ray, out RaycastHit hit)) return;

            var hitPos = GridSnapper.SnapToGrid(hit.point, size, offset);
        
            UpdateChunks(hitPos, _setVoxelType);
        }

        private void UpdateChunks(Vector3 hitPos, VoxelType voxelType)
        {
            Vector3 chunkPos;
            Chunk chunk;
            Vector3 voxPos;
            Vector3 newPos;
            switch (_shape)
            {
                case FlattenShape.Single:
                    chunkPos = engine.NearestChunk(hitPos);
                    chunk = engine.WorldData.GetChunkAt(chunkPos);

                    if (chunk == null) return;

                    voxPos = hitPos - chunkPos;
                    chunk.SetVoxel(voxPos, voxelType);
                    chunk.SetMesh(chunkPos);
                    break;
                case FlattenShape.Square:
                    for (float i = hitPos.x - _xRadius; i < hitPos.x + _xRadius; i++)
                    {
                        for (float j = hitPos.z - _zRadius; j < hitPos.z + _zRadius; j++)
                        {
                            newPos = new Vector3(i, hitPos.y, j);
                            chunkPos = engine.NearestChunk(newPos);
                            chunk = engine.WorldData.GetChunkAt(chunkPos);

                            if (chunk == null) continue;

                            voxPos = newPos - chunkPos;

                            if (voxelType == VoxelType.Default) chunk.SetVoxel(voxPos, voxelType);
                            else Flatten(voxPos, voxelType, chunk);
                            chunk.SetMesh(chunkPos);
                        }
                    }

                    break;
                case FlattenShape.Circular:
                    for (float i = hitPos.x - _xRadius; i < hitPos.x + _xRadius; i++)
                    {
                        for (float j = hitPos.z - _zRadius; j < hitPos.z + _zRadius; j++)
                        {
                            newPos = new Vector3(i, hitPos.y, j);
                            chunkPos = engine.NearestChunk(newPos);
                            chunk = engine.WorldData.GetChunkAt(chunkPos);

                            if (chunk == null) continue;

                            if (!InRange(newPos, hitPos)) continue;
                        
                            voxPos = newPos - chunkPos;
                            if (voxelType == VoxelType.Default) chunk.SetVoxel(voxPos, voxelType);
                            else Flatten(voxPos, voxelType, chunk);
                            chunk.SetMesh(chunkPos);
                        }
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool InRange(Vector3 pos, Vector3 origin) => Vector3.Distance(origin, pos) < _circleRadius;

        private void Flatten(Vector3 pos, VoxelType voxelType, Chunk chunk)
        {
            Vector3 newPos = pos;

            do
            {
                chunk.SetVoxel(newPos, VoxelType.Default);
                newPos.y++;
            } while (Vector3.Distance(pos, newPos) <= _flattenHeight);

            newPos = pos;

            do
            {
                if (chunk[newPos.x, newPos.y, newPos.z] == (float)VoxelType.Default) chunk.SetVoxel(newPos, voxelType);
                newPos.y--;
            } while (Vector3.Distance(pos, newPos) <= _flattenHeight);
        }
    }
}
