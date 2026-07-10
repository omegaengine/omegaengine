using AlphaFramework.Editor;
using AlphaFramework.Editor.World.Commands;
using AlphaFramework.World.Positionables;
using OmegaEngine;
using OmegaEngine.Foundation.Storage;
using OmegaEngine.Input;
using SlimDX;
using Template.AlphaFramework.Presentation;
using Template.AlphaFramework.World;

namespace Template.AlphaFramework.Editor.World;

/// <summary>
/// Allows the user to view and edit game maps (serialized <see cref="Universe"/>s).
/// </summary>
public sealed partial class MapEditor : UndoCommandTab
{
    private EditorPresenter? _presenter;
    private ActionReceiver? _renderTrigger;
    private Universe _universe = null!;

    /// <summary>
    /// Creates a new map editor.
    /// </summary>
    /// <param name="filePath">The path to the file to be edited.</param>
    /// <param name="overwrite"><c>true</c> if an existing file is supposed to be overwritten when <see cref="Tab.SaveFile"/> is called.</param>
    public MapEditor(string filePath, bool overwrite)
    {
        InitializeComponent();

        NameUI = "Map Editor";
        FilePath = filePath;
        _overwrite = overwrite;
    }

    /// <inheritdoc/>
    protected override void OnInitialize()
    {
        if (Path.IsPathRooted(FilePath))
        {
            _fullPath = FilePath;
            if (!_overwrite && File.Exists(_fullPath))
                _universe = Universe.Load(_fullPath);
            else
            {
                _universe = new Universe();
                _universe.Save(_fullPath);
            }
        }
        else
        {
            _universe = Universe.FromContent(FilePath);
            _fullPath = ContentManager.CreateFilePath("World/Maps", FilePath);
        }

        renderPanel.Setup();
        base.OnInitialize();
    }

    /// <inheritdoc/>
    protected override void OnUpdate()
    {
        // Set up the presenter on startup or after it was lost/reset
        if (_presenter == null)
        {
            _presenter = new(renderPanel.Engine!, _universe);
            _presenter.Initialize();
            _presenter.HookIn();

            renderPanel.AddInputReceiver(_presenter);
            renderPanel.AddInputReceiver(_renderTrigger = new(() => renderPanel.Engine!.Render()));
        }

        UpdateList();
        UpdatePropertyGrid();

        base.OnUpdate();
    }

    private void UpdateList()
    {
        listBox.BeginUpdate();
        listBox.Items.Clear();
        foreach (var positionable in _universe.Positionables)
            listBox.Items.Add(positionable);
        listBox.EndUpdate();
    }

    private void UpdatePropertyGrid()
        => propertyGrid.SelectedObject = listBox.SelectedItem ?? (object)_universe;

    /// <inheritdoc/>
    protected override void OnDelete()
    {
        if (listBox.SelectedItem is Positionable<Vector3> positionable)
            ExecuteCommand(new RemovePositionables<Vector3>(_universe, [positionable]));
    }

    /// <inheritdoc/>
    protected override void OnSaveFile()
    {
        string? directory = Path.GetDirectoryName(_fullPath);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        _universe.Save(_fullPath);

        base.OnSaveFile();
    }

    private void buttonAdd_Click(object sender, EventArgs e)
        => ExecuteCommandSafe(new AddPositionables<Vector3>(_universe, [new Entity {Name = "Object", Position = new(0, 0, 0)}]));

    private void buttonRemove_Click(object sender, EventArgs e) => Delete();

    private void listBox_SelectedIndexChanged(object sender, EventArgs e) => UpdatePropertyGrid();

    private void propertyGrid_PropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
    {
        OnChange();
        renderPanel.Engine?.Render();
    }
}
