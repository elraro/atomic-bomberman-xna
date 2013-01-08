﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BomberEngine.Game;
using Microsoft.Xna.Framework;
using BombermanLive.game;

namespace BombermanLive
{
    public class BombermanApplication : Application
    {
        public BombermanApplication(GraphicsDeviceManager graphics) : base(graphics)
        {
            RootController = new BombermanRootController();
        }
    }
}