﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BomberEngine.Game;
using BomberEngine.Core.Visual.UI;
using Microsoft.Xna.Framework;
using BomberEngine.Core.Visual;
using Bomberman.Game;
using Assets;
using BomberEngine.Core.Assets.Types;

namespace Bomberman.Menu.Screens
{
    public class MainMenuScreen : Screen
    {
        public enum ButtonId
        {
            Play,
            Multiplayer,
            DebugStartServer,
            DebugStartClient,
            Settings,
            Test,
            Exit
        }

        public MainMenuScreen(ButtonDelegate buttonDelegate)
            : base((int)MenuController.ScreenID.MainMenu)
        {
            int w = 150;
            int h = 20;

            Font font = Helper.fontButton;

            View rootView = new View();

            TextButton button = new TextButton("Play", font, 0, 0, w, h);
            button.id = (int)ButtonId.Play;
            button.buttonDelegate = buttonDelegate;
            rootView.AddView(button);

            button = new TextButton("Test", font, 0, 0, w, h);
            button.id = (int)ButtonId.Test;
            button.buttonDelegate = buttonDelegate;
            rootView.AddView(button);

            button = new TextButton("Join server", font, 0, 0, w, h);
            button.id = (int)ButtonId.DebugStartClient;
            button.buttonDelegate = buttonDelegate;
            rootView.AddView(button);

            button = new TextButton("Start server", font, 0, 0, w, h);
            button.id = (int)ButtonId.DebugStartServer;
            button.buttonDelegate = buttonDelegate;
            rootView.AddView(button);

            //button = new TextButton("Multiplayer", font, 0, 0, w, h);
            //button.id = (int)ButtonId.Multiplayer;
            //button.SetDelegate(buttonDelegate);
            //rootView.AddView(button);

            button = new TextButton("Settings", font, 0, 0, w, h);
            button.id = (int)ButtonId.Settings;
            button.buttonDelegate = buttonDelegate;
            rootView.AddView(button);

            button = new TextButton("Exit", font, 0, 0, w, h);
            button.id = (int)ButtonId.Exit;
            button.buttonDelegate = buttonDelegate;
            rootView.AddView(button);

            rootView.LayoutVer(20);
            rootView.ResizeToFitViews();

            AddView(rootView);

            rootView.x = 0.5f * (width - rootView.width);
            rootView.y = 0.5f * (height - rootView.height);
        }
    }
}
