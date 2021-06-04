using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace VoxelTerrain.Interactions
{
    [Serializable]
    public class VfxInteraction
    {
        private VisualEffect _vfx;

#pragma warning disable 0649
        [SerializeField] private string _spawnRateStringId = "Enter ID";
        [SerializeField] private int _spawnRate;

        [SerializeField] private string _spawnRateBoxStringId = "Enter ID";
        [SerializeField] private int _spawnRateBox;

        [SerializeField] private string _spawnPointStringId = "Enter ID";

        [SerializeField] private string _particleLifeStringId = "Enter ID";
        [SerializeField] private float _particleLife;

        [SerializeField] private string _sphereRadiusStringId = "Enter ID";
        [SerializeField] private float _sphereRadius;

        [SerializeField] private string _boxRadiusXStringId = "Enter ID";
        [SerializeField] private float _boxRadiusX;
        [SerializeField] private string _boxRadiusZStringId = "Enter ID";
        [SerializeField] private float _boxRadiusZ;

        [SerializeField] private string _particleRingCountStringId;
        [SerializeField] private float _particleRingCount;

        [SerializeField] private string _sparkSpawnRateStringId;
        [SerializeField] private int _sparkSpawnRate;
#pragma warning restore 0649

        public VisualEffect[] Vfx = new VisualEffect[10];

        public FlattenShape Shape { get; set; }

        #region Essentials
        
        public string SpawnRateStringId
        {
            get => _spawnRateStringId;
            set => _spawnRateStringId = value;
        }
        
        public int SpawnRate
        {
            get => _spawnRate;
            set => _spawnRate = value;
        }

        public string ParticleLifeStringId
        {
            get => _particleLifeStringId;
            set => _particleLifeStringId = value;
        }

        public float ParticleLife
        {
            get => _particleLife;
            set => _particleLife = value;
        }

        public string SpawnPointStringId
        {
            get => _spawnPointStringId;
            set => _spawnPointStringId = value;
        }

        #endregion

        #region Sphere

        public string SphereRadiusStringId
        {
            get => _sphereRadiusStringId;
            set => _sphereRadiusStringId = value;
        }

        public float SphereRadius
        {
            get => _sphereRadius;
            set => _sphereRadius = value;
        }

        #endregion

        #region Square

        public string BoxRadiusXStringId
        {
            get => _boxRadiusXStringId;
            set => _boxRadiusXStringId = value;
        }
        
        public float BoxRadiusX
        {
            get => _boxRadiusX;
            set => _boxRadiusX = value;
        }
        
        public string BoxRadiusZStringId
        {
            get => _boxRadiusZStringId;
            set => _boxRadiusZStringId = value;
        }
        
        public float BoxRadiusZ
        {
            get => _boxRadiusZ;
            set => _boxRadiusZ = value;
        }

        #endregion
        
        public void VfxPlaya(Vector3 spawnPoint, int voxelType, InteractionSettings interactionSettings, FlattenShape shape = FlattenShape.Single)
        {

            if (_vfx == null || _vfx != Vfx[voxelType]) _vfx = Vfx[voxelType];

            _vfx.SetInt(_spawnRateStringId, _spawnRate);
            switch (shape)
            {
                case FlattenShape.Single:
                    _vfx.SetInt(_spawnRateStringId, _spawnRate);


                    break;
                case FlattenShape.Square:
                    _vfx.SetInt(_spawnRateBoxStringId, _spawnRateBox);
                    _vfx.SetFloat(_particleLifeStringId, _particleLife);
                    _vfx.SetFloat(_boxRadiusXStringId, _boxRadiusX = interactionSettings.CubeXDistance);
                    _vfx.SetFloat(_boxRadiusZStringId, _boxRadiusZ = interactionSettings.CubeZDistance);
                    _vfx.SetVector3(_spawnPointStringId, spawnPoint);
                    _vfx.SetInt(_sparkSpawnRateStringId, _sparkSpawnRate);
                    _vfx.SetInt(_spawnRateStringId, 0);
                    break;
                case FlattenShape.Circular:
                    _vfx.SetInt(_spawnRateStringId, _spawnRate);


                    break;
                case FlattenShape.Sphere:
                    _vfx.SetInt(_spawnRateStringId, _spawnRate);
                    _vfx.SetFloat(_particleLifeStringId, _particleLife);
                    _vfx.SetFloat(_sphereRadiusStringId,
                        _sphereRadius = interactionSettings.SphereRadius / 100f + 0.04f);
                    _vfx.SetVector3(_spawnPointStringId, spawnPoint);
                    _vfx.SetFloat(_particleRingCountStringId,
                        _particleRingCount); // this is not an accidient, ring count needs to be the same as the spawn rate or higher
                    _vfx.SetInt(_sparkSpawnRateStringId, _sparkSpawnRate);
                    _vfx.SetInt(_spawnRateBoxStringId, 0);
                    break;
                default:
                    break;
            }
        }

        public void VfxStopa() // may need to add a small delay before this method is run on the voxelinteraction script
        {
            _vfx.SetInt(_spawnRateStringId, 0);
            _vfx.SetInt(_sparkSpawnRateStringId, 0);
            _vfx.SetInt(_spawnRateBoxStringId, 0);
        }
    }
}
