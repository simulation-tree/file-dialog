using FileDialogs.Components;
using NativeFileDialogSharp;
using Simulation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unmanaged;
using Worlds;

namespace FileDialogs.Systems
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
            ComponentType componentType = world.Schema.GetComponentType<IsFileDialog>();
            foreach (Chunk chunk in world.Chunks)
            {
                if (chunk.Definition.ContainsComponent(componentType))
                {
                    ReadOnlySpan<uint> entities = chunk.Entities;
                    Span<IsFileDialog> components = chunk.GetComponents<IsFileDialog>(componentType);
                    for (int i = 0; i < entities.Length; i++)
                    {
                        ref IsFileDialog fileDialog = ref components[i];
                        if (fileDialog.state == FileDialogStatus.Uninitialized)
                        {
                            fileDialog.state = FileDialogStatus.Shown;
                            Task<DialogResult> task;
                            if (fileDialog.type == FileDialogType.OpenMultipleFiles)
                            {
                                task = OpenMultipleFiles(fileDialog.filter, fileDialog.defaultPath);
                            }
                            else if (fileDialog.type == FileDialogType.OpenFile)
                            {
                                task = OpenFile(fileDialog.filter, fileDialog.defaultPath);
                            }
                            else if (fileDialog.type == FileDialogType.SaveFile)
                            {
                                task = SaveFile(fileDialog.filter, fileDialog.defaultPath);
                            }
                            else if (fileDialog.type == FileDialogType.ChooseDirectory)
                            {
                                task = ChooseDirectory(fileDialog.defaultPath);
                            }
                            else
                            {
                                throw new NotSupportedException($"File dialog type `{fileDialog.type}` is not supported");
                            }

                            requestList.Add((world, entities[i], task));
                        }
                    }
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
                    component.state = FileDialogStatus.Completed;
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

                    Span<Text> paths = stackalloc Text[pathCount];
                    int index = 0;
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

                    for (int i = 0; i < pathCount; i++)
                    {
                        paths[i].Dispose();
                    }
                }
                else
                {
                    if (result.IsCancelled)
                    {
                        component.state = FileDialogStatus.Cancelled;
                        Span<Text> paths = stackalloc Text[0];
                        component.callback.Invoke(world, component.type, component.state, paths, component.userData);
                    }
                    else
                    {
                        component.state = FileDialogStatus.Failed;
                        Span<Text> paths = stackalloc Text[1];
                        paths[0] = new(result.ErrorMessage);
                        component.callback.Invoke(world, component.type, component.state, paths, component.userData);
                        paths[0].Dispose();
                    }
                }
            }
            else
            {
                component.state = FileDialogStatus.Failed;
                Span<Text> paths = stackalloc Text[1];
                paths[0] = new("NotCompletedSuccessfully");
                component.callback.Invoke(world, component.type, component.state, paths, component.userData);
                paths[0].Dispose();
            }
        }

        private static Task<DialogResult> OpenMultipleFiles(ASCIIText256 filter, ASCIIText256 defaultPath)
        {
            return Task.Factory.StartNew(() => Dialog.FileOpenMultiple(filter.ToString(), defaultPath.ToString()));
        }

        private static Task<DialogResult> OpenFile(ASCIIText256 filter, ASCIIText256 defaultPath)
        {
            return Task.Factory.StartNew(() => Dialog.FileOpen(filter.ToString(), defaultPath.ToString()));
        }

        private static Task<DialogResult> SaveFile(ASCIIText256 filter, ASCIIText256 defaultPath)
        {
            return Task.Factory.StartNew(() => Dialog.FileSave(filter.ToString(), defaultPath.ToString()));
        }

        private static Task<DialogResult> ChooseDirectory(ASCIIText256 defaultPath)
        {
            return Task.Factory.StartNew(() => Dialog.FolderPicker(defaultPath.ToString()));
        }
    }
}