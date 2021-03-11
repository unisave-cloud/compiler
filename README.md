# Unisave Compiler

This repository contains the service that performs backend compilation.
It uses the [Roslyn](https://github.com/dotnet/roslyn) compiler that
comes with Mono, packaged as an HTTP service in a Docker container.

It tries to provide similar compilation to what
[Unity performs](https://docs.unity3d.com/2020.1/Documentation/Manual/CSharpCompiler.html).


## Compilation API

`POST /compile-backend`

The request to compile a backend that has been uploaded to the cloud
storage has the following structure:

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
    "framework_version": "0.10.2",
    "checked": false,
    "unsafe": false,
    "lang_version": "7.3",
    "define_symbols": ["FOO_BAR"]
}
```

The request is authorized using basic HTTP auth where the user is `api`
and the password is the security token itself.

> **Note:** Define symbols will be produced by the unisave asset,
> because they need to be included in the backend hash. Some symbols
> should be defined automatically, like `ENABLE_MONO`,
> `CSHARP_7_3_OR_NEWER`, or `UNISAVE_SERVER`, see:
> https://docs.unity3d.com/Manual/PlatformDependentCompilation.html

The compilation results are uploaded into the cloud storage.

The response has the following format:

```json
{
    "success": true,
    "message": "Compilation was successful.",
    "output": "<what the compiler prints>"
}
```

or maybe:

```json
{
    "success": false,
    "message": "Compilation error.",
    "output": "Compilation failed: 1 error(s)..."
}
```

or:

```json
{
    "success": false,
    "message": "The service experienced an internal error."
}
```


## .NET Framework version

The system currently uses the default .NET framework, which in this case
is some 4.x version, not sure which. Should you specify this in the
future, here are some resources that might be helpful:

- [Referencing additional class library assemblies (Unity docs)](https://docs.unity3d.com/Manual/dotnetProfileAssemblies.html)
- [`csc` for .NET Standard 2.0 (stack overflow)](https://stackoverflow.com/questions/57484713/how-can-i-compile-a-net-standard-2-0-class-library-by-directly-invoking-the-c-s)
- Investigate `-nostdlib+` flag
- Investigate `/usr/lib/mono/4.x.x-api` folders in the Mono container
