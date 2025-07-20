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

namespace OmegaEngine.Graphics.Shaders
{
    public class SasScriptCommand
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
        protected readonly Effect Effect;

        public List<string> Options;
        #endregion

        #region Properties
        public CommandType Command { get; }

        public string Selector { get; set; }
        #endregion

        #region Constructor
        public SasScriptCommand(CommandType type, Effect effect)
        {
            Command = type;
            Effect = effect;
        }
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

        protected string EvaluateChoice()
        {
            int index = -1;
            return EvaluateChoice(ref index);
        }

        protected string EvaluateChoice(ref int index)
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
                        ret = ret.Substring(0, match.Index);
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
    public class SasScriptLoopByType : SasScriptCommand
    {
        public SasScriptLoopByType(Effect effect) : base(CommandType.LoopByType, effect)
        {}
    }
    #endregion

    #region Loop by count
    public class SasScriptLoopByCount : SasScriptCommand
    {
        public int Count { get; private set; }

        public SasScriptLoopByCount(Effect effect) : base(CommandType.LoopByCount, effect)
        {}

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
    public class SasScriptLoopGetCount : SasScriptCommand
    {
        public EffectHandle DestinationParameter { get; private set; }

        public SasScriptLoopGetCount(Effect effect) : base(CommandType.LoopGetCount, effect)
        {}

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
    public class SasScriptLoopGetIndex : SasScriptCommand
    {
        public EffectHandle DestinationParameter { get; private set; }

        public SasScriptLoopGetIndex(Effect effect) : base(CommandType.LoopGetIndex, effect)
        {}

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
    public class SasScriptLoopUpdate : SasScriptCommand
    {
        public SasScriptLoopUpdate(Effect effect) : base(CommandType.LoopUpdate, effect)
        {}
    }
    #endregion

    #region Loop end
    public class SasScriptLoopEnd : SasScriptCommand
    {
        public SasScriptLoopEnd(Effect effect) : base(CommandType.LoopEnd, effect)
        {}
    }
    #endregion

    //--------------------//

    #region Render color target
    public class SasScriptRenderColorTarget : SasScriptCommand
    {
        public EffectHandle TextureHandle { get; private set; }

        public SasScriptRenderColorTarget(Effect effect) : base(CommandType.RenderColorTarget, effect)
        {}

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
    public class SasScriptRenderDepthStencilTarget : SasScriptCommand
    {
        public EffectHandle TextureHandle { get; private set; }

        public SasScriptRenderDepthStencilTarget(Effect effect) : base(CommandType.RenderDepthStencilTarget, effect)
        {}

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
    public class SasScriptTechnique : SasScriptCommand
    {
        public EffectHandle Handle { get; private set; }

        public SasScriptTechnique(Effect effect) : base(CommandType.Technique, effect)
        {}

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
    public class SasScriptPass : SasScriptCommand
    {
        public int PassNum { get; private set; }

        public SasScriptPass(Effect effect) : base(CommandType.Pass, effect)
        {}

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
    public class SasScriptDraw : SasScriptCommand
    {
        public enum DrawType
        {
            Scene,
            Geometry,
            Buffer,
            Hint
        }

        public DrawType DrawOption { get; private set; }

        public SasScriptDraw(Effect effect) : base(CommandType.Draw, effect)
        {}

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
    public class SasScriptScriptSignature : SasScriptCommand
    {
        public SasScriptScriptSignature(Effect effect) : base(CommandType.ScriptSignature, effect)
        {}
    }
    #endregion

    #region Script external
    public class SasScriptScriptExternal : SasScriptCommand
    {
        public SasScriptScriptExternal(Effect effect) : base(CommandType.ScriptExternal, effect)
        {}
    }
    #endregion

    #region Script command
    public class SasScriptClear : SasScriptCommand
    {
        public enum ClearType
        {
            Color,
            Depth,
            Stencil
        }

        public ClearType ClearOption { get; private set; } = ClearType.Color;

        public SasScriptClear(Effect effect) : base(CommandType.Clear, effect)
        {}

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
    public class SasScriptClearSetColor : SasScriptCommand
    {
        public Vector4 Color { get; private set; }

        public SasScriptClearSetColor(Effect effect) : base(CommandType.ClearSetColor, effect)
        {}

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
    public class SasScriptClearSetDepth : SasScriptCommand
    {
        public float Depth { get; private set; }

        public SasScriptClearSetDepth(Effect effect) : base(CommandType.ClearSetDepth, effect)
        {}

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
    public class SasScriptClearSetStencil : SasScriptCommand
    {
        public int Stencil { get; private set; }

        public SasScriptClearSetStencil(Effect effect) : base(CommandType.ClearSetStencil, effect)
        {}

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
    public class SasScriptGeometryList : SasScriptCommand
    {
        public SasScriptGeometryList(Effect effect) : base(CommandType.GeometryList, effect)
        {}
    }
    #endregion

    #region Hint
    public class SasScriptHint : SasScriptCommand
    {
        public SasScriptHint(Effect effect) : base(CommandType.Hint, effect)
        {}
    }
    #endregion

    #endregion
}
