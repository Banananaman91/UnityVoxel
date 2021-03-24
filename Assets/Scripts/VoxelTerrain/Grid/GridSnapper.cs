using UnityEngine;

namespace VoxelTerrain.Grid
{
    public static class GridSnapper
    {
        public static Vector3 SnapToGrid(Vector3 position, float size = 1f, float offset = 0.5f)
        {
            var xPos = Mathf.RoundToInt(position.x / size);
            var yPos = Mathf.RoundToInt(position.y / size);
            var zPos = Mathf.RoundToInt(position.z / size);

            var result = new Vector3(
                xPos * size,
                yPos * size,
                zPos * size);
            
            result.x += offset;
            result.y += offset;
            result.z += offset;

            return result;
        }
    }
}
