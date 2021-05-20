using UnityEngine;

namespace VoxelTerrain.Engine.InfoData
{
    public class NoiseInfo : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private float _noise1Scale;
        [SerializeField] private float _noise2Scale;
        [SerializeField] private float _heightScale;
        [SerializeField] private float _altitudeScale;
        [SerializeField] private float _moistureScale;
        [SerializeField] private float _frequency;
        [SerializeField] private float _amplitude;
        [SerializeField] private int _octaves;
        [SerializeField] private float _lacunarity;
#pragma warning restore 0649

        public float Noise1Scale => _noise1Scale;
        public float Noise2Scale => _noise2Scale;
        public float HeightScale => _heightScale;
        public float AltitudeScale => _altitudeScale;
        public float MoistureScale => _moistureScale;
        public float Frequency => _frequency;
        public float Amplitude => _amplitude;
        public int Octaves => _octaves;
        public float Lacunarity => _lacunarity;
    }
}
