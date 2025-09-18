using System;
using System.IO;
using OmegaEngine.Foundation.Storage;

namespace AlphaFramework.World;

/// <summary>
/// Contains extension methods for <see cref="IUniverse"/>.
/// </summary>
public static class UniverseExtensions
{
    /// <summary>
    /// Overwrites the map file the <paramref name="universe"/> was loaded from with the changed data.
    /// </summary>
    /// <exception cref="IOException">A problem occurred while writing the file.</exception>
    /// <exception cref="UnauthorizedAccessException">Write access to the file is not permitted.</exception>
    public static void Save(IUniverse universe)
    {
        if (universe.SourceFile is not {} sourceFile) return;

        string path = Path.IsPathRooted(sourceFile) ? sourceFile : ContentManager.CreateFilePath("World/Maps", sourceFile);
        universe.Save(path);
    }
}
