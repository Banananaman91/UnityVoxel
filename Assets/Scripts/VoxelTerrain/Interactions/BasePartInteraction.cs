using System;
using UnityEngine;
using VoxelTerrain.Grid;
using VoxelTerrain.Mouse;

namespace VoxelTerrain.Interactions
{
    [RequireComponent(typeof(VoxelInteraction))]
    public class BasePartInteraction : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private VoxelInteraction _interaction;
#pragma warning restore 0649

        private void Awake()
        {
            if (!_interaction) return;

            var pos = GridSnapper.SnapToGrid(transform.position);

            pos.y -= 1;

            StartCoroutine(_interaction.UpdateChunks(GridSnapper.SnapToGrid(pos)));
        }
    }
}
