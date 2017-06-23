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
// Collision shape for a cone.
//
// Author: Ronen Ness.
// Since: 2017.
//-----------------------------------------------------------------------------
#endregion
using Microsoft.Xna.Framework;

namespace GeonBit.Core.Physics.CollisionShapes
{
    /// <summary>
    /// Which axis the cylinder is aligned with.
    /// </summary>
    public enum CylinderDirectionAxis
    {
        /// <summary>
        /// Cylinder aligned with X axis.
        /// </summary>
        X,

        /// <summary>
        /// Cylinder aligned with Y axis.
        /// </summary>
        Y,

        /// <summary>
        /// Cylinder aligned with Z axis.
        /// </summary>
        Z,
    }

    /// <summary>
    /// Cone collision shape.
    /// </summary>
    public class CollisionCylinder : ICollisionShape
    {
        /// <summary>
        /// Create the collision cylinder.
        /// </summary>
        /// <param name="halfExtent">Half extent on X, Y and Z of the cylinder.</param>
        /// <param name="axis">Cylinder axis direction.</param>
        public CollisionCylinder(Vector3 halfExtent, CylinderDirectionAxis axis = CylinderDirectionAxis.Y)
        {
            switch (axis)
            {
                case CylinderDirectionAxis.X:
                    _shape = new BulletSharp.CylinderShapeX(halfExtent.X, halfExtent.Y, halfExtent.Z);
                    break;

                case CylinderDirectionAxis.Y:
                    _shape = new BulletSharp.CylinderShape(halfExtent.X, halfExtent.Y, halfExtent.Z);
                    break;

                case CylinderDirectionAxis.Z:
                    _shape = new BulletSharp.CylinderShapeZ(halfExtent.X, halfExtent.Y, halfExtent.Z);
                    break;
            }
        }
    }
}
