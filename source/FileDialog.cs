using FileDialogs.Components;
using FileDialogs.Functions;
using Unmanaged;
using Worlds;

namespace FileDialogs
{
    public readonly partial struct FileDialog : IEntity
    {
        public FileDialog(World world, FileDialogType type, Chosen callback, FixedString filter, FixedString defaultPath, ulong userData = default)
        {
            this.world = world;
            value = world.CreateEntity(new IsFileDialog(callback, type, filter, defaultPath, userData));
        }

        void IEntity.Describe(ref Archetype archetype)
        {
            archetype.AddComponentType<IsFileDialog>();
        }

        public static FileDialog OpenMultipleFiles(World world, Chosen callback, FixedString filter = default, FixedString defaultPath = default, ulong userData = default)
        {
            return new FileDialog(world, FileDialogType.OpenMultipleFiles, callback, filter, defaultPath, userData);
        }

        public static FileDialog OpenFile(World world, Chosen callback, FixedString filter = default, FixedString defaultPath = default, ulong userData = default)
        {
            return new FileDialog(world, FileDialogType.OpenFile, callback, filter, defaultPath, userData);
        }

        public static FileDialog SaveFile(World world, Chosen callback, FixedString filter = default, FixedString defaultPath = default, ulong userData = default)
        {
            return new FileDialog(world, FileDialogType.SaveFile, callback, filter, defaultPath, userData);
        }

        public static FileDialog ChooseDirectory(World world, Chosen callback, FixedString defaultPath = default, ulong userData = default)
        {
            return new FileDialog(world, FileDialogType.ChooseDirectory, callback, default, defaultPath, userData);
        }
    }
}