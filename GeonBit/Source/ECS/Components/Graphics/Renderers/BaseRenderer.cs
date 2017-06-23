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
// Implement basic functionality for components that render stuff.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion

namespace GeonBit.ECS.Components.Graphics
{
    /// <summary>
    /// Base implementation for most graphics-related components.
    /// </summary>
    public abstract class BaseRendererComponent : BaseComponent
    {
        /// <summary>
        /// Get the main entity instance of this renderer.
        /// </summary>
        protected abstract Core.Graphics.BaseRenderableEntity Entity { get; }

        /// <summary>
        /// Set / get the rendering queue of this entity.
        /// </summary>
        public Core.Graphics.RenderingQueue RenderingQueue
        {
            get { return Entity.RenderingQueue; }
            set { Entity.RenderingQueue = value; }
        }

        /// <summary>
        /// Copy basic properties to another component (helper function to help with Cloning).
        /// </summary>
        /// <param name="copyTo">Other component to copy values to.</param>
        /// <returns>The object we are copying properties to.</returns>
        protected override BaseComponent CopyBasics(BaseComponent copyTo)
        {
            base.CopyBasics(copyTo);
            ((BaseRendererComponent)copyTo).RenderingQueue = RenderingQueue;
            return copyTo;
        }

        /// <summary>
        /// Called when GameObject turned disabled.
        /// </summary>
        protected override void OnDisabled()
        {
            Entity.Visible = false;
        }

        /// <summary>
        /// Called when GameObject is enabled.
        /// </summary>
        protected override void OnEnabled()
        {
            Entity.Visible = true;
        }

        /// <summary>
        /// Change component parent GameObject.
        /// </summary>
        /// <param name="prevParent">Previous parent.</param>
        /// <param name="newParent">New parent.</param>
        override protected void OnParentChange(GameObject prevParent, GameObject newParent)
        {
            // remove from previous parent
            if (prevParent != null && prevParent.SceneNode != null)
            {
                prevParent.SceneNode.RemoveEntity(Entity);
            }

            // add model entity to new parent
            if (newParent != null && newParent.SceneNode != null)
            {
                newParent.SceneNode.AddEntity(Entity);
            }
        }
    }
}
