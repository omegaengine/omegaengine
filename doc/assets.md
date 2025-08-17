# Assets

An asset is a content file loaded by the engine at runtime. This includes textures, models, sounds, etc.. These files are located using a [Virtual File System](#vfs).

Asset files are often referenced many times during an application's runtime. To prevent repeated load delays the engine keeps loaded and parsed content in an in-memory cache that can be flushed, e.g. after switching maps.

## VFS

The Virtual File System (VFS) combines multiple directory structures into a single view of the filesystem used to load assets.

Search order:

1. Mod
   - Directory specified via the `/mod` command-line argument  
     (only if implemented by the game)
   - Directories specified in the `OMEGAENGINE_CONTENT_MOD` environment variable  
     (only if the `/mod` command-line argument was not used)
2. Base
   - Directory specified in game settings  
     (only if implemented by the game)
   - Directories specified in the `OMEGAENGINE_CONTENT` environment variable  
     (only if not overriden by game settings)
   - The `content` directory next to the game's executable  
     (only if not overriden by the `OMEGAENGINE_CONTENT` environment variable or game settings)

### Base directory

The base directory is usually located in the directory of the application EXE and named `base`. This location can be overridden in the engine configuration.

## Mods

A game modification (mod) is a set of changes based on an existing game used to modify existing gameplay, add additional content or create an entirely new game.

The engine supports mods via a [virtual file system](#vfs) and modding support in the AlphaEditor.
