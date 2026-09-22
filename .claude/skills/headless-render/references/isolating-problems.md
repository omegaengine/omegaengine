# Isolating a rendering problem

A rendered image tells you that something is wrong, not where. These are the techniques that
narrow it down, roughly in the order worth trying. The common thread is that each one removes a
stage from the picture, so a difference either survives or disappears and you learn something
either way.

## 1. Vary the code path before varying the code

Most rendering features have several implementations selected at runtime, and a bug usually lives
in one of them. Capturing the same camera across those variants costs one command each and often
localizes the problem immediately.

- `--water-effects None | RefractionOnly | ReflectTerrain | ReflectAll` — the water techniques
  differ substantially: `None` samples only a scrolling surface texture, `RefractionOnly` adds one
  scene render target, `ReflectAll` adds a second. If only the last is wrong, the reflection path
  is implicated and everything shared by all three is not.
- `--no-post` — separates the scene pass from the post-screen chain. If the image is only wrong
  with post effects on, the scene pass is fine.
- `--phase 0,1,2,3` — a lighting problem that appears only at noon is about direct light; one that
  appears at midnight too is about ambient or the material.

## 2. Dump the intermediate render targets

`--dump-render-targets` writes each child view's texture next to the screenshot. Child views are
where water reflection and refraction, glow and depth are produced, so this splits "the
intermediate is wrong" from "the shader consuming it is wrong".

Compare the dumps across the before/after builds. If an intermediate is byte-identical and the
final image is not, the production side is correct and the bug is in how the main pass consumes it.
That is a strong, cheap result — it eliminates an entire half of the pipeline.

## 3. Render a shader's internal value as color

When the consuming shader is implicated, make it output the quantity you are suspicious about
instead of its real result:

```hlsl
// was: return DullBlendFactor*DullColor + (1-DullBlendFactor)*combinedColor;
return float4(fresnelTerm, fresnelTerm, fresnelTerm, 1);
```

Then rebuild and capture (see below). Reading the gray level tells you the value's actual range,
which settles questions like "is this term ever negative" far faster than reasoning about it.

Remember the output still goes through the sRGB encode on the way to the back buffer, so a
displayed 0.73 is a linear 0.49. Decode before interpreting the number.

The same trick proves a round-trip: make the shader return one sampled texture unchanged and
capture it on both builds. If those two images are identical, the sampling and encoding of that
texture are provably correct and you can stop suspecting them.

## 4. Running throwaway shader experiments

The compiled `.fxo` files are tracked in git, so experiments need care not to leave a dirty tree.

```powershell
# 1. Edit shaders\<Name>.fx
# 2. Recompile (build.ps1 knows which files need the legacy compiler)
& .\shaders\build.ps1
# 3. Rebuild the engine so the new .fxo is copied to artifacts
dotnet build .\src\OmegaEngine\OmegaEngine.csproj -c Debug -v q
# 4. Capture
# 5. Restore BOTH the source and the binaries
git checkout -- shaders src/OmegaEngine/Shaders
```

Step 5 matters more than it looks. The legacy D3DX compiler does not produce byte-identical output
across runs, so even recompiling an unmodified `.fx` leaves the `.fxo` showing as modified. Always
restore from git rather than rebuilding to "undo", and check `git status` afterwards.

Wrap experiments in a script with a `try`/`finally` that restores the shader, so an exception
midway does not strand the tree in a patched state.

## 5. Only then, tune

Once you know which computation changed the image, separate "this is a bug" from "this is correct
behavior that changed the look". They need different responses, and the second one is not yours
to decide alone.

Before proposing a tuning value, test it and look at the result. Intuition about color math is
unreliable — raising a blend factor to bring back a tint can easily make things worse in a way
that is obvious in an image and invisible in the arithmetic. Render the candidate values, show
them, and let the person whose project it is pick.
