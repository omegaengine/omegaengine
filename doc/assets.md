# Assets

An asset is a content file loaded by the engine at runtime. This includes textures, models, sounds, etc.. These files are located using a [Virtual File System](#vfs).

Asset files are often referenced many times during an application's runtime. To prevent repeated load delays the engine keeps loaded and parsed content in an in-memory cache that can be flushed, e.g. after switching maps.

## VFS

The Virtual File System (VFS) combines multiple directory structures into a single view of the filesystem used to load assets.

Search order:

1. Files in the active [mod](#mods) directory, if any
1. Data archives in the active [mod](#mods) directory, if any
1. Files in the application's base directory
1. Data archives in the application's base directory

### Base directory

The base directory is usually located in the directory of the application EXE and named `base`. This location can be overridden in the engine configuration.

### Data archives

The data archives provide a a way to combine the entire game content (maps, scripts, models, textures, ...) into a few large files. This simplifies installation and updating and also reduces the likelihood of users accidentally modifying the content.

The data archives are normal ZIP archives, optionally using a file extensions different from `.zip`. They should be stored in uncompressed form to speed up loading processes.

## Mods

A game modification (mod) is a set of changes based on an existing game used to modify existing gameplay, add additional content or create an entirely new game.

The engine supports mods via a [virtual file system](#vfs) and modding support in the AlphaEditor.
