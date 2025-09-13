using System;
using Unmanaged;
using Worlds;

namespace FileDialogs.Functions
{
    public unsafe readonly struct Chosen : IEquatable<Chosen>
    {
        private readonly delegate* unmanaged<Input, void> function;

        public Chosen(delegate* unmanaged<Input, void> function)
        {
            this.function = function;
        }

        public readonly override bool Equals(object? obj)
        {
            return obj is Chosen chosen && Equals(chosen);
        }

        public readonly bool Equals(Chosen other)
        {
            return (nint)function == (nint)other.function;
        }

        public readonly override int GetHashCode()
        {
            return ((nint)function).GetHashCode();
        }

        public readonly void Invoke(World world, FileDialogType type, FileDialogStatus status, ReadOnlySpan<Text> paths, ulong userData)
        {
            using MemoryAddress pathsAllocation = MemoryAddress.Allocate(paths);
            function(new Input(world, type, status, pathsAllocation, paths.Length, userData));
        }

        public static bool operator ==(Chosen left, Chosen right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Chosen left, Chosen right)
        {
            return !(left == right);
        }

        public readonly struct Input
        {
            public readonly World world;
            public readonly FileDialogType type;
            public readonly FileDialogStatus status;
            public readonly ulong userData;

            private readonly MemoryAddress paths;
            private readonly int pathCount;

            /// <summary>
            /// Array of paths chosen.
            /// <para>
            /// If <see cref="status"/> is failed, it will contain
            /// the reason at the first index.
            /// </para>
            /// <para>
            /// If <see cref="status"/> is cancelled, it will be empty.
            /// </para>
            /// </summary>
            public readonly ReadOnlySpan<Text> Paths => new(paths.pointer, pathCount);

            public Input(World world, FileDialogType type, FileDialogStatus status, MemoryAddress paths, int pathCount, ulong userData)
            {
                this.world = world;
                this.type = type;
                this.status = status;
                this.paths = paths;
                this.pathCount = pathCount;
                this.userData = userData;
            }
        }
    }
}