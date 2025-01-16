using FileDialog.Functions;
using Unmanaged;
using Worlds;

namespace FileDialog.Components
{
    [Component]
    public struct IsFileDialog
    {
        public Chosen callback;
        public FileDialogEntity.Status state;
        public FileDialogEntity.Type type;
        public FixedString filter;
        public FixedString defaultPath;
        public ulong userData;

        public IsFileDialog(Chosen callback, FileDialogEntity.Type type, FixedString filter, FixedString defaultPath, ulong userData)
        {
            this.callback = callback;
            this.type = type;
            this.filter = filter;
            this.defaultPath = defaultPath;
            this.userData = userData;
            state = FileDialogEntity.Status.Uninitialized;
        }
    }
}
