﻿using Assets;
using BomberEngine.Debugging;
using BomberEngine.Game;
using Bomberman.Game;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Bomberman.Menu;
using BomberEngine.Core.Visual;
using BomberEngine.Core.Assets.Types;
using BomberEngine.Core.Input;
using BomberEngine.Consoles;
using Bomberman.Game.Elements.Players;
using Bomberman.Network;

namespace Bomberman
{
    public class BombermanRootController : RootController
    {
        private MenuController menuController;

        public BombermanRootController(ContentManager contentManager)
            : base(contentManager)
        {   
        }

        protected override void OnStart()
        {
            BombermanAssetManager manager = (BombermanAssetManager)Application.Assets();
            manager.AddPackToLoad(AssetPacks.Packs.ALL);
            manager.LoadImmediately();

            StartMainMenu();
        }

        protected override void OnControllerStop(Controller controller)
        {
            if (controller is MenuController)
            {
                MenuController.ExitCode exitCode = (MenuController.ExitCode)controller.exitCode;
                if (exitCode == MenuController.ExitCode.Quit)
                {
                    Application.sharedApplication.Stop();
                    return;
                }

                GameSettings settings = new GameSettings("x");
                switch (exitCode)
                {
                    case MenuController.ExitCode.SingleStart:
                    {   
                        break;
                    }

                    case MenuController.ExitCode.MultiplayerStart:
                    {
                        settings.mode = GameSettings.Mode.MultiplayeServer;
                        break;
                    }

                    case MenuController.ExitCode.MultiplayerJoin:
                    {
                        ServerInfo serverInfo = controller.exitData as ServerInfo;
                        Debug.Assert(serverInfo != null);

                        settings.mode = GameSettings.Mode.MultiplayerClient;
                        settings.serverInfo = serverInfo;
                        break;
                    }
                }

                StartController(new GameController(settings));
            }
            else if (controller is GameController)
            {
                StartMainMenu();
            }
        }

        protected override CConsole CreateConsole()
        {
            CConsole console = base.CreateConsole();
            CVars.Register(console);
            return console;
        }

        public override bool OnKeyPressed(KeyEventArg e)
        {
            if (base.OnKeyPressed(e))
            {
                return true;
            }

            if (e.key == KeyCode.Oem8)
            {
                ToggleConsole();
                return true;
            }

            return false;
        }

        private void StartMainMenu()
        {
            menuController = new MenuController();
            StartController(menuController);
        }
    }
}
