---
name: headless-render
description: Render Frame of Reference maps to image files without a human at the keyboard, so rendering changes can actually be looked at. Use this whenever a change touches shaders, lighting, color handling, textures, materials, post-screen effects, water, terrain or the skybox and you want to see the result - and especially for before/after comparison against another commit, for reproducing a reported visual regression, or for dumping intermediate render targets to find out which stage of the pipeline a problem lives in. Reach for this instead of asking the user to run the game and describe what they see.
---

# Headless rendering for visual verification

Unit tests can prove a render state reaches the device. They cannot tell you whether the image
looks right. This skill drives the engine from a throwaway host process so you can render a known
map from a known camera, save it as a JPG, and look at it yourself — including against a build of
an older commit, which is what turns "this looks a bit off" into a specific, localized finding.

## Quick start

```powershell
.\.claude\skills\headless-render\scripts\capture.ps1 -Out <dir> -HarnessArgs '--map','Mountains','--phase','1'
```

That builds the solution, builds the harness against it, renders, and writes
`<dir>\Mountains-phase1.jpg`. Add `-NoBuild` when the engine is already up to date — the harness
itself is always rebuilt, which takes about a second.

Anything after `-HarnessArgs` goes straight to the harness. Passing an unrecognized option prints
the full list (it also lives at the bottom of `harness\Program.cs`). The ones that matter most:

| Option | Why you would use it |
|---|---|
| `--map <name>` | `Mountains`, `Tutorial`, `Benchmark`, `Menu`. |
| `--phase 0,1,2,3` | Dawn / noon / dusk / midnight in one run. Lighting bugs often show at only one. |
| `--water` | Aim at the map's largest water plane instead of the terrain center. |
| `--target x,y,z` | Explicit camera target, when you need a specific spot. |
| `--yaw` `--pitch` `--radius` | Framing. Defaults look at terrain from a middle distance. |
| `--water-effects ReflectAll` | Force a water quality level; the paths differ a lot, so test the one that is broken. |
| `--no-post` | Disable post-screen effects. Essential for checking that the scene pass is right on its own. |
| `--seed <n>` | Seed the random numbers, so particle systems (fire, smoke, ...) come out identical across runs and builds. Use it whenever a comparison includes particles. |
| `--dump-render-targets` | Also write each child view's texture (reflection, refraction, glow) as PNG. |

## Before/after against another commit

This is the main reason to use the skill, and the part worth getting right. Build the other commit
in a worktree and render it with the same harness and the same camera:

```powershell
git worktree add --detach <scratch>\baseline <commit-ish>
.\.claude\skills\headless-render\scripts\capture.ps1 -Repo <scratch>\baseline -Out <scratch>\before -HarnessArgs '--map','Mountains','--phase','1'
.\.claude\skills\headless-render\scripts\capture.ps1 -Out <scratch>\after  -HarnessArgs '--map','Mountains','--phase','1'
git worktree remove --force <scratch>\baseline
```

Pick the baseline commit so it isolates *your* change — usually `<your-commit>^` rather than an
older tag, otherwise unrelated work lands in the diff and you will chase the wrong thing.

The script copies the same harness build into each tree's `artifacts` directory and points
`OMEGAENGINE_CONTENT` at that tree's `content`, so both sides differ only by the engine code and
content you are actually comparing. Read the two images and describe the difference concretely
(brighter, flatter, more saturated, detail lost in shadow) rather than just "changed".

## Things that will bite you

These are all silent or confusing failures rather than clean errors, which is why the harness
handles them for you. If you write your own host, you need all of them.

**The window must be shown.** `Engine.Render` returns immediately while `Target.Visible` is false,
so you get a black or stale image and no error. The harness shows a borderless form parked at
-32000,-32000 — off-screen but visible as far as the engine is concerned.

**The executable must sit next to the engine DLLs.** `Engine` derives `ShaderDir` from
`Locations.InstallBase`, which is the entry assembly's directory, so a harness run from its own
`bin` folder cannot find `Shaders\*.fxo`. The script copies the exe into `artifacts\<Config>\net472`
and runs it from there.

**`XmlStorageConfig.Apply()` before loading any map.** The maps store `Vector2`/`Vector3` members
as XML attributes, and the plain serializer expects elements. Without the overrides every position
and size silently deserializes to zero: the map loads, the terrain renders, and every entity and
water plane sits at the origin. Nothing errors — you just quietly photograph the wrong scene.

**`EntityTemplate.LoadAll()` and `TerrainTemplate.LoadAll()` before loading any map.** Deserialization
resolves template names as it goes and throws `'EntityTemplates.xml' has not been loaded yet`
wrapped in an `InvalidDataException` about XML, which does not point at the real cause.

**Use a camera that does not move.** `MenuPresenter` and friends animate off a `Stopwatch`, so two
runs frame differently and the comparison is worthless. The harness uses its own `Presenter`
subclass with a fixed `ArcballCamera`. Particle systems are the same problem in another guise: they
spawn from an unseeded `Random`, so pass `--seed` or two runs of the *same* build already differ.

**Render several frames before capturing.** Terrain, shaders and textures load lazily over the
first frames. The harness renders 12 by default (`--warmup`).

**Dispose everything.** `Engine.OnDispose` forces `GC.Collect()` + `GC.WaitForPendingFinalizers()`,
and an undisposed `EngineElement` throws from its finalizer in Debug builds. That exception comes
from the finalizer thread, so it terminates the process with no test output and a message that
points at `EngineElement.Finalize` rather than at whatever leaked. If a headless run dies that way,
suspect an object whose constructor threw, not the object named in the message.

**`View` is ambiguous** between `OmegaEngine.Graphics.View` and `System.Windows.Forms.View` in any
file that uses both namespaces. Alias it.

**Coordinates:** map X = engine +X, map Y = engine **-Z**, map height = engine +Y. Positionables
expose `EnginePosition` which has already done this conversion — prefer it over converting by hand.

## Finding out where a rendering problem lives

When an image looks wrong, resist guessing at causes. See
[references/isolating-problems.md](references/isolating-problems.md) for the techniques that turn a
vague "washed out" into a specific stage: dumping intermediate render targets, rendering a shader's
internal values as color, and bisecting a change across the affected code paths. That file also
covers running throwaway shader experiments without leaving a dirty working tree.

## Cleaning up

The script removes the harness executable from `artifacts` when it finishes, and `harness\bin\` is
already git-ignored. Remove any worktree you created (`git worktree remove --force`, then
`git worktree prune`) — worktree registrations live in the repo's `.git` directory, not in your
scratch space, so leaving them behind clutters the repository for everyone.
