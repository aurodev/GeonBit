﻿#region LICENSE
/**
 * For the purpose of making video games only, GeonBit is distributed under the MIT license.
 * to use this source code or GeonBit as a whole for any other purpose, please seek written 
 * permission from the library author.
 * 
 * Copyright (c) 2017 Ronen Ness [ronenness@gmail.com].
 * You may not remove this license notice.
 */
#endregion
#region File Description
//-----------------------------------------------------------------------------
// A composite renderable model, made of meshes.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;
using System.Collections.Specialized;

namespace GeonBit.Core.Graphics
{

    /// <summary>
    /// A renderable model, made of multiple mesh renderers.
    /// This type of model is slightly slower than the SimpleModelEntity and ModelEntity, but has the following advantages:
    /// 1. finer-grain control over parts of the model.
    /// 2. proper camera-distance sorting if the model contains both opaque and transparent parts.
    /// </summary>
    public class CompositeModelEntity : BaseRenderableEntity
    {
        /// <summary>
        /// Model to render.
        /// </summary>
        public Model Model
        {
            get; protected set;
        }

        /// <summary>
        /// Dictionary with all the mesh entities.
        /// </summary>
        OrderedDictionary _meshes = new OrderedDictionary();
        
        /// <summary>
        /// Create the model entity from model instance.
        /// </summary>
        /// <param name="model">Model to draw.</param>
        public CompositeModelEntity(Model model)
        {
            Model = model;
            foreach (var mesh in Model.Meshes)
            {
                _meshes[mesh.Name] = new MeshEntity(model, mesh);
            }
        }

        /// <summary>
        /// Return meshes count.
        /// </summary>
        public int MeshesCount
        {
            get { return _meshes.Count; }
        }

        /// <summary>
        /// Get mesh entity by index.
        /// </summary>
        /// <param name="index">Mesh index to get.</param>
        /// <returns>MeshEntity instance for this mesh.</returns>
        public MeshEntity GetMesh(int index)
        {
            return _meshes[index] as MeshEntity;
        }

        /// <summary>
        /// Get mesh entity by name.
        /// </summary>
        /// <param name="name">Mesh name to get.</param>
        /// <returns>MeshEntity instance for this mesh.</returns>
        public MeshEntity GetMesh(string name)
        {
            return _meshes[name] as MeshEntity;
        }

        /// <summary>
        /// Draw this entity.
        /// </summary>
        /// <param name="parent">Parent node that's currently drawing this entity.</param>
        /// <param name="localTransformations">Local transformations from the direct parent node.</param>
        /// <param name="worldTransformations">World transformations to apply on this entity (this is what you should use to draw this entity).</param>
        public override void Draw(Node parent, Matrix localTransformations, Matrix worldTransformations)
        {
            // not visible / no active camera? skip
            if (!Visible || GraphicsManager.ActiveCamera == null)
            {
                return;
            }

            // call draw callback
            OnDraw?.Invoke(this);

            // draw all meshes
            foreach (DictionaryEntry mesh in _meshes)
            {
                GraphicsManager.DrawEntity(mesh.Value as MeshEntity, worldTransformations);
            }
        }

        /// <summary>
        /// Create the model entity from asset path.
        /// </summary>
        /// <param name="path">Path of the model to load.</param>
        public CompositeModelEntity(string path) : this(ResourcesManager.Instance.GetModel(path))
        {
        }

        /// <summary>
        /// Draw this model.
        /// </summary>
        /// <param name="worldTransformations">World transformations to apply on this entity (this is what you should use to draw this entity).</param>
        public override void DoEntityDraw(Matrix worldTransformations)
        {
        }

        /// <summary>
        /// Get the bounding sphere of this entity.
        /// </summary>
        /// <param name="parent">Parent node that's currently drawing this entity.</param>
        /// <param name="localTransformations">Local transformations from the direct parent node.</param>
        /// <param name="worldTransformations">World transformations to apply on this entity (this is what you should use to draw this entity).</param>
        /// <returns>Bounding box of the entity.</returns>
        protected override BoundingSphere CalcBoundingSphere(Node parent, Matrix localTransformations, Matrix worldTransformations)
        {
            BoundingSphere modelBoundingSphere = ModelUtils.GetBoundingSphere(Model);
            modelBoundingSphere.Radius *= worldTransformations.Scale.Length();
            modelBoundingSphere.Center = worldTransformations.Translation;
            return modelBoundingSphere;
        }

        /// <summary>
        /// Get the bounding box of this entity.
        /// </summary>
        /// <param name="parent">Parent node that's currently drawing this entity.</param>
        /// <param name="localTransformations">Local transformations from the direct parent node.</param>
        /// <param name="worldTransformations">World transformations to apply on this entity (this is what you should use to draw this entity).</param>
        /// <returns>Bounding box of the entity.</returns>
        protected override BoundingBox CalcBoundingBox(Node parent, Matrix localTransformations, Matrix worldTransformations)
        {
            // get bounding box in local space
            BoundingBox modelBoundingBox = ModelUtils.GetBoundingBox(Model);

            // initialize minimum and maximum corners of the bounding box to max and min values
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            // iterate bounding box corners and transform them
            foreach (Vector3 corner in modelBoundingBox.GetCorners())
            {
                // get curr position and update min / max
                Vector3 currPosition = Vector3.Transform(corner, worldTransformations);
                min = Vector3.Min(min, currPosition);
                max = Vector3.Max(max, currPosition);
            }

            // create and return transformed bounding box
            return new BoundingBox(min, max);
        }
    }
}