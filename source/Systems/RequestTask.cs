using NativeFileDialogSharp;
using System.Threading.Tasks;
using Worlds;

namespace FileDialogs.Systems
{
    internal readonly struct RequestTask
    {
        public readonly World world;
        public readonly uint entity;
        public readonly Task<DialogResult> task;

        public RequestTask(World world, uint entity, Task<DialogResult> task)
        {
            this.world = world;
            this.entity = entity;
            this.task = task;
        }
    }
}