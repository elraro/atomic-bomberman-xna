﻿using BomberEngine.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BomberEngine.Game
{
    public class ContextImpl : Context
    {
        private GraphicsDeviceManager graphics;

        public ContextImpl(GraphicsDeviceManager graphics)
        {
            this.graphics = graphics;
        }

        public GraphicsDevice GraphicsDevice
        {
            get { return graphics.GraphicsDevice; }
        }
    }
}