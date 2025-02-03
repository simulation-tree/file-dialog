namespace FileDialogs
{
    public enum FileDialogStatus : byte
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