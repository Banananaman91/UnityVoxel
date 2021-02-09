using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelTerrain;
using VoxelTerrain.Dependencies;

public class MouseInteraction : MonoBehaviour
{
    [SerializeField] private VoxelEngine engine;
    [SerializeField] private VoxelType _setVoxelType;
    private Camera CamMain => Camera.main;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0)) DestroyVoxel();
        else if (Input.GetMouseButton(1)) CreateVoxel();
    }

    private void DestroyVoxel()
    {
        var ray = CamMain.ViewportPointToRay(CamMain.ScreenToViewportPoint(Input.mousePosition));
        
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        var hitPos = hit.point;
        
        var rayDir = ray.direction;
        rayDir.x = (float) Math.Round(rayDir.x * 2) / 2;
        rayDir.y = (float) Math.Round(rayDir.y * 2) / 2;
        rayDir.z = (float) Math.Round(rayDir.z * 2) / 2;

        var offset = rayDir;

        var posX = Mathf.FloorToInt((hitPos.x + offset.x) / engine._chunkSize) * engine._chunkSize;
        var posY = Mathf.FloorToInt((hitPos.y + offset.y) / engine._chunkHeight) * engine._chunkHeight;
        var posZ = Mathf.FloorToInt((hitPos.z + offset.z) / engine._chunkSize) * engine._chunkSize;

        var hasChunk = engine.WorldData.Chunks.ContainsKey(ChunkId.FromWorldPos(posX, posY, posZ));

        if (!hasChunk) return;
        
        var chunk = engine.WorldData.Chunks[ChunkId.FromWorldPos(posX, posY, posZ)];
        var voxPosX = Mathf.Floor(hitPos.x + offset.x) - posX;
        var voxPosY = Mathf.Floor(hitPos.y + offset.y) - posY;
        var voxPosZ = Mathf.Floor(hitPos.z + offset.z) - posZ;

        chunk[(int) voxPosX, (int) voxPosY, (int) voxPosZ] = VoxelType.Default;
        chunk.UpdateMesh(posX, posY, posZ);

        var hasMonoChunk = engine._chunkPoolDictionary.ContainsKey(ChunkId.FromWorldPos(posX, posY, posZ));

        if (!hasMonoChunk) return;

        var monoChunk = engine._chunkPoolDictionary[ChunkId.FromWorldPos(posX, posY, posZ)];
        monoChunk.UpdateChunk(chunk);
    }
    
    private void CreateVoxel()
    {
        var ray = CamMain.ViewportPointToRay(CamMain.ScreenToViewportPoint(Input.mousePosition));
        
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        var hitPos = hit.point;
        
        var rayDir = ray.direction;
        rayDir.x = (float) Math.Round(rayDir.x * 2) / 2;
        rayDir.y = (float) Math.Round(rayDir.y * 2) / 2;
        rayDir.z = (float) Math.Round(rayDir.z * 2) / 2;

        var offset = rayDir;

        var x = Mathf.FloorToInt((hitPos.x - offset.x) / engine._chunkSize) * engine._chunkSize;
        var y = Mathf.FloorToInt((hitPos.y - offset.y) / engine._chunkHeight) * engine._chunkHeight;
        var z = Mathf.FloorToInt((hitPos.z - offset.z) / engine._chunkSize) * engine._chunkSize;

        var hasChunk = engine.WorldData.Chunks.ContainsKey(ChunkId.FromWorldPos(x, y, z));

        if (!hasChunk) return;
        
        var chunk = engine.WorldData.Chunks[ChunkId.FromWorldPos(x, y, z)];

        var voxPosX = Mathf.Floor(hitPos.x - offset.x) - x;
        var voxPosY = Mathf.Floor(hitPos.y - offset.y) - y;
        var voxPosZ = Mathf.Floor(hitPos.z - offset.z) - z;

        chunk[(int) voxPosX, (int) voxPosY, (int) voxPosZ] = _setVoxelType;
        chunk.UpdateMesh(x, y, z);

        var hasMonoChunk = engine._chunkPoolDictionary.ContainsKey(ChunkId.FromWorldPos(x, y, z));

        if (!hasMonoChunk) return;

        var monoChunk = engine._chunkPoolDictionary[ChunkId.FromWorldPos(x, y, z)];
        monoChunk.UpdateChunk(chunk);
    }
}
