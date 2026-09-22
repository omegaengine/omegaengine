using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NanoByte.Common.Dispatch;
using OmegaEngine.Graphics;
using OmegaEngine.Graphics.Renderables;
using SlimDX;

namespace AlphaFramework.Presentation;

/// <summary>
/// An <see cref="ICollection{T}"/> facade over the root renderables of a <see cref="Scene"/> that groups everything belonging to one owner under a shared transform.
/// </summary>
/// <param name="roots">The scene's root renderables, usually <see cref="Scene.Positionables"/>.</param>
/// <remarks>
/// <para><see cref="ModelViewSync{TModel,TView}"/> knows nothing about render hierarchies; it only adds and removes flat representations.
/// Create callbacks therefore call <see cref="AddTo{T}"/> to put a renderable into an owner's group. The subsequent <see cref="Add"/> then
/// leaves it where it is, while <see cref="Remove"/> works for either case.</para>
/// <para>A group only gets a <see cref="Pivot"/> of its own once more than one thing needs to share the owner's transform. While a group holds
/// a single renderable, that renderable carries the transform itself and its own local transform is folded into <see cref="PositionableRenderable.PreTransform"/>.
/// Adding a second renderable or attaching a light or sound promotes the group to a pivot, which takes over the owner's position, rotation and scale; it is never demoted again.</para>
/// <para>Renderables added to a group remain owned by the caller: releasing a group only disposes the <see cref="Pivot"/> itself.</para>
/// </remarks>
public sealed class PivotedRenderables(ICollection<PositionableRenderable> roots) : ICollection<PositionableRenderable>
{
    /// <summary>
    /// Everything belonging to one owner.
    /// </summary>
    private sealed class Group(object owner, string? name)
    {
        public readonly object Owner = owner;
        public readonly string? Name = name;
        public readonly List<PositionableRenderable> Members = [];

        /// <summary>The shared transform node; <c>null</c> while a single member carries the transform itself.</summary>
        public Pivot? Pivot;

        /// <summary>Keeps the <see cref="Pivot"/> in place even while no renderable is in the group, because something else is attached to it.</summary>
        public bool Pinned;

        /// <summary>The node that carries the owner's transform.</summary>
        public PositionableRenderable? Anchor => Pivot ?? (Members.Count == 0 ? null : Members[0]);
    }

    private readonly Dictionary<object, Group> _groups = [];
    private readonly Dictionary<PositionableRenderable, Group> _groupOf = [];
    private readonly Dictionary<PositionableRenderable, PositionableRenderable> _parents = [];

    /// <summary>
    /// Adds a renderable to an owner's group, so that it follows the owner's transform.
    /// </summary>
    /// <param name="renderable">The renderable, with its placement relative to the owner already applied to <see cref="PositionableRenderable.Position"/> and friends.</param>
    /// <param name="owner">An object identifying the group, usually an element of the game world.</param>
    /// <param name="name">The <see cref="Renderable.Name"/> to give a <see cref="Pivot"/> created for this group.</param>
    /// <returns><paramref name="renderable"/>, so that this can be used inline in create callbacks.</returns>
    public T AddTo<T>(T renderable, object owner, string? name = null)
        where T : PositionableRenderable
    {
        var group = GetGroup(owner, name);

        if (group.Pivot == null)
        {
            if (group.Members.Count == 0)
            {
                // A lone renderable carries the owner's transform itself; a pivot above it would be an extra node for nothing
                Collapse(renderable);
                roots.Add(renderable);
                Track(group, renderable);
                return renderable;
            }

            Promote(group);
        }

        group.Pivot!.Children.Add(renderable);
        Track(group, renderable);
        return renderable;
    }

    /// <summary>
    /// Gets the <see cref="Pivot"/> of an owner's group, creating it if there is none yet.
    /// </summary>
    /// <param name="owner">An object identifying the group, usually an element of the game world.</param>
    /// <param name="name">The <see cref="Renderable.Name"/> to give a newly created pivot.</param>
    /// <remarks>
    /// Use this for objects that are not renderables themselves but resolve their position through one, i.e. lights and sounds.
    /// It permanently keeps the group on a pivot, because a collapsed member's transform carries its own geometry's placement, which must not leak into such attachments.
    /// </remarks>
    public Pivot PivotFor(object owner, string? name = null)
    {
        var group = GetGroup(owner, name);
        group.Pinned = true;
        return group.Pivot ?? Promote(group);
    }

    /// <summary>
    /// The node that carries an owner's transform; <c>null</c> if nothing belongs to that owner.
    /// </summary>
    /// <remarks>This is either the group's <see cref="Pivot"/> or, for a group holding a single renderable, that renderable.</remarks>
    public PositionableRenderable? AnchorFor(object owner)
        => _groups.TryGetValue(owner, out var group) ? group.Anchor : null;

    /// <summary>
    /// All renderables that belong to some owner's group.
    /// </summary>
    /// <remarks>Excludes the <see cref="Pivot"/>s themselves, which have no geometry.</remarks>
    public IEnumerable<PositionableRenderable> GroupedRenderables => _groupOf.Keys;

    /// <summary>
    /// The transform that places a grouped renderable's geometry in its owner's coordinate system.
    /// </summary>
    /// <remarks>Reading <see cref="PositionableRenderable.Position"/> directly is not enough, because a collapsed member uses it for the owner's world position.</remarks>
    public Matrix LocalTransformOf(PositionableRenderable renderable)
        => _groupOf.TryGetValue(renderable, out var group) && group.Pivot == null
            ? renderable.PreTransform
            : LocalMatrixOf(renderable);

    /// <summary>
    /// Drops an owner's group, disposing a <see cref="Pivot"/> it may have created.
    /// </summary>
    /// <remarks>Groups that hold no renderables are dropped automatically; this is only needed for ones kept alive by <see cref="PivotFor"/>.</remarks>
    public void Release(object owner)
    {
        if (_groups.TryGetValue(owner, out var group)) Release(group);
    }

    /// <summary>
    /// Drops all groups, disposing the <see cref="Pivot"/>s they created.
    /// </summary>
    public void ReleaseAll()
    {
        foreach (var group in _groups.Values.ToList()) Release(group);
    }

    /// <summary>
    /// Places a renderable in the local coordinate system of <paramref name="parent"/> instead of at the scene root.
    /// </summary>
    /// <returns><paramref name="renderable"/>, so that this can be used inline in create callbacks.</returns>
    /// <remarks>Unlike <see cref="AddTo{T}"/> this names the parent directly, for hierarchies that are not built around an owner.</remarks>
    public T PlaceUnder<T>(T renderable, PositionableRenderable parent)
        where T : PositionableRenderable
    {
        parent.Children.Add(renderable);
        _parents[renderable] = parent;
        return renderable;
    }

    /// <summary>
    /// Moves all renderables currently placed underneath <paramref name="from"/> over to <paramref name="to"/>.
    /// </summary>
    /// <remarks>Use this when a parent renderable is replaced by a freshly built one.</remarks>
    public void MovePlacements(PositionableRenderable from, PositionableRenderable to)
    {
        foreach (var renderable in _parents.Where(x => x.Value == from).Select(x => x.Key).ToList())
        {
            to.Children.Add(renderable);
            _parents[renderable] = to;
        }
    }

    private Group GetGroup(object owner, string? name)
    {
        if (!_groups.TryGetValue(owner, out var group))
            _groups.Add(owner, group = new(owner, name));
        return group;
    }

    private void Track(Group group, PositionableRenderable renderable)
    {
        group.Members.Add(renderable);
        _groupOf.Add(renderable, group);
    }

    /// <summary>
    /// Folds a renderable's local transform into <see cref="PositionableRenderable.PreTransform"/>,
    /// freeing its position, rotation and scale to carry its owner's transform instead.
    /// </summary>
    private static void Collapse(PositionableRenderable renderable)
    {
        renderable.PreTransform = LocalMatrixOf(renderable);
        ResetTransform(renderable);
    }

    private static Matrix LocalMatrixOf(PositionableRenderable renderable)
        => renderable.PreTransform * Matrix.Scaling(renderable.Scale) * Matrix.RotationQuaternion(renderable.Rotation) * Matrix.Translation((Vector3)renderable.Position);

    private static void ResetTransform(PositionableRenderable renderable)
    {
        renderable.Scale = new(1, 1, 1);
        renderable.Rotation = Quaternion.Identity;
        renderable.Position = default;
    }

    /// <summary>
    /// Gives a group a <see cref="Pivot"/> of its own, moving a previously collapsed member underneath it.
    /// </summary>
    private Pivot Promote(Group group)
    {
        var pivot = new Pivot {Name = group.Name};

        if (group.Members.Count == 1)
        {
            // Hand the owner's transform over to the pivot. The member's own placement stays folded into its PreTransform,
            // so changes made to it since it was collapsed are kept.
            var collapsed = group.Members[0];
            pivot.Position = collapsed.Position;
            pivot.Rotation = collapsed.Rotation;
            pivot.Scale = collapsed.Scale;
            ResetTransform(collapsed);
            pivot.Children.Add(collapsed); // Reparenting also takes it out of the scene root
        }

        roots.Add(pivot);
        group.Pivot = pivot;
        return pivot;
    }

    private void Release(Group group)
    {
        _groups.Remove(group.Owner);

        foreach (var member in group.Members)
        {
            _groupOf.Remove(member);
            if (group.Pivot == null) roots.Remove(member);
        }
        group.Members.Clear();

        if (group.Pivot is {} pivot)
        {
            // Detach whatever is still below the pivot, so that disposing it does not dispose renderables owned by someone else
            pivot.Children.Clear();

            roots.Remove(pivot);
            pivot.Dispose();
        }
    }

    /// <inheritdoc/>
    public void Add(PositionableRenderable item)
    {
        // Already placed in the hierarchy by AddTo or PlaceUnder
        if (_groupOf.ContainsKey(item) || _parents.ContainsKey(item)) return;

        roots.Add(item);
    }

    /// <inheritdoc/>
    public bool Remove(PositionableRenderable item)
    {
        if (_groupOf.TryGetValue(item, out var group))
        {
            _groupOf.Remove(item);
            group.Members.Remove(item);

            bool removed = group.Pivot is {} pivot ? pivot.Children.Remove(item) : roots.Remove(item);
            if (group.Members.Count == 0 && !group.Pinned) Release(group);
            return removed;
        }

        if (_parents.TryGetValue(item, out var parent))
        {
            _parents.Remove(item);
            return parent.Children.Remove(item);
        }

        return roots.Remove(item);
    }

    /// <inheritdoc/>
    public void Clear()
    {
        ReleaseAll();
        foreach (var renderable in _parents.Keys.ToList()) Remove(renderable);
        roots.Clear();
    }

    /// <inheritdoc/>
    public bool Contains(PositionableRenderable item)
        => _groupOf.ContainsKey(item) || _parents.ContainsKey(item) || roots.Contains(item);

    /// <inheritdoc/>
    public void CopyTo(PositionableRenderable[] array, int arrayIndex)
    {
        foreach (var item in this) array[arrayIndex++] = item;
    }

    /// <inheritdoc/>
    public int Count => roots.Count + _groups.Values.Where(x => x.Pivot != null).Sum(x => x.Members.Count) + _parents.Count;

    /// <inheritdoc/>
    public bool IsReadOnly => false;

    /// <inheritdoc/>
    /// <remarks>The scene roots already cover collapsed members and pivots, so only what lives below them is added.</remarks>
    public IEnumerator<PositionableRenderable> GetEnumerator()
        => roots.Concat(_groups.Values.Where(x => x.Pivot != null).SelectMany(x => x.Members))
                .Concat(_parents.Keys)
                .GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
