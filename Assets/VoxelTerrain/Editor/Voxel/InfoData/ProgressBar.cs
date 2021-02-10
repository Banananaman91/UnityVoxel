using UnityEngine;
using UnityEngine.UI;

namespace VoxelTerrain.Editor.Voxel.InfoData
{
    [System.Serializable]
    public struct ProgressBar
    {
        [SerializeField] private Slider _progress;
        [SerializeField] private Text _progressText;

        public Slider Progress => _progress;
        public Text ProgressText => _progressText;
    }
}
