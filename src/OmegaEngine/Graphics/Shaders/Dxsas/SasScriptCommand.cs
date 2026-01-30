/*
 * Copyright 2006-2014 Bastian Eicher
 * Copyright 2004 NVIDIA Corporation
 * This software contains source code provided by NVIDIA Corporation.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using NanoByte.Common;
using SlimDX;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics.Shaders.Dxsas;

public class SasScriptCommand(SasScriptCommand.CommandType type, Effect effect)
{
    #region Enumerations
    public enum CommandType
    {
        LoopByType,
        LoopByCount,
        LoopGetCount,
        LoopGetIndex,
        LoopUpdate,
        LoopEnd,
        RenderColorTarget,
        RenderDepthStencilTarget,
        Technique,
        Pass,
        Draw,
        ScriptSignature,
        ScriptExternal,
        Clear,
        ClearSetColor,
        ClearSetDepth,
        ClearSetStencil,
        GeometryList,
        Hint
    };
    #endregion

    #region Variables
    public int Index;
    public bool IsDirty;
    protected readonly Effect Effect = effect;

    public List<string> Options;
    #endregion

    #region Properties
    public CommandType Command { get; } = type;

    public string? Selector { get; set; }
    #endregion

    #region Access
    public virtual bool Setup()
    {
        return true;
    }

    public virtual bool Update()
    {
        return true;
    }

    protected string? EvaluateChoice()
    {
        int index = -1;
        return EvaluateChoice(ref index);
    }

    protected string? EvaluateChoice(ref int index)
    {
        string ret = null;
        if (string.IsNullOrEmpty(Selector))
            ret = Options[0];
        else
        {
            EffectHandle handle = Effect.GetParameter(null, Selector);
            if (handle == null)
            {
                // bad - maybe they wanted a UI string, but this is probably not valid in the current spec.
                // (selector should always be a valid parameter).
                // We'll just return the first option.
                ret = Options[0];
            }
            else
            {
                ParameterDescription desc = Effect.GetParameterDescription(handle);
                switch (desc.Type)
                {
                    case ParameterType.Bool:
                    {
                        var bValue = Effect.GetValue<bool>(handle);
                        if (Options.Count != 2)
                        {
                            Log.Error("SAS: bool selector must have 2 arguments");
                            ret = "";
                        }
                        else
                            ret = (bValue ? Options[0] : Options[1]);
                    }
                        break;
                    case ParameterType.Int:
                    {
                        var iValue = Effect.GetValue<int>(handle);
                        if (Options.Count <= iValue)
                        {
                            Log.Error("SAS: integer selector too large");
                            ret = "";
                        }
                        else
                            ret = Options[iValue];
                    }
                        break;
                    case ParameterType.Float:
                    {
                        var iValue = (int)Effect.GetValue<float>(handle);
                        if (Options.Count <= iValue)
                            Log.Error("SAS: float selector too large");
                        ret = Options[iValue];
                    }
                        break;
                    default:
                    {
                        Log.Error("SAS: Invalid choice selector");
                    }
                        break;
                }
            }
        }
        if (ret != null)
        {
            // Check for end index e.g. clear=color0, clear=color1, etc.
            if (index != -1)
            {
                var digitFind = new Regex(@"[0123456789]");
                Match match = digitFind.Match(ret);
                if (match.Success)
                {
                    index = Int32.Parse(ret.Substring(match.Index, ret.Length - match.Index), CultureInfo.InvariantCulture);
                    ret = ret[..match.Index];
                }
                else
                    index = 0;
            }
            return ret;
        }

        return null;
    }
    #endregion
}

#region Derived classes

#region Loop by type
public class SasScriptLoopByType(Effect effect) : SasScriptCommand(CommandType.LoopByType, effect);
#endregion

#region Loop by count
public class SasScriptLoopByCount(Effect effect) : SasScriptCommand(CommandType.LoopByCount, effect)
{
    public int Count { get; private set; }

    #region Update
    public override bool Update()
    {
        if (IsDirty)
        {
            string selection = EvaluateChoice();
            Count = 0;

            if (!string.IsNullOrEmpty(selection))
            {
                EffectHandle paramHandle = Effect.GetParameter(null, selection);

                if (paramHandle == null)
                    Log.Error("SAS: Couldn't get loopbycount parameter");
                else
                    Count = SasHelper.GetIntegerFromParam(Effect, paramHandle);
            }

            IsDirty = false;
        }
        return true;
    }
    #endregion
}
#endregion

#region Loop get count
public class SasScriptLoopGetCount(Effect effect) : SasScriptCommand(CommandType.LoopGetCount, effect)
{
    public EffectHandle DestinationParameter { get; private set; }

    #region Update
    public override bool Update()
    {
        if (IsDirty)
        {
            string selection = EvaluateChoice();

            if (!string.IsNullOrEmpty(selection))
            {
                DestinationParameter = Effect.GetParameter(null, selection);
                if (DestinationParameter == null)
                    Log.Error("SAS: Couldn't get loopgetcount parameter");
            }

            IsDirty = false;
        }
        return true;
    }
    #endregion
}
#endregion

#region Loop get index
public class SasScriptLoopGetIndex(Effect effect) : SasScriptCommand(CommandType.LoopGetIndex, effect)
{
    public EffectHandle DestinationParameter { get; private set; }

    #region Update
    public override bool Update()
    {
        if (IsDirty)
        {
            string selection = EvaluateChoice();
            if (!string.IsNullOrEmpty(selection))
            {
                DestinationParameter = Effect.GetParameter(null, selection);
                if (DestinationParameter == null)
                    Log.Error("SAS: Couldn't get loopgetindex parameter");
            }

            IsDirty = false;
        }
        return true;
    }
    #endregion
}
#endregion

#region Loop update
public class SasScriptLoopUpdate(Effect effect) : SasScriptCommand(CommandType.LoopUpdate, effect);
#endregion

#region Loop end
public class SasScriptLoopEnd(Effect effect) : SasScriptCommand(CommandType.LoopEnd, effect);
#endregion

//--------------------//

#region Render color target
public class SasScriptRenderColorTarget(Effect effect) : SasScriptCommand(CommandType.RenderColorTarget, effect)
{
    public EffectHandle? TextureHandle { get; private set; }

    #region Update
    public override bool Update()
    {
        if (IsDirty)
        {
            string selection = EvaluateChoice();
            if (string.IsNullOrEmpty(selection))
                TextureHandle = null;
            else
            {
                EffectHandle paramHandle = Effect.GetParameter(null, selection);
                if (paramHandle == null)
                {
                    Log.Error("SAS: Couldn't get color target parameter");
                    TextureHandle = null;
                }
                else
                    TextureHandle = paramHandle;
            }

            IsDirty = false;
        }
        return true;
    }
    #endregion
}
#endregion

#region Render DepthStencil target
public class SasScriptRenderDepthStencilTarget(Effect effect) : SasScriptCommand(CommandType.RenderDepthStencilTarget, effect)
{
    public EffectHandle? TextureHandle { get; private set; }

    #region Update
    public override bool Update()
    {
        if (IsDirty)
        {
            string selection = EvaluateChoice();
            if (string.IsNullOrEmpty(selection))
                TextureHandle = null;
            else
            {
                EffectHandle paramHandle = Effect.GetParameter(null, selection);

                if (paramHandle == null)
                {
                    Log.Error("SAS: Couldn't get depth target parameter");
                    TextureHandle = null;
                }
                else
                    TextureHandle = paramHandle;
            }

            IsDirty = false;
        }
        return true;
    }
    #endregion
}
#endregion

//--------------------//

#region Technique
public class SasScriptTechnique(Effect effect) : SasScriptCommand(CommandType.Technique, effect)
{
    public EffectHandle? Handle { get; private set; }

    #region Update
    public override bool Update()
    {
        if (IsDirty)
        {
            string selection = EvaluateChoice();
            Handle = null;
            for (int tech = 0; tech < Effect.Description.Techniques; tech++)
            {
                EffectHandle thisTechHandle = Effect.GetTechnique(tech);
                if (StringUtils.EqualsIgnoreCase(Effect.GetTechniqueDescription(thisTechHandle).Name, selection))
                {
                    Handle = thisTechHandle;
                    break;
                }
            }
            if (Handle == null)
                Log.Error("SAS: Couldn't find technique");
            IsDirty = false;
        }
        return true;
    }
    #endregion
}
#endregion

#region Pass
public class SasScriptPass(Effect effect) : SasScriptCommand(CommandType.Pass, effect)
{
    public int PassNum { get; private set; }

    #region Update
    public override bool Update()
    {
        if (true /*isDirty*/)
        {
            string selection = EvaluateChoice();
            PassNum = 0;
            int pass;
            for (pass = 0; pass < Effect.GetTechniqueDescription(Effect.Technique).Passes; pass++)
            {
                EffectHandle thisPassHandle = Effect.GetPass(Effect.Technique, pass);
                if (StringUtils.EqualsIgnoreCase(Effect.GetPassDescription(thisPassHandle).Name, selection))
                {
                    PassNum = pass;
                    break;
                }
            }
            if (pass == Effect.GetTechniqueDescription(Effect.Technique).Passes)
                Log.Error("SAS: Couldn't find pass!");

            IsDirty = false;
        }
        return true;
    }
    #endregion
}
#endregion

#region Draw
public class SasScriptDraw(Effect effect) : SasScriptCommand(CommandType.Draw, effect)
{
    public enum DrawType
    {
        Scene,
        Geometry,
        Buffer,
        Hint
    }

    public DrawType DrawOption { get; private set; }

    #region Update
    public override bool Update()
    {
        if (IsDirty)
        {
            string selection = EvaluateChoice();
            if (StringUtils.EqualsIgnoreCase(selection, "scene")) DrawOption = DrawType.Scene;
            else if (StringUtils.EqualsIgnoreCase(selection, "geometry")) DrawOption = DrawType.Geometry;
            else DrawOption = StringUtils.EqualsIgnoreCase(selection, "buffer") ? DrawType.Buffer : DrawType.Hint;
            IsDirty = false;
        }

        return true;
    }
    #endregion
}
#endregion

//--------------------//

#region Script signature
public class SasScriptScriptSignature(Effect effect) : SasScriptCommand(CommandType.ScriptSignature, effect);
#endregion

#region Script external
public class SasScriptScriptExternal(Effect effect) : SasScriptCommand(CommandType.ScriptExternal, effect);
#endregion

#region Script command
public class SasScriptClear(Effect effect) : SasScriptCommand(CommandType.Clear, effect)
{
    public enum ClearType
    {
        Color,
        Depth,
        Stencil
    }

    public ClearType ClearOption { get; private set; } = ClearType.Color;

    #region Update
    public override bool Update()
    {
        if (IsDirty)
        {
            int index = 0;
            string selection = EvaluateChoice(ref index);
            if (StringUtils.EqualsIgnoreCase(selection, "color"))
                ClearOption = ClearType.Color;
            else if (StringUtils.EqualsIgnoreCase(selection, "depth"))
                ClearOption = ClearType.Depth;
            else if (StringUtils.EqualsIgnoreCase(selection, "stencil"))
                ClearOption = ClearType.Stencil;
            else
                Log.Error("SAS: unrecognized clear command");
            IsDirty = false;
        }
        return true;
    }
    #endregion
}
#endregion

//--------------------//

#region Clear set color
public class SasScriptClearSetColor(Effect effect) : SasScriptCommand(CommandType.ClearSetColor, effect)
{
    public Vector4 Color { get; private set; }

    #region Update
    public override bool Update()
    {
        if (IsDirty)
        {
            string selection = EvaluateChoice();
            EffectHandle paramHandle = Effect.GetParameter(null, selection);

            if (paramHandle != null)
                Color = SasHelper.GetVector4FromParam(Effect, paramHandle);
            else
                Log.Error("SAS: Couldn't get clear color parameter");
            IsDirty = false;
        }
        return true;
    }
    #endregion
}
#endregion

#region Clear set depth
public class SasScriptClearSetDepth(Effect effect) : SasScriptCommand(CommandType.ClearSetDepth, effect)
{
    public float Depth { get; private set; }

    #region Update
    public override bool Update()
    {
        if (IsDirty)
        {
            string selection = EvaluateChoice();
            EffectHandle paramHandle = Effect.GetParameter(null, selection);
            if (paramHandle != null)
                Depth = SasHelper.GetFloatFromParam(Effect, paramHandle);
            else
                Log.Error("SAS: Couldn't get clear depth parameter");
            IsDirty = false;
        }
        return true;
    }
    #endregion
}
#endregion

#region Clear set stencil
public class SasScriptClearSetStencil(Effect effect) : SasScriptCommand(CommandType.ClearSetStencil, effect)
{
    public int Stencil { get; private set; }

    #region Update
    public override bool Update()
    {
        if (IsDirty)
        {
            string selection = EvaluateChoice();
            EffectHandle paramHandle = Effect.GetParameter(null, selection);
            if (paramHandle != null)
                Stencil = SasHelper.GetIntegerFromParam(Effect, paramHandle);
            else
                Log.Error("SAS: Couldn't get clear stencil parameter");
            IsDirty = false;
        }
        return true;
    }
    #endregion
}
#endregion

//--------------------//

#region Geometry list
public class SasScriptGeometryList(Effect effect) : SasScriptCommand(CommandType.GeometryList, effect);
#endregion

#region Hint
public class SasScriptHint(Effect effect) : SasScriptCommand(CommandType.Hint, effect);
#endregion

#endregion
