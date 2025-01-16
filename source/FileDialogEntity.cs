using FileDialog.Components;
using FileDialog.Functions;
using System;
using Unmanaged;
using Worlds;

namespace FileDialog
{
    public readonly struct FileDialogEntity : IEntity
    {
        private readonly Entity entity;

        uint IEntity.Value => entity.GetEntityValue();
        World IEntity.World => entity.GetWorld();

        void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsFileDialog>();
        }

#if NET
        [Obsolete("Default constructor not supported", true)]
        public FileDialogEntity()
        {
            throw new NotSupportedException();
        }
#endif

        public FileDialogEntity(World world, Type type, Chosen callback, FixedString filter, FixedString defaultPath, ulong userData = default)
        {
            entity = new Entity<IsFileDialog>(world, new IsFileDialog(callback, type, filter, defaultPath, userData));
        }

        public readonly void Dispose()
        {
            entity.Dispose();
        }

        public static FileDialogEntity OpenMultipleFiles(World world, Chosen callback, FixedString filter = default, FixedString defaultPath = default, ulong userData = default)
        {
            return new FileDialogEntity(world, Type.OpenMultipleFiles, callback, filter, defaultPath, userData);
        }

        public static FileDialogEntity OpenFile(World world, Chosen callback, FixedString filter = default, FixedString defaultPath = default, ulong userData = default)
        {
            return new FileDialogEntity(world, Type.OpenFile, callback, filter, defaultPath, userData);
        }

        public static FileDialogEntity SaveFile(World world, Chosen callback, FixedString filter = default, FixedString defaultPath = default, ulong userData = default)
        {
            return new FileDialogEntity(world, Type.SaveFile, callback, filter, defaultPath, userData);
        }

        public static FileDialogEntity ChooseDirectory(World world, Chosen callback, FixedString defaultPath = default, ulong userData = default)
        {
            return new FileDialogEntity(world, Type.ChooseDirectory, callback, default, defaultPath, userData);
        }

        public enum Type : byte
        {
            Unknown,
            OpenMultipleFiles,
            OpenFile,
            SaveFile,
            ChooseDirectory
        }

        public enum Status : byte
        {
            /// <summary>
            /// File dialog hasn't displayed itself yet.
            /// </summary>
            Uninitialized,

            /// <summary>
            /// File dialog is shown, and waiting for input.
            /// </summary>
            Shown,

            /// <summary>
            /// A successful input has been given.
            /// </summary>
            Completed,

            /// <summary>
            /// Cancelled selection.
            /// </summary>
            Cancelled,

            /// <summary>
            /// A failure has occurred.
            /// </summary>
            Failed
        }
    }
}