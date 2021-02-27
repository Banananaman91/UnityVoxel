using UnityEngine;
using UnityEngine.UI;

namespace VoxelTerrain.Editor.Voxel.InfoData
{
    public class ProgressBar : MonoBehaviour
    {
        [SerializeField] private Slider _progress;
        [SerializeField] private Text _progressText;

        public Slider Progress => _progress;
        public Text ProgressText => _progressText;
    }
}
