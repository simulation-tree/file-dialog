# File Dialog

Implements [`NativeFileDialogSharp`](https://github.com/simulation-tree/NativeFileDialogSharp) asynchronously.

### Features
- Open multiple files
- Open and save single files
- CHoose directories

### Usage

Using this library is done by creating temporary `FileDialogEntity` entities:
```cs
using World = new();
using Simulation simulator = new();
using Text selectedPath = new();
simulator.AddSystem<FileDialogSystem>();

FileDialogEntity.OpenFile(world, new(&ChosenFile), userData: selectedPath.Address);

while (selectedPath.Length == 0)
{
    simulator.Update();
}

Console.WriteLine(selectedPath);

[UnmanagedCallersOnly]
static void ChosenFile(Chosen.Input input)
{
    Text selectedPath = new((nint)input.userData);
    if (input.status == FileDialogEntity.Status.Cancelled)
    {
        selectedPath.CopyFrom("Cancelled");
    }
    else
    {
        selectedPath.CopyFrom(input.Paths[0]);
    }
}
```

The function pointer can be reused for other operations, with the `input.type` value allowing for
differentiating between each. As well as `input.status` allowing to check if the operation was cancelled,
failed, or completed successfully.
