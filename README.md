# Unisave Compiler

This repository contains the service that performs backend compilation.
It is the [Roslyn](https://github.com/dotnet/roslyn) copmiler that comes with
Mono, packaged as an HTTP service in a Docker container.

It tries to provide similar compilation to what [Unity performs](https://docs.unity3d.com/2020.1/Documentation/Manual/CSharpCompiler.html).


## Compilation API

The request to compile a backend that has been uploaded has the following structure:

```json
{
    "game_id": "0DIbfKDpVNxtWhME",
    "backend_id": "8nkzCBqDxo2YHiUm",
    "files": [
        {
            "path": "Assets/Backend/MyFile.cs",
            "hash": "2a6f6933c33f7530594cc331c298999c"
        },
        {
            "path": "Assets/Backend/MyLib.dll",
            "hash": "a52da9bfbaa6841965d1dc7e2b4af586"
        }
    ],
    "framework_version": "0.10.2"
}
```

The request is authorized using basic HTTP auth where the user is `token`
and the password is the security token itself.
