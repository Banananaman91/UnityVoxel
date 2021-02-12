using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VoxelTerrain;
using VoxelTerrain.Dependencies;
using VoxelTerrain.Editor.Grid;
using VoxelTerrain.Editor.Voxel.InfoData;

public class MouseInteraction : MonoBehaviour
{
    [SerializeField] private VoxelEngine engine;
    [SerializeField] private VoxelType _setVoxelType;
    private Camera CamMain => Camera.main;
    private float offset => engine ? engine.ChunkInfo.VoxelSize / 2 : 0.5f;
    private float size => engine ? engine.ChunkInfo.VoxelSize : 1f;
    private List<Task> _taskPool = new List<Task>();

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0)) DestroyVoxel();
        else if (Input.GetMouseButton(1)) CreateVoxel();
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
        var posX = Mathf.FloorToInt((hitPos.x) / engine.ChunkSize) * engine.ChunkSize;
        var posY = Mathf.FloorToInt((hitPos.y) / engine.ChunkHeight) * engine.ChunkHeight;
        var posZ = Mathf.FloorToInt((hitPos.z) / engine.ChunkSize) * engine.ChunkSize;

        var leftPos = Mathf.FloorToInt((hitPos.x - size) / engine.ChunkSize) * engine.ChunkSize;
        var rightPos = Mathf.FloorToInt((hitPos.x + size) / engine.ChunkSize) * engine.ChunkSize;
        var topPos = Mathf.FloorToInt((hitPos.y + size) / engine.ChunkHeight) * engine.ChunkHeight;
        var bottomPos = Mathf.FloorToInt((hitPos.y - size) / engine.ChunkHeight) * engine.ChunkHeight;
        var frontPos = Mathf.FloorToInt((hitPos.z + size) / engine.ChunkSize) * engine.ChunkSize;
        var backPos = Mathf.FloorToInt((hitPos.z + size) / engine.ChunkSize) * engine.ChunkSize;

        var chunk = engine.WorldData.Chunks[ChunkId.FromWorldPos(posX, posY, posZ)];
        var leftChunk = engine.WorldData.Chunks[ChunkId.FromWorldPos(leftPos, posY, posZ)];
        var rightChunk = engine.WorldData.Chunks[ChunkId.FromWorldPos(rightPos, posY, posZ)];
        var topChunk = engine.WorldData.Chunks[ChunkId.FromWorldPos(posX, topPos, posZ)];
        var bottomChunk = engine.WorldData.Chunks[ChunkId.FromWorldPos(posX, bottomPos, posZ)];
        var frontChunk = engine.WorldData.Chunks[ChunkId.FromWorldPos(posX, posY, frontPos)];
        var backChunk = engine.WorldData.Chunks[ChunkId.FromWorldPos(posX, posY, backPos)];
        
        var voxPosX = hitPos.x - offset - posX;
        var voxPosY = hitPos.y - offset - posY;
        var voxPosZ = hitPos.z - offset - posZ;

        chunk[(int) voxPosX, (int) voxPosY, (int) voxPosZ] = voxelType;
        chunk.UpdateMesh(posX, posY, posZ);
        
        var hasMonoChunk = engine._chunkPoolDictionary.ContainsKey(ChunkId.FromWorldPos(posX, posY, posZ));

        if (!hasMonoChunk) return;

        var monoChunk = engine._chunkPoolDictionary[ChunkId.FromWorldPos(posX, posY, posZ)];
        monoChunk.UpdateChunk(chunk);

        if (leftChunk != chunk)
        {
            leftChunk.UpdateMesh(leftPos, posY, posZ);
            var hasNearMonoChunk = engine._chunkPoolDictionary.ContainsKey(ChunkId.FromWorldPos(leftPos, posY, posZ));

            if (!hasNearMonoChunk) return;

            var nearMonoChunk = engine._chunkPoolDictionary[ChunkId.FromWorldPos(leftPos, posY, posZ)];
            nearMonoChunk.UpdateChunk(leftChunk);
        }

        if (rightChunk != chunk)
        {
            rightChunk.UpdateMesh(rightPos, posY, posZ);
            var hasNearMonoChunk = engine._chunkPoolDictionary.ContainsKey(ChunkId.FromWorldPos(rightPos, posY, posZ));

            if (!hasNearMonoChunk) return;

            var nearMonoChunk = engine._chunkPoolDictionary[ChunkId.FromWorldPos(rightPos, posY, posZ)];
            nearMonoChunk.UpdateChunk(rightChunk);
        }

        if (topChunk != chunk)
        {
            topChunk.UpdateMesh(posX, topPos, posZ);
            var hasNearMonoChunk = engine._chunkPoolDictionary.ContainsKey(ChunkId.FromWorldPos(posX, topPos, posZ));

            if (!hasNearMonoChunk) return;

            var nearMonoChunk = engine._chunkPoolDictionary[ChunkId.FromWorldPos(posX, topPos, posZ)];
            nearMonoChunk.UpdateChunk(topChunk);
        }

        if (bottomChunk != chunk)
        {
            bottomChunk.UpdateMesh(posX, bottomPos, posZ);
            var hasNearMonoChunk = engine._chunkPoolDictionary.ContainsKey(ChunkId.FromWorldPos(posX, bottomPos, posZ));

            if (!hasNearMonoChunk) return;

            var nearMonoChunk = engine._chunkPoolDictionary[ChunkId.FromWorldPos(posX, bottomPos, posZ)];
            nearMonoChunk.UpdateChunk(bottomChunk);
        }

        if (frontChunk != chunk)
        {
            frontChunk.UpdateMesh(posX, posY, frontPos);
            var hasNearMonoChunk = engine._chunkPoolDictionary.ContainsKey(ChunkId.FromWorldPos(posX, posY, frontPos));

            if (!hasNearMonoChunk) return;

            var nearMonoChunk = engine._chunkPoolDictionary[ChunkId.FromWorldPos(posX, posY, frontPos)];
            nearMonoChunk.UpdateChunk(frontChunk);
        }

        if (backChunk != chunk)
        {
            backChunk.UpdateMesh(posX, posY, backPos);
            var hasNearMonoChunk = engine._chunkPoolDictionary.ContainsKey(ChunkId.FromWorldPos(posX, posY, backPos));

            if (!hasNearMonoChunk) return;

            var nearMonoChunk = engine._chunkPoolDictionary[ChunkId.FromWorldPos(posX, posY, backPos)];
            nearMonoChunk.UpdateChunk(backChunk);
        }
    }
}
