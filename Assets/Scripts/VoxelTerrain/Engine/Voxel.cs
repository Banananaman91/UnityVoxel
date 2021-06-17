namespace VoxelTerrain.Engine
{
    public struct Voxel
    {
        public int Type;
        public float Value;

        public Voxel(int type, float value)
        {
            Type = type;
            Value = value;
        }
    }
}
