using FileDialogs.Functions;
using Unmanaged;

namespace FileDialogs.Components
{
    public struct IsFileDialog
    {
        public Chosen callback;
        public FileDialogStatus state;
        public FileDialogType type;
        public ASCIIText256 filter;
        public ASCIIText256 defaultPath;
        public ulong userData;

        public IsFileDialog(Chosen callback, FileDialogType type, ASCIIText256 filter, ASCIIText256 defaultPath, ulong userData)
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
