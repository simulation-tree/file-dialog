using FileDialogs.Functions;
using Unmanaged;
using Worlds;

namespace FileDialogs.Components
{
    [Component]
    public struct IsFileDialog
    {
        public Chosen callback;
        public FileDialogStatus state;
        public FileDialogType type;
        public FixedString filter;
        public FixedString defaultPath;
        public ulong userData;

        public IsFileDialog(Chosen callback, FileDialogType type, FixedString filter, FixedString defaultPath, ulong userData)
        {
            this.callback = callback;
            this.type = type;
            this.filter = filter;
            this.defaultPath = defaultPath;
            this.userData = userData;
            state = FileDialogStatus.Uninitialized;
        }
    }
}
