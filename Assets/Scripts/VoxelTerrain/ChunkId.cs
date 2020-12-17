﻿using System;

namespace VoxelTerrain
{
    public readonly struct ChunkId
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;

        public ChunkId(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        #region Equality Members

        public bool Equals(ChunkId other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
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
                var hashCode = X;
                hashCode = (hashCode * 397) ^ Y;
                hashCode = (hashCode * 397) ^ Z;
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