# Procedurally generated voxel terrain engine
With customisable voxel interactions, voxel-based shader and voxel-based VFX controller tool.

## Recent Updates
![HLSL Marching cubes](https://github.com/Banananaman91/UnityVoxel/blob/main/Assets/Scripts/VoxelTerrain/MMesh/MarchingCubes.compute) and ![Layered Noise](https://github.com/Banananaman91/UnityVoxel/blob/main/Assets/Scripts/VoxelTerrain/MMesh/NoiseGenerator.compute)

## WIP
Currently reworking a lot of these systems to improve the terrain as well as performance.
For guarantee, last fully working iteration was at
![Commit ID: 0ac3bdf3c310b0312b3de1fc2ad08dafe98a23c2](https://github.com/Banananaman91/UnityVoxel/tree/0ac3bdf3c310b0312b3de1fc2ad08dafe98a23c2)

## Dependencies
The following packages are required to run:
Universal Render Pipeline
Jobs
Burst
Mathematics
Newtonsoft.json library

# Features

![](VoxelWorld.gif)

## ![Infinite seeded worlds](https://github.com/Banananaman91/UnityVoxel/blob/main/Assets/Scripts/VoxelTerrain/Engine/VoxelEngine.cs)
Generate seeded worlds with continously generated terrain.

![](VoxelGenerating.gif)

## ![Voxel Interactions](https://github.com/Banananaman91/UnityVoxel/blob/main/Assets/Scripts/VoxelTerrain/Mouse/VoxelInteraction.cs)
Customise voxel interactions based on shape and size, offering abilities for destruction/creation as well as flattening surfaces.

![](VoxelInteraction.gif)

## ![Voxel-based Shader](https://github.com/Banananaman91/UnityVoxel/wiki/Voxel-Based-shader)
Material shader uses voxel values to map textures to the mesh, using Barycentric coordinates for per-triangle texturing and Triplanar Mapping with individual offsets.

![](VoxelShader.gif)

## ![Voxel-based VFX control tool](https://github.com/Banananaman91/UnityVoxel/blob/main/Assets/Editor/VfxInteractGui.cs)
Scriptable objects with custom GUI allows the ability to control VFX properties and call specific VFX based on voxel type, allowing all unique VFX based on voxel type and interaction size.

![](https://raw.githubusercontent.com/Banananaman91/UnityVoxel/main/VFXInteraction.PNG)
