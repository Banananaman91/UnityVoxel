using UnityEngine;

namespace VoxelTerrain.Engine.InfoData
{
    public class NoiseInfo : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private float _noiseScale;
        [SerializeField] private float _frequency;
        [SerializeField] private float _amplitude;
        [SerializeField] private int _octaves;
        [SerializeField] private float _lacunarity;
#pragma warning restore 0649

        public float NoiseScale => _noiseScale;
        public float Frequency => _frequency;
        public float Amplitude => _amplitude;
        public int Octaves => _octaves;
        public float Lacunarity => _lacunarity;
    }
}
