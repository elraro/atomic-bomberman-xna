﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bomberman.Game.Elements.Cells;
using Bomberman.Game.Elements.Items;
using Bomberman.Game.Elements.Players.Input;

namespace Bomberman.Game.Elements.Players
{
    public class Player : MovableCell, PlayerInputListener
    {
        private static readonly int SPEED_START = 145;
        private static readonly int SPEED_INCREASE = 25;

        private bool alive;
        private bool moving;

        private int bombRadius;
        private float bombTimeout;
        private bool bombBouncing;
        private bool bombDetonated;

        private PlayerInput input;

        public Player(PlayerInput input, int x, int y)
            : base(x, y)
        {
            this.input = input;
            input.SetListener(this);

            alive = true;
            speed = SPEED_START;
            direction = Direction.DOWN;
        }

        public override void Update(float delta)
        {
            if (moving)
            {
                switch (direction)
                {
                    case Direction.UP:
                    {
                        MoveY(-delta * speed);
                        break;
                    }

                    case Direction.DOWN:
                    {
                        MoveY(delta * speed);
                        break;
                    }

                    case Direction.LEFT:
                    {
                        MoveX(-delta * speed);
                        break;
                    }

                    case Direction.RIGHT:
                    {
                        MoveX(delta * speed);
                        break;
                    }
                }
            }
        }

        public void OnActionPressed(PlayerInput playerInput, PlayerAction action)
        {
            switch (action)
            {
                case PlayerAction.Up:
                {
                    SetMoveDirection(Direction.UP);
                    break;
                }

                case PlayerAction.Down:
                {
                    SetMoveDirection(Direction.DOWN);
                    break;
                }

                case PlayerAction.Left:
                {
                    SetMoveDirection(Direction.LEFT);
                    break;
                }

                case PlayerAction.Right:
                {
                    SetMoveDirection(Direction.RIGHT);
                    break;
                }

                case PlayerAction.Bomb:
                {
                    SetBomb();
                    break;
                }
            }
        }

        public void OnActionReleased(PlayerInput playerInput, PlayerAction action)
        {
            moving = false;

            if (playerInput.GetPressedActionCount() > 0)
            {
                if (playerInput.IsActionPressed(PlayerAction.Up))
                {
                    SetMoveDirection(Direction.UP);
                }
                else if (playerInput.IsActionPressed(PlayerAction.Down))
                {
                    SetMoveDirection(Direction.DOWN);
                }
                else if (playerInput.IsActionPressed(PlayerAction.Left))
                {
                    SetMoveDirection(Direction.LEFT);
                }
                else if (playerInput.IsActionPressed(PlayerAction.Right))
                {
                    SetMoveDirection(Direction.RIGHT);
                }
            }
        }

        public void OnActonsReleased(PlayerInput playerInput)
        {
            moving = false;
        }

        private void SetMoveDirection(Direction direction)
        {
            this.direction = direction;
            moving = true;
        }

        public bool IsAlive()
        {
            return alive;
        }

        public Bomb SetBomb()
        {
            return new Bomb(this, false); // TODO: calculate dud
        }

        public float GetBombTimeout()
        {
            return bombTimeout;
        }

        public int GetBombRadius()
        {
            return bombRadius;
        }

        public bool IsBombBouncing()
        {
            return bombBouncing;
        }

        public bool IsBobmDetonated()
        {
            return bombDetonated;
        }
    }
}
