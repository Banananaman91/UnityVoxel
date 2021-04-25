using System;
using UnityEngine;
using VoxelTerrain.Grid;

namespace VoxelTerrain.Interactions
{
    [RequireComponent(typeof(VoxelInteraction))]
    public class BasePartInteraction : MonoBehaviour
    {
        [SerializeField] private VoxelInteraction _interaction;

        private void Awake()
        {
            if (!_interaction) return;

            var pos = GridSnapper.SnapToGrid(transform.position);

            pos.y -= 1;

            StartCoroutine(_interaction.UpdateChunks(GridSnapper.SnapToGrid(pos)));
        }
    }
}
