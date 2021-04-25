using System;
using UnityEngine;

namespace VoxelTerrain.Interactions
{
    [CreateAssetMenu(fileName = "Vfx Interaction", menuName = "VFX/Interaction", order = 1), Serializable]
    public class ScriptableVfxInteract : ScriptableObject
    {
        [SerializeField] VfxInteraction _vfxInteraction;

        public VfxInteraction VFXInteraction
        {
            get => _vfxInteraction;
            set => _vfxInteraction = value;
        }
    }
}
