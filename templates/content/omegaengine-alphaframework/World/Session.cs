using System;
using System.ComponentModel;
using System.IO;
using AlphaFramework.World;
using ICSharpCode.SharpZipLib.Zip;
using OmegaEngine.Foundation.Storage;

namespace Template.AlphaFramework.World;

/// <summary>
/// Represents a game session (i.e. a game actually being played).
/// It is equivalent to the content of a savegame.
/// </summary>
public sealed class Session : Session<Universe>
{
    /// <summary>
    /// Creates a new game session based upon a given <see cref="Universe"/>.
    /// </summary>
    /// <param name="baseUniverse">The universe to base the new game session on.</param>
    public Session(Universe baseUniverse) : base(baseUniverse)
    {}

    /// <summary>
    /// The file extensions when this class is stored as a file.
    /// </summary>
    public const string FileExt = $".{Constants.AppNameShort}Save";

    /// <summary>
    /// Used for encrypting serialized versions of this class.
    /// </summary>
    /// <remarks>This provides only very basic protection against savegame tampering.</remarks>
    private const string EncryptionKey = "Session";

    /// <summary>
    /// Base-constructor for XML serialization. Do not call manually!
    /// </summary>
    public Session()
    {}

    /// <summary>
    /// Loads a <see cref="Session"/> from an encrypted XML file (savegame).
    /// </summary>
    /// <param name="path">The file to load from.</param>
    /// <returns>The loaded <see cref="Session"/>.</returns>
    /// <exception cref="IOException">A problem occurred while reading the file.</exception>
    /// <exception cref="UnauthorizedAccessException">Read access to the file is not permitted.</exception>
    /// <exception cref="InvalidOperationException">A problem occurred while deserializing the XML data.</exception>
    public static Session Load(string path)
    {
        Session session;
        try
        {
            session = XmlZipStorage.LoadXmlZip<Session>(path, EncryptionKey);
        }
        #region Error handling
        catch (ZipException ex)
        {
            throw new IOException(ex.Message, ex);
        }
        #endregion

        // Restore the original map filename
        session.Universe.SourceFile = session.MapSourceFile;

        return session;
    }

    /// <summary>
    /// Saves this session in an encrypted XML file (savegame).
    /// </summary>
    /// <param name="path">The file to save in.</param>
    /// <exception cref="IOException">A problem occurred while writing the file.</exception>
    /// <exception cref="UnauthorizedAccessException">Write access to the file is not permitted.</exception>
    public override void Save(string path)
        => this.SaveXmlZip(path, EncryptionKey);

    /// <summary>
    /// <see cref="IUniverse.GameTime"/> time left over from the last <see cref="Update"/> call due to the fixed update step size.
    /// </summary>
    [DefaultValue(0.0)]
    public double LeftoverGameTime { get; set; }

    /// <inheritdoc/>
    public override double Update(double elapsedRealTime)
    {
        double elapsedGameTime = elapsedRealTime * TimeWarpFactor;
        double gameTimeDelta = LeftoverGameTime + elapsedGameTime;
        LeftoverGameTime = UpdateDeterministic(gameTimeDelta);
        return elapsedGameTime;
    }

    /// <summary>Fixed step size for updates in seconds. Makes updates deterministic.</summary>
    private const double UpdateStepSize = 0.015;

    private double UpdateDeterministic(double gameTimeDelta)
    {
        while (Math.Abs(gameTimeDelta) >= UpdateStepSize)
        {
            // Handle negative time
            double effectiveStep = Math.Sign(gameTimeDelta) * UpdateStepSize;

            Universe.Update(effectiveStep);
            gameTimeDelta -= effectiveStep;
        }

        return gameTimeDelta;
    }
}
