﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using BomberEngine.Core.Input;
using BomberEngine.Core.Events;

namespace BomberEngine.Core.Visual
{
    public delegate void ButtonDelegate(Button button);
    
    public class Button : View
    {
        protected ButtonDelegate buttonDelegate;

        public Button(float width, float height)
            : this(0, 0, width, height)
        {
        }

        public Button(float x, float y, float width, float height)
            : base(x, y, width, height)
        {
            focusable = true;
        }

        public override bool HandleEvent(Event evt)
        {
            if (evt.code == Event.KEY)
            {
                KeyEvent keyEvent = evt as KeyEvent;
                if (keyEvent.arg.key == KeyCode.Enter)
                {
                    if (keyEvent.state == KeyEvent.PRESSED)
                    {
                        OnPress();
                        return true;                        
                    }

                    if (keyEvent.state == KeyEvent.RELEASED)
                    {
                        OnRelease();
                        return true;
                    }
                }
            }

            return base.HandleEvent(evt);
        }

        protected virtual void OnPress()
        {
            if (buttonDelegate != null)
            {
                buttonDelegate(this);
            }
        }

        protected virtual void OnRelease()
        {
        }

        //////////////////////////////////////////////////////////////////////////////

        #region Properties

        public ButtonDelegate Delegate()
        {
            return buttonDelegate;
        }

        public void SetDelegate(ButtonDelegate buttonDelegate)
        {
            this.buttonDelegate = buttonDelegate;
        }

        #endregion
    }
}
