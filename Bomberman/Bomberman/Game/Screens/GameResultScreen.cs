﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BomberEngine.Game;
using BomberEngine.Core.Assets.Types;
using BomberEngine.Core.Visual;
using Bomberman.UI;

namespace Bomberman.Game.Screens
{
    public class GameResultScreen : Screen
    {
        public enum ButtonId
        {
            Exit
        }

        public GameResultScreen(Game game, ButtonDelegate buttonDelegate)
        {
            Font font = Helper.fontButton;
            TextView text = new TextView(font, "GAME ENDED");
            text.alignX = View.ALIGN_CENTER;
            text.alignY = View.ALIGN_CENTER;
            text.x = 0.5f * width;
            text.y = 0.5f * height;

            Button button = new TempButton("EXIT");
            button.alignX = View.ALIGN_CENTER;
            button.buttonDelegate = buttonDelegate;
            button.id = (int)ButtonId.Exit;
            button.x = 0.5f * width;
            button.y = text.y + 50;
            AddView(button);

            SetCancelButton(button);
        }
    }
}
