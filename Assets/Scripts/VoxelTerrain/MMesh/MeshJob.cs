using System.Collections.Generic;
using TerrainData;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using VoxelTerrain.Voxel;
using VoxelTerrain.Voxel.Dependencies;

namespace VoxelTerrain.MMesh
{
    public struct MeshJob : IJob
    {
        internal NativeList<Vector3> vertices;
        internal NativeList<int> triangles;
        internal NativeList<Vector4> voxelUv;
        internal NativeList<Vector4> baryUv;
        public NativeArray<byte> currentVoxels;
        public NativeArray<byte> rightVoxels;
        public NativeArray<byte> forwardVoxels;
        public NativeArray<byte> rightForwardVoxels;
        public float scale;
        public Vector3 origin;
        public bool interpolate;
        public float noiseScale;
        public int seed;
        public float groundLevel;
        
        static readonly Vector4[] barycentricCoords = new Vector4[3] {new Vector4(1, 0, 0, 1), new Vector4(0, 1, 0, 1), new Vector4(0, 0, 1, 1)};

        public void Execute()
        {
            MarchingCubes();
        }
        
        private void MarchingCubes()
        {
            int flagIndex = 0;
            int index = 0;

            // var Vertices = new List<Vector3>();
            // var Triangles = new List<int>();
            // var voxelUv = new List<Vector4>();
            // var baryUv = new List<Vector4>();

            float[] afCubes = new float[8];

            for (int x = 0; x < Chunk.ChunkSize; x++)
            {
                for (int y = 0; y < Chunk.ChunkHeight - 1; y++)
                {
                    for (int z = 0; z < Chunk.ChunkSize; z++)
                    {
                        if (x == Chunk.ChunkSize - 1 || z == Chunk.ChunkSize - 1)
                        {
                            //Offsets are same as cornerOffsets[8]
                            afCubes[0] = GetVoxelAt(x, y, z, origin, scale);
                            afCubes[1] = GetVoxelAt(x + 1, y, z, origin, scale);
                            afCubes[2] = GetVoxelAt(x + 1, y + 1, z, origin, scale);
                            afCubes[3] = GetVoxelAt(x, y + 1, z, origin, scale);
                            afCubes[4] = GetVoxelAt(x, y, z + 1, origin, scale);
                            afCubes[5] = GetVoxelAt(x + 1, y, z + 1, origin, scale);
                            afCubes[6] = GetVoxelAt(x + 1, y + 1, z + 1, origin, scale);
                            afCubes[7] = GetVoxelAt(x, y + 1, z + 1, origin, scale);
                        }
                        else
                        {
                            //Offsets are same as cornerOffsets[8]
                            afCubes[0] = currentVoxels[Chunk.PosToIndex(x, y, z)];
                            afCubes[1] = currentVoxels[Chunk.PosToIndex(x + 1, y, z)];
                            afCubes[2] = currentVoxels[Chunk.PosToIndex(x + 1, y + 1, z)];
                            afCubes[3] = currentVoxels[Chunk.PosToIndex(x, y + 1, z)];
                            afCubes[4] = currentVoxels[Chunk.PosToIndex(x, y, z + 1)];
                            afCubes[5] = currentVoxels[Chunk.PosToIndex(x + 1, y, z + 1)];
                            afCubes[6] = currentVoxels[Chunk.PosToIndex(x + 1, y + 1, z + 1)];
                            afCubes[7] = currentVoxels[Chunk.PosToIndex(x, y + 1, z + 1)];
                        }

                        //Calculate the index of the current cube configuration as follows:
                        //Loop over each of the 8 corners of the cube, and set the corresponding
                        //bit to 1 if its value is below the surface level.
                        //this will result in a value between 0 and 255

                        flagIndex = 0;
                        for (int vtest = 0; vtest < 8; vtest++)
                        {
                            if (afCubes[vtest] <= 0.0f)
                                flagIndex |= 1 << vtest;
                        }

                        //Skip to next if all corners are the same
                        if (flagIndex == 0x00 || flagIndex == 0xFF)
                            continue;

                        //Get the offset of this current block
                        var offset = new Vector3(x * scale, y * scale, z * scale);

                        for (int triangle = 0; triangle < 5; triangle++)
                        {
                            int edgeIndex = VoxelLookUp.a2iTriangleConnectionTable[flagIndex][3 * triangle];

                            if (edgeIndex < 0)
                                continue; //Skip if the edgeIndex is -1

                            Vector4 voxelTypes = new Vector4(0, 0, 0, 1);
                            for (int triangleCorner = 0; triangleCorner < 3; triangleCorner++)
                            {
                                edgeIndex =
                                    VoxelLookUp.a2iTriangleConnectionTable[flagIndex][3 * triangle + triangleCorner];

                                var edge1 = VoxelLookUp.edgeVertexOffsets[edgeIndex, 0];
                                var edge2 = VoxelLookUp.edgeVertexOffsets[edgeIndex, 1];

                                edge1 *= scale;
                                edge2 *= scale;

                                Vector3 middle;
                                if (interpolate)
                                {
                                    float ofst;
                                    float s1;
                                    float delta;
                                    if (x == Chunk.ChunkSize - 1 || z == Chunk.ChunkSize - 1)
                                    {
                                        s1 = GetVoxelAt(x + (int) edge1.x, y + (int) edge1.y,
                                            z + (int) edge1.z, origin, scale);
                                        delta = s1 - GetVoxelAt(x + (int) edge2.x,
                                            y + (int) edge2.y, z + (int) edge2.z, origin, scale);
                                    }
                                    else
                                    {
                                        s1 = currentVoxels[Chunk.PosToIndex(x + (int) edge1.x, y + (int) edge1.y,
                                            z + (int) edge1.z)];
                                        delta = s1 - currentVoxels[Chunk.PosToIndex(x + (int) edge2.x,
                                            y + (int) edge2.y, z + (int) edge2.z)];
                                    }

                                    if (delta == 0.0f) ofst = 0.5f;
                                    else ofst = s1 / delta;
                                    middle = edge1 + ofst * (edge2 - edge1);
                                }
                                else
                                {
                                    middle = (edge1 + edge2) * 0.5f;
                                }
                                
                                edge1 /= scale;
                                edge2 /= scale;

                                float voxel1;
                                float voxel2;
                                float voxelValue;
                                if (x == Chunk.ChunkSize - 1 || z == Chunk.ChunkSize - 1)
                                {
                                    voxel1 = GetVoxelAt(x + (int)edge1.x, y + (int)edge1.y,
                                        z + (int)edge1.z, origin, scale);
                                    voxel2 = GetVoxelAt(x + (int)edge2.x,
                                        y + (int)edge2.y, z + (int)edge2.z, origin, scale);
                                }
                                else
                                {
                                    voxel1 = currentVoxels[Chunk.PosToIndex(x + (int)edge1.x, y + (int)edge1.y,
                                        z + (int)edge1.z)];
                                    voxel2 = currentVoxels[Chunk.PosToIndex(x + (int)edge2.x,
                                        y + (int)edge2.y, z + (int)edge2.z)];
                                }

                                if (voxel1 > 0)
                                {
                                    voxelValue = voxel1;
                                }
                                else
                                {
                                    voxelValue = voxel2;
                                }

                                if (triangleCorner == 0)
                                {
                                    voxelTypes.x = voxelValue;
                                }
                                else if (triangleCorner == 1)
                                {
                                    voxelTypes.y = voxelValue;
                                }
                                else
                                {
                                    voxelTypes.z = voxelValue;
                                }

                                vertices.Add(offset + middle);
                                triangles.Add(index++);
                            }

                            for (int i = 0; i < 3; i++)
                            {
                                voxelUv.Add(voxelTypes);
                                baryUv.Add(barycentricCoords[i]);
                            }
                        }
                    }
                }
            }

            // vertices = new NativeArray<Vector3>(Vertices.ToArray(), Allocator.Persistent);
            // triangles = new NativeArray<int>(Triangles.ToArray(), Allocator.Persistent);
            // uv0 = new NativeArray<Vector4>(voxelUv.ToArray(), Allocator.Persistent);
            // uv1 = new NativeArray<Vector4>(baryUv.ToArray(), Allocator.Persistent);
        }
        
        private float GetVoxelAt(float x, float y, float z, Vector3 chunkPos, float scale)
        {
            NativeArray<byte> toCheck =
                new NativeArray<byte>(Chunk.ChunkSize * Chunk.ChunkHeight * Chunk.ChunkSize, Allocator.Temp);

            var noise = false;
            
            //Neighbour checking for chunks
            if (currentVoxels.IsCreated && x != Chunk.ChunkSize && z != Chunk.ChunkSize) toCheck = currentVoxels;
            else if (rightVoxels.IsCreated && x == Chunk.ChunkSize && z != Chunk.ChunkSize) toCheck = rightVoxels;
            else if (forwardVoxels.IsCreated && x != Chunk.ChunkSize && z == Chunk.ChunkSize) toCheck = forwardVoxels;
            else if (rightForwardVoxels.IsCreated && x == Chunk.ChunkSize && z == Chunk.ChunkSize)
                toCheck = rightForwardVoxels;
            else noise = true;

            if (noise) return BiomeGenerator.GenerateVoxelType(chunkPos.x + x * scale, chunkPos.y + y * scale, chunkPos.z + z * scale, noiseScale, seed, groundLevel);

            if (x == Chunk.ChunkSize) x = 0;
            if (z == Chunk.ChunkSize) z = 0;

            var index = Chunk.PosToIndex((int) x, (int) y, (int) z);
            return toCheck[index];
        }
    }
}
