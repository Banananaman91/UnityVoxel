namespace VoxelTerrain
{
    public readonly struct ChunkId
    {
        private readonly int _x;
        private readonly int _y;
        private readonly int _z;

        public ChunkId(int x, int y, int z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        #region Equality Members

        public bool Equals(ChunkId other)
        {
            return _x == other._x && _y == other._y && _z == other._z;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is ChunkId other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _x;
                hashCode = (hashCode * 397) ^ _y;
                hashCode = (hashCode * 397) ^ _z;
                return hashCode;
            }
        }

        public static bool operator ==(ChunkId left, ChunkId right)
        {
            return !left.Equals(right);
        }

        public static bool operator !=(ChunkId left, ChunkId right)
        {
            return !left.Equals(right);
        }

        #endregion

        public static ChunkId FromWorldPos(int x, int y, int z)
        {
            //return new ChunkId(x << 4, y << 4, z << 4);
            return new ChunkId(x, y, z);
        }
    }
}
