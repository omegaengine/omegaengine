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

/*
using SlimDX.Direct3D9;
using SlimDX;
using System.Collections;
using OmegaEngine.Assets;

namespace OmegaEngine.Graphics.Shaders
{
    /// <summary>
    /// Summary description for ScriptEngine.
    /// </summary>
    internal class SasScriptEngine
    {
        class InstructionPointer
        {
            public int materialStackPos = 0;
            public int currentInstruction = 0;
            public ArrayList commands = new ArrayList();
            public XMesh geometryMesh = null;
            public int geometryAttribute = 0;
            public int loopCount = 0;
            public int loopStartCount = 0;
            public int loopReturnInstruction = -1;
            public int passNum = 0;
        }

        private Vector4 clearColor = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
        private float clearDepth = 1.0f;
        private int clearStencil = 0;
        private bool isDrawObjects = true;

        public bool ModelDrawn { get { return isDrawObjects; } }

        ArrayList materialStack = new ArrayList();
        Stack callStack = new Stack();
        public bool Run(Scene scene, View renderer)
        {
            isDrawObjects = false;

            if (scene.GetMaterials().Count == 0)
                return true;

            renderer.PreScene();

            materialStack.Clear();
            foreach (XMaterial mat in scene.GetMaterials())
            {
                if (mat.MaterialEffect != null)
                {
                    materialStack.Add(mat);
                }
            }

            if (materialStack.Count == 0)
                return true;

            XMaterial firstMat = (XMaterial)materialStack[materialStack.Count - 1];
            Effect effect = firstMat.MaterialEffect;
            if (effect == null || effect.Disposed)
            {
                // fail
                return false;
            }

            // Reset to default
            renderer.ResetTargets();

            renderer.PreEffectRender(scene, firstMat);

            // Make sure we start with an empty stack
            callStack.Clear();

            InstructionPointer ip = new InstructionPointer();
            ip.materialStackPos = materialStack.Count - 1;
            ip.currentInstruction = 0;
            ip.commands = firstMat.EntryPoint;
            callStack.Push(ip);

            // Make the first call.
            Execute(scene, renderer);

            renderer.ResetTargets();

            renderer.PostScene();

            return true;
        }

        public unsafe bool Execute(Scene scene, View renderer)
        {
            InstructionPointer ip = ((InstructionPointer)callStack.Peek());
            XMaterial mat = (XMaterial)materialStack[ip.materialStackPos];

            Effect effect = mat.MaterialEffect;
            while (ip.currentInstruction != ip.commands.Count)
            {
                SasScriptCommand commandObject = (SasScriptCommand)ip.commands[ip.currentInstruction];
                ip.currentInstruction++;

                // Update if dirty.
                commandObject.Update();

                // If we entered a loop count of 0, skip all the instructions except the end.
                if (ip.loopReturnInstruction != -1 &&
                    ip.loopCount == 0 &&
                    commandObject.Command != SasScriptCommand.CommandType.LoopEnd)
                {
                    continue;
                }

                switch (commandObject.Command)
                {
                    case SasScriptCommand.CommandType.Clear:
                        {
                            SasScriptClear clear = (SasScriptClear)commandObject;
                            switch (clear.ClearOption)
                            {
                                case SasScriptClear.ClearType.Color:
                                    renderer.ClearColor(clearColor);
                                    break;
                                case SasScriptClear.ClearType.Depth:
                                    renderer.ClearDepth(clearDepth);
                                    break;
                                case SasScriptClear.ClearType.Stencil:
                                    renderer.ClearStencil(clearStencil);
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;

                    case SasScriptCommand.CommandType.RenderColorTarget:
                        {
                            SasScriptRenderColorTarget renderColorTarget = (SasScriptRenderColorTarget)commandObject;
                            renderer.SetColorTarget(renderColorTarget.Index, mat.GetTexture(renderColorTarget.TextureHandle));

                            // We may need to update the effect parameters if the rendertarget has changed
                            // since this may effect viewportpixelsize...
                            // Can optimize this, but for now, just blast through them...
                            for (int i = 0; i < effect.Description.Parameters; i++)
                            {
                                XMaterial.ParameterInfo info = mat.GetParameterInfo(i);
                                if (info.semanticID == XMaterial.SemanticID.viewportpixelsize &&
                                    (Effect.GetParameterDescription(info.handle).Columns == 2))
                                {
                                    Vector2 size = renderer.CurrentTargetSize;
                                    Effect.SetValue(info.handle, &size, sizeof(float) * 2);
                                }
                            }
                        }
                        break;

                    case SasScriptCommand.CommandType.RenderDepthStencilTarget:
                        {
                            SasScriptRenderDepthStencilTarget renderDepthTarget = (SasScriptRenderDepthStencilTarget)commandObject;
                            SasTexture tex = mat.GetTexture(renderDepthTarget.TextureHandle);
                            if (tex != null)
                                renderer.SetDepthTarget(tex.DepthSurface);
                            else
                                renderer.SetDepthTarget(null);
                        }
                        break;

                    case SasScriptCommand.CommandType.ClearSetColor:
                        {
                            SasScriptClearSetColor clearSetColor = (SasScriptClearSetColor)commandObject;
                            clearColor = clearSetColor.Color;
                        }
                        break;

                    case SasScriptCommand.CommandType.ClearSetDepth:
                        {
                            SasScriptClearSetDepth clearSetDepth = (SasScriptClearSetDepth)commandObject;
                            clearDepth = clearSetDepth.Depth;
                        }
                        break;

                    case SasScriptCommand.CommandType.ClearSetStencil:
                        {
                            SasScriptClearSetStencil clearSetStencil = (SasScriptClearSetStencil)commandObject;
                            clearStencil = clearSetStencil.Stencil;
                        }
                        break;

                    case SasScriptCommand.CommandType.Pass:
                        {
                            SasScriptPass pass = (SasScriptPass)commandObject;
                            ip.passNum = pass.PassNum;

                            InstructionPointer ipNew = new InstructionPointer();
                            ipNew.commands = mat.GetPassScript(effect.GetPass(effect.Technique, pass.PassNum));
                            ipNew.currentInstruction = 0;
                            ipNew.materialStackPos = ip.materialStackPos;
                            ipNew.geometryMesh = ip.geometryMesh;
                            ipNew.geometryAttribute = ip.geometryAttribute;
                            ipNew.passNum = ip.passNum;
                            callStack.Push(ipNew);

                            Execute(scene, renderer);
                        }
                        break;

                    case SasScriptCommand.CommandType.LoopByCount:
                        {
                            // Record the return address for this loop & the loop count
                            SasScriptLoopByCount byCount = (SasScriptLoopByCount)commandObject;
                            ip.loopCount = byCount.Count;
                            ip.loopStartCount = byCount.Count;

                            // We already advanced the count, so the current is the next one on.
                            ip.loopReturnInstruction = ip.currentInstruction;
                        }
                        break;

                    case SasScriptCommand.CommandType.LoopGetIndex:
                        {
                            SasScriptLoopGetIndex getIndex = (SasScriptLoopGetIndex)commandObject;
                            if (getIndex.DestinationParameter != null)
                            {
                                SasEffectUtils.SetIntegerParam(effect, getIndex.DestinationParameter, ip.loopCount);
                                mat.DirtyCommands();
                            }
                        }
                        break;

                    case SasScriptCommand.CommandType.LoopGetCount:
                        {
                            SasScriptLoopGetCount getCount = (SasScriptLoopGetCount)commandObject;
                            if (getCount.DestinationParameter != null)
                            {
                                SasEffectUtils.SetIntegerParam(effect, getCount.DestinationParameter, ip.loopStartCount);
                                mat.DirtyCommands();
                            }
                        }
                        break;

                    case SasScriptCommand.CommandType.LoopEnd:
                        {
                            SasScriptLoopEnd end = (SasScriptLoopEnd)commandObject;

                            if (ip.loopReturnInstruction != -1)
                            {
                                ip.loopCount--;
                                if (ip.loopCount > 0)
                                {
                                    ip.currentInstruction = ip.loopReturnInstruction;
                                }
                                else
                                {
                                    ip.loopReturnInstruction = -1;
                                    ip.loopCount = 0; // could be -1 for a 0 entry loop
                                }
                            }
                            else
                            {
                                App.ReportError("loop end with no begin!");
                            }
                        }
                        break;

                    case SasScriptCommand.CommandType.Draw:
                        {
                            SasScriptDraw draw = (SasScriptDraw)commandObject;

                            // Pass number not established, can't go on.
                            if (ip.passNum == -1)
                                break;

                            Effect.Begin(0);
                            Effect.BeginPass(ip.passNum);
                            effect.CommitChanges();

                            switch (draw.DrawOption)
                            {
                                case SasScriptDraw.DrawType.Buffer:
                                    renderer.DrawQuad();
                                    break;

                                // The same thing for now.
                                case SasScriptDraw.DrawType.Geometry:
                                    {
                                        isDrawObjects = true;
                                        if (ip.geometryMesh == null)
                                        {
                                            // Draw the whole scene using this overridden material
                                            foreach (SasMesh mesh in scene.GetMeshes())
                                            {
                                                renderer.Device.VertexDeclaration = mesh.GetVertexDeclaration();
                                                for (int count = 0; count < mesh.GetMaterials().Count; count++)
                                                {
                                                    mesh.Draw(renderer.Device, count);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            renderer.Device.VertexDeclaration = ip.geometryMesh.GetVertexDeclaration();
                                            ip.geometryMesh.Draw(renderer.Device, ip.geometryAttribute);
                                        }
                                    }
                                    break;

                                case SasScriptDraw.DrawType.Scene:
                                    {
                                        isDrawObjects = true;
                                        // Draw the whole scene using this overridden material
                                        foreach (SasMesh mesh in scene.GetMeshes())
                                        {
                                            renderer.Device.VertexDeclaration = mesh.GetVertexDeclaration();
                                            for (int count = 0; count < mesh.GetMaterials().Count; count++)
                                            {
                                                mesh.Draw(renderer.Device, count);
                                            }
                                        }
                                    }
                                    break;

                                default:
                                    break;
                            }
                            Effect.EndPass();
                            Effect.End();
                        }
                        break;

                    case SasScriptCommand.CommandType.Technique:
                        {
                            SasScriptTechnique tech = (SasScriptTechnique)commandObject;
                            Effect.Technique = tech.Handle;


                            InstructionPointer ipNew = new InstructionPointer();
                            ipNew.commands = mat.GetTechniqueScript(tech.Handle);
                            ipNew.currentInstruction = 0;
                            ipNew.materialStackPos = ip.materialStackPos;
                            ipNew.geometryAttribute = ip.geometryAttribute;
                            ipNew.geometryMesh = ip.geometryMesh;
                            ipNew.passNum = ip.passNum;
                            callStack.Push(ipNew);

                            Execute(scene, renderer);
                        }
                        break;

                    case SasScriptCommand.CommandType.ScriptExternal:
                        {
                            SasScriptScriptExternal scriptExternal = (SasScriptScriptExternal)commandObject;
                            if (ip.materialStackPos == 0)
                            {
                                // At the top of the stack.  Walk the scene and draw it using the materials
                                // in the scene.
                                isDrawObjects = true;

                                foreach (SasMesh mesh in scene.GetMeshes())
                                {
                                    renderer.Device.VertexDeclaration = mesh.GetVertexDeclaration();

                                    int subSet = 0;
                                    foreach (XMaterial matCurrent in mesh.GetMaterials())
                                    {
                                        Effect newEffect = matCurrent.MaterialEffect;
                                        if (newEffect == null || newEffect.Disposed)
                                        {
                                            subSet++;
                                            continue;
                                        }

                                        // Prepare the effect parameters
                                        renderer.PreEffectRender(scene, matCurrent);

                                        // Add this material to the stack
                                        materialStack.Add(matCurrent);

                                        InstructionPointer ipNew = new InstructionPointer();
                                        ipNew.materialStackPos = materialStack.Count - 1;
                                        ipNew.currentInstruction = 0;
                                        ipNew.commands = matCurrent.EntryPoint;
                                        ipNew.geometryMesh = mesh;
                                        ipNew.geometryAttribute = subSet;
                                        callStack.Push(ipNew);

                                        subSet++;

                                        // Draw this section
                                        Execute(scene, renderer);
                                    }

                                }
                            }
                            else
                            {

                                InstructionPointer ipNew = new InstructionPointer();
                                ipNew.materialStackPos = ip.materialStackPos - 1;
                                XMaterial newMat = (XMaterial)materialStack[ipNew.materialStackPos];
                                ipNew.commands = newMat.EntryPoint;
                                ipNew.currentInstruction = 0;
                                callStack.Push(ipNew);

                                renderer.PushTargets();

                                // New mat, so set the variables
                                renderer.PreEffectRender(scene, newMat);

                                Execute(scene, renderer);

                                renderer.PopTargets();
                            }
                        }
                        break;

                    default:
                        break;
                }
            }


            callStack.Pop();
            return true;
        }
    }
}
*/


