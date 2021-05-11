namespace VoxelTerrain.Engine
{
    public struct Voxel
    {
        public byte Type;
        public float Value;

        public Voxel(byte type, float value)
        {
            Type = type;
            Value = value;
        }
    }
}
