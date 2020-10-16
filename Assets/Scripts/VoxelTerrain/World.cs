using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World
{
    public Dictionary<ChunkId, Chunk> Chunks = new Dictionary<ChunkId, Chunk>();

    public BlockType this[int x, int y, int z]
    {
        get
        {
            var chunk = Chunks[ChunkId.FromWorldPos(x, y, z)];
            return chunk[x & 0xf, y & 0xf, z & 0xf];
        }

        set
        {
            var chunk = Chunks[ChunkId.FromWorldPos(x, y, z)];
            chunk[x & 0xf, y & 0xf, z & 0xf] = value;
        }
    }
}
