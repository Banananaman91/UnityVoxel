using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelTerrain;
using VoxelTerrain.Dependencies;

public class MouseInteraction : MonoBehaviour
{
    [SerializeField] private VoxelEngine engine;
    private Camera CamMain => Camera.main;

    private void Start()
    {
        var rounded = Math.Round(6.5f);
        Debug.Log(rounded);
    }

    // Update is called once per frame
    void Update()
    {
        if (!Input.GetMouseButton(0)) return;
        
        Debug.Log("Mouse Input");
        
        var ray = CamMain.ViewportPointToRay(CamMain.ScreenToViewportPoint(Input.mousePosition));
        
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;
        
        Debug.Log("Ray Hit");
        
        var hitPos = hit.point;
        
        Debug.Log("Hit Pos = " + hitPos);
        
        var posX = Mathf.FloorToInt(hitPos.x / engine._chunkSize) * engine._chunkSize;
        var posY = Mathf.FloorToInt(hitPos.y / engine._chunkHeight) * engine._chunkHeight;
        var posZ = Mathf.FloorToInt(hitPos.z / engine._chunkSize) * engine._chunkSize;
        
        Debug.Log("Chunk Pos = " + posX + ", " + posY + ", " + posZ);

        var hasChunk = engine.WorldData.Chunks.ContainsKey(ChunkId.FromWorldPos(posX, posY, posZ));

        if (!hasChunk) return;
        
        Debug.Log("Has Chunk");
        
        var chunk = engine.WorldData.Chunks[ChunkId.FromWorldPos(posX, posY, posZ)];
        var voxPosX = Math.Round(hitPos.x - 0.5f) - posX;
        var voxPosY = Math.Round(hitPos.y - 0.5f) - posY;
        var voxPosZ = Math.Round(hitPos.z - 0.5f) - posZ;
        
        Debug.Log("Voxel Pos = " + voxPosX + ", " + voxPosY + ", " + voxPosZ);

        chunk[(int) voxPosX, (int) voxPosY, (int) voxPosZ] = VoxelType.Default;
        chunk.UpdateMesh(posX, posY, posZ);
        
        Debug.Log("Mesh Updated");

        var hasMonoChunk = engine._chunkPoolDictionary.ContainsKey(ChunkId.FromWorldPos(posX, posY, posZ));

        if (!hasMonoChunk) return;
        
        Debug.Log("Has Mono Chunk");
        
        var monoChunk = engine._chunkPoolDictionary[ChunkId.FromWorldPos(posX, posY, posZ)];
        monoChunk.UpdateChunk(chunk);
    }
}
