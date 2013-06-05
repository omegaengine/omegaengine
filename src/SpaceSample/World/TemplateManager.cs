/*
 * Copyright 2006-2013 Bastian Eicher
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
using Common.Collections;
using World.Properties;

namespace World
{
    /// <summary>
    /// Manages lists of <see cref="Template{T}"/>s.
    /// </summary>
    public static class TemplateManager
    {
        #region Constants
        /// <summary>The XML file <see cref="EntityTemplates"/> is stored in.</summary>
        public const string EntityFileName = "EntityTemplates.xml";

        /// <summary>The XML file <see cref="ItemTemplates"/> is stored in.</summary>
        public const string ItemFileName = "ItemTemplates.xml";

        /// <summary>The XML file <see cref="TerrainTemplates"/> is stored in.</summary>
        public const string TerrainFileName = "TerrainTemplates.xml";
        #endregion

        #region Properties
        private static TemplateCollection<EntityTemplate> _entityTemplates;

        /// <summary>
        /// A list of all loaded <see cref="EntityTemplate"/>s.
        /// </summary>
        public static NamedCollection<EntityTemplate> EntityTemplates
        {
            get
            {
                #region Sanity checks
                if (_entityTemplates == null) throw new InvalidOperationException(string.Format(Resources.TemplateNotLoaded, "Entity"));
                #endregion

                return _entityTemplates;
            }
        }

        private static TemplateCollection<ItemTemplate> _itemTemplates;

        /// <summary>
        /// A list of all loaded <see cref="ItemTemplate"/>s.
        /// </summary>
        public static NamedCollection<ItemTemplate> ItemTemplates
        {
            get
            {
                #region Sanity checks
                if (_itemTemplates == null) throw new InvalidOperationException(string.Format(Resources.TemplateNotLoaded, "Item"));
                #endregion

                return _itemTemplates;
            }
        }

        private static TemplateCollection<TerrainTemplate> _terrainTemplates;

        /// <summary>
        /// A list of all loaded <see cref="TerrainTemplate"/>s.
        /// </summary>
        public static NamedCollection<TerrainTemplate> TerrainTemplates
        {
            get
            {
                #region Sanity checks
                if (_terrainTemplates == null) throw new InvalidOperationException(string.Format(Resources.TemplateNotLoaded, "Terrain"));
                #endregion

                return _terrainTemplates;
            }
        }
        #endregion

        //--------------------//

        #region Access
        /// <summary>
        /// Loads the lists of <see cref="EntityTemplate"/>s, <see cref="ItemTemplate"/>s and <see cref="TerrainTemplate"/>s.
        /// </summary>
        public static void LoadLists()
        {
            _entityTemplates = TemplateCollection<EntityTemplate>.FromContent(EntityFileName);
            _itemTemplates = TemplateCollection<ItemTemplate>.FromContent(ItemFileName);
            _terrainTemplates = TemplateCollection<TerrainTemplate>.FromContent(TerrainFileName);
        }

        /// <summary>
        /// Gets an <see cref="EntityTemplate"/> by a certain name.
        /// </summary>
        /// <param name="name">The name of the <see cref="EntityTemplate"/>.</param>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="name"/> was not found in the collection.</exception>
        public static EntityTemplate GetEntityTemplate(string name)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (_entityTemplates == null) throw new InvalidOperationException(string.Format(Resources.TemplateNotLoaded, "Entity"));
            #endregion

            try
            {
                return _entityTemplates[name];
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidOperationException(string.Format(Resources.TemplateUnknown, "Entity", name));
            }
        }

        /// <summary>
        /// Gets an <see cref="ItemTemplate"/> by a certain name.
        /// </summary>
        /// <param name="name">The name of the <see cref="ItemTemplate"/>.</param>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="name"/> was not found in the collection.</exception>
        public static ItemTemplate GetItemTemplate(string name)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (_itemTemplates == null) throw new InvalidOperationException(string.Format(Resources.TemplateNotLoaded, "Item"));
            #endregion

            try
            {
                return _itemTemplates[name];
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidOperationException(string.Format(Resources.TemplateUnknown, "Item", name));
            }
        }

        /// <summary>
        /// Gets a <see cref="TerrainTemplate"/> by a certain name.
        /// </summary>
        /// <param name="name">The name of the <see cref="TerrainTemplate"/>.</param>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="name"/> was not found in the collection.</exception>
        public static TerrainTemplate GetTerrainTemplate(string name)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (_terrainTemplates == null) throw new InvalidOperationException(string.Format(Resources.TemplateNotLoaded, "Terrain"));
            #endregion

            try
            {
                return _terrainTemplates[name];
            }
            catch (KeyNotFoundException)
            {
                throw new InvalidOperationException(string.Format(Resources.TemplateUnknown, "Terrain", name));
            }
        }
        #endregion
    }
}
