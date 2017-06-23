﻿#region LICENSE
//-----------------------------------------------------------------------------
// For the purpose of making video games, educational projects or gamification,
// GeonBit is distributed under the MIT license and is totally free to use.
// To use this source code or GeonBit as a whole for other purposes, please seek 
// permission from the library author, Ronen Ness.
// 
// Copyright (c) 2017 Ronen Ness [ronenness@gmail.com].
// Do not remove this license notice.
//-----------------------------------------------------------------------------
#endregion
#region File Description
//-----------------------------------------------------------------------------
// A basic renderable model.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace GeonBit.Core.Graphics
{

    /// <summary>
    /// A basic renderable model.
    /// This type of model renderer renders the entire model as a single unit, and not as multiple meshes, but still
    /// provide some control over materials etc.
    /// </summary>
    public class ModelEntity : BaseRenderableEntity
    {
        /// <summary>
        /// Model to render.
        /// </summary>
        public Model Model
        {
            get; protected set;
        }

        /// <summary>
        /// Should we process mesh parts?
        /// This option is useful for inheriting types, it will iterate meshes before draw calls and call a virtual processing function.
        /// </summary>
        virtual protected bool ProcessMeshParts { get { return false; } }

        /// <summary>
        /// Add bias to distance from camera when sorting by distance from camera.
        /// </summary>
        override public float CameraDistanceBias { get { return _lastRadius * 100f; } }

        // store last rendering radius (based on bounding sphere)
        float _lastRadius = 0f;

        /// <summary>
        /// Blending state of this entity.
        /// </summary>
        public BlendState BlendingState = BlendState.AlphaBlend;

        /// <summary>
        /// Dictionary with materials to use per meshes.
        /// Key is mesh name, value is material to use for this mesh.
        /// </summary>
        Dictionary<string, Materials.MaterialAPI[]> _materials = new Dictionary<string, Materials.MaterialAPI[]>();

        /// <summary>
        /// Get materials dictionary.
        /// </summary>
        internal Dictionary<string, Materials.MaterialAPI[]>  OverrideMaterialsDictionary { get { return _materials; } }

        /// <summary>
        /// If set, will always replace the world matrix when rendering entity.
        /// </summary>
        public Matrix? OverrideWorldMatrix = null;

        /// <summary>
        /// Optional custom render settings for this specific instance.
        /// Note: this method is much less efficient than materials override.
        /// </summary>
        public MaterialOverrides MaterialOverride = new MaterialOverrides();

        /// <summary>
        /// Create the model entity from model instance.
        /// </summary>
        /// <param name="model">Model to draw.</param>
        public ModelEntity(Model model)
        {
            // store model
            Model = model;
        }

        /// <summary>
        /// Create the model entity from asset path.
        /// </summary>
        /// <param name="path">Path of the model to load.</param>
        public ModelEntity(string path) : this(ResourcesManager.Instance.GetModel(path))
        {
        }

        /// <summary>
        /// Copy materials from another dictionary of materials.
        /// </summary>
        /// <param name="materials">Source materials to copy.</param>
        public void CopyMaterials(Dictionary<string, Materials.MaterialAPI[]> materials)
        {
            foreach (var pair in materials)
            {
                _materials[pair.Key] = pair.Value;
            }
        }

        /// <summary>
        /// Set alternative material for a specific mesh id.
        /// </summary>
        /// <param name="material">Material to set.</param>
        /// <param name="meshId">Mesh name. If empty string is provided, this material will be used for all meshes.</param>
        public void SetMaterial(Materials.MaterialAPI material, string meshId = "")
        {
            _materials[meshId] = new Materials.MaterialAPI[] { material };
        }

        /// <summary>
        /// Set alternative materials for a specific mesh id.
        /// </summary>
        /// <param name="material">Materials to set.</param>
        /// <param name="meshId">Mesh name. If empty string is provided, this material will be used for all meshes.</param>
        public void SetMaterials(Materials.MaterialAPI[] material, string meshId = "")
        {
            _materials[meshId] = material;
        }

        /// <summary>
        /// Get material for a given mesh id.
        /// </summary>
        /// <param name="meshId">Mesh id to get material for.</param>
        /// <param name="effectIndex">Effect index to get material for.</param>
        public Materials.MaterialAPI GetMaterial(string meshId, int effectIndex = 0)
        {
            // material to return
            Materials.MaterialAPI[] ret = null;

            // try to get global material or material for this specific mesh
            if (_materials.TryGetValue("", out ret) || _materials.TryGetValue(meshId, out ret))
            {
                // get material for effect index or null if overflow
                return effectIndex < ret.Length ? ret[effectIndex] : null;
            }

            // if not found, return the default material attached to the mesh effect
            return (Materials.MaterialAPI)Model.Meshes[meshId].Effects[effectIndex].GetMaterial();
        }

        /// <summary>
        /// Draw this model.
        /// </summary>
        /// <param name="worldTransformations">World transformations to apply on this entity (this is what you should use to draw this entity).</param>
        public override void DoEntityDraw(Matrix worldTransformations)
        {
            // use override world matrix
            if (OverrideWorldMatrix != null)
            {
                worldTransformations = OverrideWorldMatrix.Value;
            }

            // reset last radius
            _lastRadius = 0f;
            float scaleLen = worldTransformations.Scale.Length();

            // set blend state
            GraphicsManager.GraphicsDevice.BlendState = BlendingState;

            // iterate model meshes
            foreach (var mesh in Model.Meshes)
            {
                // iterate over mesh effects
                for (int index = 0; index < mesh.Effects.Count; ++index)
                {
                    // get material for this mesh and effect index
                    Materials.MaterialAPI material = GetMaterial(mesh.Name, index);

                    // no material found? skip.
                    // note: this can happen if user set alternative materials array with less materials than original mesh file
                    if (material == null) { break; }

                    // update per-entity override properties
                    material = MaterialOverride.Apply(material);

                    // apply material effect on the mesh part
                    material.Apply(worldTransformations);
                    mesh.MeshParts[index].Tag = mesh.MeshParts[index].Effect;
                    mesh.MeshParts[index].Effect = material.Effect;
                }

                // update last radius
                _lastRadius = System.Math.Max(_lastRadius, mesh.BoundingSphere.Radius * scaleLen);

                // iterate mesh parts
                if (ProcessMeshParts)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        // call the before-drawing-mesh-part callback
                        BeforeDrawingMeshPart(part);
                    }
                }

                // draw the mesh itself
                mesh.Draw();

                // restore original effect
                for (int index = 0; index < mesh.Effects.Count; ++index)
                {
                    mesh.MeshParts[index].Effect = mesh.MeshParts[index].Tag as Effect;
                }
            }
        }

        /// <summary>
        /// Called before drawing each mesh part.
        /// This is useful to extend this model with animations etc.
        /// </summary>
        /// <param name="part">Mesh part we are about to draw.</param>
        protected virtual void BeforeDrawingMeshPart(ModelMeshPart part)
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