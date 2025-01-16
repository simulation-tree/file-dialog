using FileDialog.Components;
using NativeFileDialogSharp;
using Simulation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unmanaged;
using Worlds;

namespace FileDialog.Systems
{
    public readonly partial struct FileDialogSystem : ISystem
    {
        private static readonly Dictionary<SystemContainer, List<(World, uint, Task<DialogResult>)>> requests = new();

        void ISystem.Start(in SystemContainer systemContainer, in World world)
        {
            requests[systemContainer] = new();
        }

        void ISystem.Update(in SystemContainer systemContainer, in World world, in TimeSpan delta)
        {
            //check each task if theyre finished
            List<(World, uint, Task<DialogResult>)> requestList = requests[systemContainer];
            for (int i = requestList.Count - 1; i >= 0; i--)
            {
                (World world, uint entity, Task<DialogResult> task) request = requestList[i];
                if (request.task.IsCompleted)
                {
                    requestList.RemoveAt(i);
                    DialogResult? result = request.task.IsCompletedSuccessfully ? request.task.Result : null;
                    HandleDialogResult(request.world, request.entity, result);
                    request.world.DestroyEntity(request.entity);
                }
            }

            //start tasks
            ComponentQuery<IsFileDialog> query = new(world);
            query.ExcludeDisabled(true);
            foreach (var r in query)
            {
                ref IsFileDialog fileDialog = ref r.component1;
                if (fileDialog.state == FileDialogEntity.Status.Uninitialized)
                {
                    fileDialog.state = FileDialogEntity.Status.Shown;
                    Task<DialogResult> task;
                    if (fileDialog.type == FileDialogEntity.Type.OpenMultipleFiles)
                    {
                        task = OpenMultipleFiles(fileDialog.filter, fileDialog.defaultPath);
                    }
                    else if (fileDialog.type == FileDialogEntity.Type.OpenFile)
                    {
                        task = OpenFile(fileDialog.filter, fileDialog.defaultPath);
                    }
                    else if (fileDialog.type == FileDialogEntity.Type.SaveFile)
                    {
                        task = SaveFile(fileDialog.filter, fileDialog.defaultPath);
                    }
                    else if (fileDialog.type == FileDialogEntity.Type.ChooseDirectory)
                    {
                        task = ChooseDirectory(fileDialog.defaultPath);
                    }
                    else
                    {
                        throw new NotSupportedException($"File dialog type `{fileDialog.type}` is not supported");
                    }

                    requestList.Add((world, r.entity, task));
                }
            }
        }

        void ISystem.Finish(in SystemContainer systemContainer, in World world)
        {
            //todo: implement cancelling those tasks
        }

        private static void HandleDialogResult(World world, uint entity, DialogResult? result)
        {
            ref IsFileDialog component = ref world.GetComponent<IsFileDialog>(entity);
            if (result is not null)
            {
                if (result.IsOk)
                {
                    component.state = FileDialogEntity.Status.Completed;
                    string? path = result.Path;
                    if (path?.Length == 0)
                    {
                        path = null;
                    }

                    int pathCount = 0;
                    if (path is not null)
                    {
                        pathCount++;
                    }

                    if (result.Paths is not null)
                    {
                        pathCount += result.Paths.Count;
                    }

                    USpan<Text> paths = stackalloc Text[pathCount];
                    uint index = 0;
                    if (path is not null)
                    {
                        paths[index++] = new(path);
                    }

                    if (result.Paths is not null)
                    {
                        for (uint i = 0; i < result.Paths.Count; i++)
                        {
                            paths[index++] = new(result.Paths[(int)i]);
                        }
                    }

                    component.callback.Invoke(world, component.type, component.state, paths, component.userData);

                    for (uint i = 0; i < pathCount; i++)
                    {
                        paths[i].Dispose();
                    }
                }
                else
                {
                    if (result.IsCancelled)
                    {
                        component.state = FileDialogEntity.Status.Cancelled;
                        USpan<Text> paths = stackalloc Text[0];
                        component.callback.Invoke(world, component.type, component.state, paths, component.userData);
                    }
                    else
                    {
                        component.state = FileDialogEntity.Status.Failed;
                        USpan<Text> paths = stackalloc Text[1];
                        paths[0] = new(result.ErrorMessage);
                        component.callback.Invoke(world, component.type, component.state, paths, component.userData);
                        paths[0].Dispose();
                    }
                }
            }
            else
            {
                component.state = FileDialogEntity.Status.Failed;
                USpan<Text> paths = stackalloc Text[1];
                paths[0] = new("NotCompletedSuccessfully");
                component.callback.Invoke(world, component.type, component.state, paths, component.userData);
                paths[0].Dispose();
            }
        }

        private static Task<DialogResult> OpenMultipleFiles(FixedString filter, FixedString defaultPath)
        {
            return Task.Factory.StartNew(() => Dialog.FileOpenMultiple(filter.ToString(), defaultPath.ToString()));
        }

        private static Task<DialogResult> OpenFile(FixedString filter, FixedString defaultPath)
        {
            return Task.Factory.StartNew(() => Dialog.FileOpen(filter.ToString(), defaultPath.ToString()));
        }

        private static Task<DialogResult> SaveFile(FixedString filter, FixedString defaultPath)
        {
            return Task.Factory.StartNew(() => Dialog.FileSave(filter.ToString(), defaultPath.ToString()));
        }

        private static Task<DialogResult> ChooseDirectory(FixedString defaultPath)
        {
            return Task.Factory.StartNew(() => Dialog.FolderPicker(defaultPath.ToString()));
        }
    }
}