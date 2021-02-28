﻿using UnityEngine;
using VoxelTerrain.Editor.Noise;

namespace VoxelTerrain.Editor.Voxel.InfoData
{
    public class NoiseInfo : MonoBehaviour
    {
        [SerializeField] private FastNoiseLite.NoiseType _noiseType;
        [SerializeField] private bool _setCustomSeed;
        [SerializeField] private int _seedValue;
        [SerializeField] private bool _randomSeed;
        [SerializeField] private float _noiseFrequency;
        [SerializeField] private FastNoiseLite.RotationType3D _rotationType3D;
        [SerializeField] private FastNoiseLite.TransformType3D _transformType3D;

        public FastNoiseLite.NoiseType NoiseType => _noiseType;
        public bool SetCustomSeed
        {
            get => _setCustomSeed;
            set => _setCustomSeed = value;
        }

        public int SeedValue => _seedValue;
        public bool RandomSeed
        {
            get => _randomSeed;
            set => _randomSeed = value;
        }

        public float NoiseFrequency => _noiseFrequency;
        public FastNoiseLite.RotationType3D RotationType3D => _rotationType3D;
        public FastNoiseLite.TransformType3D TransformType3D => _transformType3D;
        public bool RandomSeedActive { get; set; }

        public bool CustomSeedActive { get; set; }
    }
}