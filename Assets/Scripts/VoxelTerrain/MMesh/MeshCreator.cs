﻿using System;
using System.Collections.Generic;
using UnityEngine;
using VoxelTerrain.DataConversion;
using VoxelTerrain.Engine;
using VoxelTerrain.Engine.Dependencies;

namespace VoxelTerrain.MMesh
{
    public readonly struct MeshCreator
    {
        public readonly List<Vector3> Vertices;
        public readonly List<int> Triangles;
        public readonly List<Vector4> uv0;
        public readonly List<Vector4> uv1;
        static readonly Vector4[] barycentricCoords = new Vector4[] {new Vector4(1, 0, 0, 1), new Vector4(0, 1, 0, 1), new Vector4(0, 0, 1, 1)};
        private readonly World _world;

        public MeshCreator(World world)
        {
            _world = world;
            Vertices = new List<Vector3>();
            Triangles = new List<int>();
            uv0 = new List<Vector4>();
            uv1 = new List<Vector4>();
        }

        public void SetMesh(Voxel[] Voxels, float x, float y, float z, float size, bool interpolate)
        {
            MarchingCubes(Voxels, size, new Vector3(x, y, z), interpolate);
        }

        private void MarchingCubes(IReadOnlyList<Voxel> voxels, float voxelSize, Vector3 origin, bool interpolate)
        {
            var index = 0;

            Vertices.Clear();
            Triangles.Clear();
            uv0.Clear();
            uv1.Clear();

            var afCubes = new float[8];

            for (var x = 0; x < Chunk.ChunkSize; x++)
            {
                for (var y = 0; y < Chunk.ChunkHeight - 1; y++)
                {
                    for (var z = 0; z < Chunk.ChunkSize; z++)
                    {
                        //Offsets are same as cornerOffsets[8]
                        afCubes[0] = voxels[Converter.PosToIndex(x, y, z)].Value;
                        afCubes[1] = voxels[Converter.PosToIndex(x + 1, y, z)].Value;
                        afCubes[2] = voxels[Converter.PosToIndex(x + 1, y + 1, z)].Value;
                        afCubes[3] = voxels[Converter.PosToIndex(x, y + 1, z)].Value;
                        afCubes[4] = voxels[Converter.PosToIndex(x, y, z + 1)].Value;
                        afCubes[5] = voxels[Converter.PosToIndex(x + 1, y, z + 1)].Value;
                        afCubes[6] = voxels[Converter.PosToIndex(x + 1, y + 1, z + 1)].Value;
                        afCubes[7] = voxels[Converter.PosToIndex(x, y + 1, z + 1)].Value;


                        //Calculate the index of the current cube configuration as follows:
                        //Loop over each of the 8 corners of the cube, and set the corresponding
                        //bit to 1 if its value is below the surface level.
                        //this will result in a value between 0 and 255

                        var flagIndex = 0;
                        for (var vtest = 0; vtest < 8; vtest++)
                        {
                            if (afCubes[vtest] <= 0.0f)
                                flagIndex |= 1 << vtest;
                        }

                        //Skip to next if all corners are the same
                        if (flagIndex == 0x00 || flagIndex == 0xFF)
                            continue;

                        //Get the offset of this current block
                        var offset = new Vector3(x * voxelSize, y * voxelSize, z * voxelSize);

                        for (var triangle = 0; triangle < 5; triangle++)
                        {
                            var edgeIndex = VoxelLookUp.a2iTriangleConnectionTable[flagIndex][3 * triangle];

                            if (edgeIndex < 0)
                                continue; //Skip if the edgeIndex is -1

                            var voxelTypes = new Vector4(0, 0, 0, 1);
                            for (var triangleCorner = 0; triangleCorner < 3; triangleCorner++)
                            {
                                edgeIndex =
                                    VoxelLookUp.a2iTriangleConnectionTable[flagIndex][3 * triangle + triangleCorner];

                                var edge1 = VoxelLookUp.edgeVertexOffsets[edgeIndex, 0];
                                var edge2 = VoxelLookUp.edgeVertexOffsets[edgeIndex, 1];

                                edge1 *= voxelSize;
                                edge2 *= voxelSize;

                                Vector3 middle;
                                if (interpolate)
                                {
                                    float ofst;
                                    float s1;
                                    float delta;
                                    s1 = voxels[Converter.PosToIndex(x + (int) edge1.x, y + (int) edge1.y,
                                        z + (int) edge1.z)].Value;
                                    delta = s1 - voxels[Converter.PosToIndex(x + (int) edge2.x,
                                        y + (int) edge2.y, z + (int) edge2.z)].Value;


                                    ofst = s1 / delta;
                                    middle = edge1 + ofst * (edge2 - edge1);
                                }
                                else
                                {
                                    middle = (edge1 + edge2) * 0.5f;
                                }

                                edge1 /= voxelSize;
                                edge2 /= voxelSize;

                                float voxel1;
                                float voxel2;
                                voxel1 = voxels[Converter.PosToIndex(x + (int) edge1.x, y + (int) edge1.y,
                                    z + (int) edge1.z)].Type;
                                voxel2 = voxels[Converter.PosToIndex(x + (int) edge2.x,
                                    y + (int) edge2.y, z + (int) edge2.z)].Type;


                                var voxelValue = voxel1 > 0 ? voxel1 : voxel2;

                                switch (triangleCorner)
                                {
                                    case 0:
                                        voxelTypes.x = voxelValue;
                                        break;
                                    case 1:
                                        voxelTypes.y = voxelValue;
                                        break;
                                    default:
                                        voxelTypes.z = voxelValue;
                                        break;
                                }

                                Vertices.Add(offset + middle);
                                Triangles.Add(index++);
                            }

                            for (var i = 0; i < 3; i++)
                            {
                                uv0.Add(voxelTypes);
                                uv1.Add(barycentricCoords[i]);
                            }
                        }
                    }
                }
            }
        }
    }
}