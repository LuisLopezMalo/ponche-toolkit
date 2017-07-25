﻿using PoncheToolkit.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoncheToolkit.Graphics3D
{
    /// <summary>
    /// Represent a G-Buffer generated by the engine for every frame. This for the purposes of creating
    /// custom shaders so they have the necessary information to make some post-processing.
    /// </summary>
    public class PTGBuffer : UpdatableStateObject
    {
        private PTRenderTarget2D depthBuffer;
        private PTRenderTarget2D diffuseBuffer;
        private PTRenderTarget2D specularBuffer;
        private PTRenderTarget2D normalBuffer;


        /// <inheritdoc/>
        public override bool UpdateState()
        {
            if (IsStateUpdated)
                return IsStateUpdated;



            IsStateUpdated = true;
            OnStateUpdated();
            return IsStateUpdated;
        }
    }
}
