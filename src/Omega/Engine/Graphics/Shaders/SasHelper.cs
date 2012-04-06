/*
 * Copyright 2006-2012 Bastian Eicher
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
using System.Globalization;
using Common;
using Common.Utils;
using SlimDX;
using SlimDX.Direct3D9;

namespace OmegaEngine.Graphics.Shaders
{
    /// <summary>
    /// Helper class for accessing SAS-compliant annotations and reading/setting parameters
    /// </summary>
    internal static class SasHelper
    {
        #region Annotation String
        public static string FindAnnotationString(Effect effect, EffectHandle handle, string name)
        {
            if (effect == null) throw new ArgumentNullException("effect");
            if (handle == null) throw new ArgumentNullException("handle");
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            ParameterDescription paramDesc = effect.GetParameterDescription(handle);
            for (int i = 0; i < paramDesc.Annotations; i++)
            {
                EffectHandle annotationHandle = effect.GetAnnotation(handle, i);
                if (annotationHandle != null)
                {
                    ParameterDescription annotationDesc = effect.GetParameterDescription(annotationHandle);
                    if (annotationDesc.Type == ParameterType.String && StringUtils.Compare(annotationDesc.Name, name))
                        return effect.GetString(annotationHandle);
                }
            }
            return null;
        }
        #endregion

        #region Technique Annotation String
        public static string FindTechniqueAnnotationString(Effect effect, EffectHandle handle, string name)
        {
            if (effect == null) throw new ArgumentNullException("effect");
            if (handle == null) throw new ArgumentNullException("handle");
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            TechniqueDescription paramDesc = effect.GetTechniqueDescription(handle);
            for (int i = 0; i < paramDesc.Annotations; i++)
            {
                EffectHandle annotationHandle = effect.GetAnnotation(handle, i);
                if (annotationHandle != null)
                {
                    ParameterDescription annotationDesc = effect.GetParameterDescription(annotationHandle);
                    if (annotationDesc.Type == ParameterType.String && StringUtils.Compare(annotationDesc.Name, name))
                        return effect.GetString(annotationHandle);
                }
            }
            return null;
        }
        #endregion

        #region Pass Annotation String
        public static string FindPassAnnotationString(Effect effect, EffectHandle handle, string name)
        {
            if (effect == null) throw new ArgumentNullException("effect");
            if (handle == null) throw new ArgumentNullException("handle");
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            PassDescription paramDesc = effect.GetPassDescription(handle);
            for (int i = 0; i < paramDesc.Annotations; i++)
            {
                EffectHandle annotationHandle = effect.GetAnnotation(handle, i);
                if (annotationHandle != null)
                {
                    ParameterDescription annotationDesc = effect.GetParameterDescription(annotationHandle);
                    if (annotationDesc.Type == ParameterType.String && StringUtils.Compare(annotationDesc.Name, name))
                        return effect.GetString(annotationHandle);
                }
            }
            return null;
        }
        #endregion

        #region Annotation Float
        public static float FindAnnotationFloat(Effect effect, EffectHandle handle, string name)
        {
            if (effect == null) throw new ArgumentNullException("effect");
            if (handle == null) throw new ArgumentNullException("handle");
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            ParameterDescription paramDesc = effect.GetParameterDescription(handle);
            for (int i = 0; i < paramDesc.Annotations; i++)
            {
                EffectHandle annotationHandle = effect.GetAnnotation(handle, i);
                if (annotationHandle != null)
                {
                    ParameterDescription annotationDesc = effect.GetParameterDescription(annotationHandle);
                    if (StringUtils.Compare(annotationDesc.Name, name))
                    {
                        try
                        {
                            switch (annotationDesc.Type)
                            {
                                case ParameterType.String:
                                    return float.Parse(effect.GetString(annotationHandle), CultureInfo.InvariantCulture);
                                case ParameterType.Int:
                                    return effect.GetValue<int>(annotationHandle);
                                case ParameterType.Float:
                                    return effect.GetValue<float>(annotationHandle);
                                case ParameterType.Bool:
                                    return effect.GetValue<bool>(annotationHandle) ? 1.0f : 0.0f;
                                default:
                                    return 0.0f;
                            }
                        }
                        catch (Direct3D9Exception)
                        {
                            return 0.0f;
                        }
                    }
                }
            }
            return 0.0f;
        }
        #endregion

        #region Annotation Vector4
        public static Vector4 FindAnnotationVector4(Effect effect, EffectHandle handle, string name)
        {
            if (effect == null) throw new ArgumentNullException("effect");
            if (handle == null) throw new ArgumentNullException("handle");
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            var ret = new Vector4();

            ParameterDescription paramDesc = effect.GetParameterDescription(handle);
            for (int i = 0; i < paramDesc.Annotations; i++)
            {
                EffectHandle annotationHandle = effect.GetAnnotation(handle, i);
                if (annotationHandle != null)
                {
                    ParameterDescription annotationDesc = effect.GetParameterDescription(annotationHandle);
                    if (StringUtils.Compare(annotationDesc.Name, name))
                    {
                        try
                        {
                            switch (annotationDesc.Type)
                            {
                                case ParameterType.Float:
                                {
                                    ret.W = ret.X = ret.Y = ret.Z = 0.0f;
                                    float[] value = effect.GetValue<float>(annotationHandle, annotationDesc.Columns);
                                    switch (annotationDesc.Columns)
                                    {
                                        case 1:
                                            ret.X = ret.Y = ret.Z = ret.W = value[0];
                                            break;
                                        case 2:
                                            ret.X = value[0];
                                            ret.Y = value[1];
                                            break;
                                        case 3:
                                            ret.X = value[0];
                                            ret.Y = value[1];
                                            ret.Z = value[2];
                                            break;
                                        case 4:
                                            ret.X = value[0];
                                            ret.Y = value[1];
                                            ret.Z = value[2];
                                            ret.W = value[3];
                                            break;
                                    }

                                    return ret;
                                }

                                default:
                                    return ret;
                            }
                        }
                        catch (Direct3D9Exception)
                        {
                            return ret;
                        }
                    }
                }
            }
            return ret;
        }
        #endregion

        //--------------------//

        #region Get Integer
        public static int GetIntegerFromParam(Effect effect, EffectHandle handle)
        {
            if (effect == null) throw new ArgumentNullException("effect");
            if (handle == null) throw new ArgumentNullException("handle");

            ParameterDescription desc = effect.GetParameterDescription(handle);
            switch (desc.Type)
            {
                case ParameterType.Bool:
                    var bValue = effect.GetValue<bool>(handle);
                    return (bValue ? 1 : 0);
                case ParameterType.Int:
                    return effect.GetValue<int>(handle);
                case ParameterType.Float:
                    return (int)effect.GetValue<float>(handle);
                default:
                    Log.Error("SAS: Can't convert value to integer: " + desc.Name);
                    break;
            }
            return 0;
        }
        #endregion

        #region Set Integer
        public static void SetIntegerParam(Effect effect, EffectHandle handle, int value)
        {
            if (effect == null) throw new ArgumentNullException("effect");
            if (handle == null) throw new ArgumentNullException("handle");

            ParameterDescription desc = effect.GetParameterDescription(handle);
            switch (desc.Type)
            {
                case ParameterType.Bool:
                    effect.SetValue(handle, (value != 0));
                    break;
                case ParameterType.Int:
                    effect.SetValue(handle, value);
                    break;
                case ParameterType.Float:
                    effect.SetValue(handle, (float)value);
                    break;
                default:
                    Log.Info("SAS: Can't convert value to integer: " + desc.Name);
                    break;
            }
        }
        #endregion

        #region Get Float
        public static float GetFloatFromParam(Effect effect, EffectHandle handle)
        {
            if (effect == null) throw new ArgumentNullException("effect");
            if (handle == null) throw new ArgumentNullException("handle");

            ParameterDescription desc = effect.GetParameterDescription(handle);
            switch (desc.Type)
            {
                case ParameterType.Bool:
                    var bValue = effect.GetValue<bool>(handle);
                    return (bValue ? 1.0f : 0.0f);
                case ParameterType.Int:
                    return effect.GetValue<int>(handle);
                case ParameterType.Float:
                    return effect.GetValue<float>(handle);
                default:
                    Log.Info("SAS: Can't convert value to float: " + desc.Name);
                    break;
            }
            return 0.0f;
        }
        #endregion

        #region Get Vector4
        public static Vector4 GetVector4FromParam(Effect effect, EffectHandle handle)
        {
            if (effect == null) throw new ArgumentNullException("effect");
            if (handle == null) throw new ArgumentNullException("handle");

            var ret = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);
            ParameterDescription desc = effect.GetParameterDescription(handle);
            switch (desc.Type)
            {
                case ParameterType.Bool:
                    var bValue = effect.GetValue<bool>(handle);
                    ret.X = ret.Y = ret.Z = ret.W = (bValue ? 1.0f : 0.0f);
                    break;
                case ParameterType.Int:
                    float fValue = effect.GetValue<int>(handle);
                    ret.X = ret.Y = ret.Z = ret.W = fValue;
                    break;
                case ParameterType.Float:
                    float[] value = effect.GetValue<float>(handle, desc.Columns);
                    switch (desc.Columns)
                    {
                        case 1:
                            ret.X = ret.Y = ret.Z = ret.W = value[0];
                            break;
                        case 2:
                            ret.X = value[0];
                            ret.Y = value[1];
                            break;
                        case 3:
                            ret.X = value[0];
                            ret.Y = value[1];
                            ret.Z = value[2];
                            break;
                        case 4:
                            ret.X = value[0];
                            ret.Y = value[1];
                            ret.Z = value[2];
                            ret.W = value[3];
                            break;
                    }
                    break;
                default:
                    Log.Info("SAS: Can't convert value to Vector4: " + desc.Name);
                    break;
            }
            return ret;
        }
        #endregion
    }
}
