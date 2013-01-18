﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BomberEngine.Core;
using BomberEngine.Core.Visual;
using Assets;
using BomberEngine.Game;
using Bomberman.Game.Elements.Players;
using Bomberman.Game.Elements.Cells;
using BomberEngine.Debugging;
using Bomberman.Game.Elements.Items;
using Bomberman.Content;
using BombermanCommon.Resources.Scheme;

namespace Bomberman.Game.Elements.Fields
{
    public class Field : Updatable
    {
        private FieldCellArray cells;

        private PlayerList players;

        private static Field currentField;

        private TimerManager timerManager;
        
        public Field(Scheme scheme, PlayerList players)
        {
            this.players = players;

            currentField = this;
            timerManager = new TimerManager();

            SetupField(scheme.GetFieldData());
            SetupPlayers(players, scheme.GetPlayerLocations());
        }

        private void SetupField(FieldData data)
        {
            int width = data.GetWidth();
            int height = data.GetHeight();

            cells = new FieldCellArray(width, height);

            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    FieldBlocks block = data.Get(x, y);
                    switch (block)
                    {
                        case FieldBlocks.Blank:
                        {
                            cells.Set(x, y, new EmptyCell(x, y));
                            break;
                        }

                        case FieldBlocks.Brick:
                        {
                            cells.Set(x, y, new BrickCell(x, y, false));
                            break;
                        }

                        case FieldBlocks.Solid:
                        {
                            cells.Set(x, y, new BrickCell(x, y, true));
                            break;
                        }

                        default:
                        {
                            Debug.Assert(false, "Unsupported cell type: " + block);
                            break;
                        }
                    }
                }
            }
        }

        private void SetupPlayers(PlayerList players, PlayerLocationInfo[] locations)
        {
            List<Player> playerList = players.GetList();
            foreach (Player player in playerList)
            {
                int index = player.GetIndex();
                PlayerLocationInfo info = locations[index];
                player.SetCell(info.x, info.y);
            }
        }

        public void Update(float delta)
        {
            timerManager.Update(delta);

            UpdateCells(delta);

            players.Update(delta);
        }

        private void UpdateCells(float delta)
        {
            FieldCell[] cellsArray = cells.GetArray();
            for (int i = 0; i < cellsArray.Length; ++i)
            {
                cellsArray[i].Update(delta);
            }
        }

        public void AddPlayer(Player player)
        {
            players.Add(player);
        }

        public void SetBomb(Bomb bomb)
        {
            cells.Set(bomb.GetCx(), bomb.GetCy(), bomb);
        }

        public void BlowBomb(Bomb bomb)
        {
            int cx = bomb.GetCx();
            int cy = bomb.GetCy();

            SetExplosion(bomb, cx, cy);
            SpreadExplosion(bomb, cx - 1, cy);
            SpreadExplosion(bomb, cx, cy - 1);
            SpreadExplosion(bomb, cx + 1, cy);
            SpreadExplosion(bomb, cx, cy + 1);
        }

        private void SpreadExplosion(Bomb bomb, int startCx, int startCy)
        {
            int dcx = startCx - bomb.GetCx();
            int dcy = startCy - bomb.GetCy();

            int radius = bomb.GetRadius();
            int cx = startCx;
            int cy = startCy;

            for (int i = 0; i < radius; ++i)
            {
                bool spreaded = SetExplosion(bomb, cx, cy);
                if (!spreaded)
                {
                    break;
                }

                cx += dcx;
                cy += dcy;
            }
        }

        private bool SetExplosion(Bomb bomb, int cx, int cy)
        {
            FieldCell cell = GetCell(cx, cy);
            if (cell == null)
            {
                return false; // bomb hits the wall
            }

            if (cell.IsBreakable())
            {
                SetCell(new EmptyCell(cx, cy));
                return false; // bomb destroyed a brick
            }

            if (cell.IsSolid())
            {
                return false; // bomb hits solid block
            }

            if (cell.IsBomb() && cell != bomb)
            {
                BlowBomb((Bomb)cell);
                return true;
            }

            SetCell(new ExplosionCell(cx, cy));
            return true;
        }

        public PlayerList GetPlayers()
        {
            return players;
        }

        public FieldCell GetCell(int cx, int cy)
        {
            return cells.Get(cx, cy);
        }

        public void SetCell(FieldCell cell)
        {
            cells.Set(cell.GetCx(), cell.GetCy(), cell);
        }

        public void ClearCell(int cx, int cy)
        {
            SetCell(new EmptyCell(cx, cy));
        }

        public bool IsObstacleCell(int cx, int cy)
        {
            FieldCell cell = GetCell(cx, cy);
            return cell == null || cell.IsObstacle();
        }

        public FieldCellArray GetCells()
        {
            return cells;
        }

        public int GetWidth()
        {
            return cells.GetWidth();
        }

        public int GetHeight()
        {
            return cells.GetHeight();
        }

        public void CellPosChanged(MovableCell cell, float oldPx, float oldPy)
        {
            ProcessFieldBounds(cell);
            ProcessCollisions(cell, oldPx, oldPy);
        }

        private static void ProcessFieldBounds(MovableCell cell)
        {
            Direction direction = cell.GetDirection();
            switch (direction)
            {
                case Direction.LEFT:
                {
                    float minX = 0.5f * Constant.CELL_WIDTH;
                    if (cell.GetPx() < minX)
                    {
                        cell.SetPosX(minX);
                    }
                    break;
                }

                case Direction.RIGHT:
                {
                    float maxX = Constant.FIELD_WIDTH - 0.5f * Constant.CELL_WIDTH;
                    if (cell.GetPx() > maxX)
                    {
                        cell.SetPosX(maxX);
                    }
                    break;
                }

                case Direction.UP:
                {
                    float minY = 0.5f * Constant.CELL_HEIGHT;
                    if (cell.GetPy() < minY)
                    {
                        cell.SetPosY(minY);
                    }
                    break;
                }

                case Direction.DOWN:
                {
                    float maxY = Constant.FIELD_HEIGHT - 0.5f * Constant.CELL_HEIGHT;
                    if (cell.GetPy() > maxY)
                    {
                        cell.SetPosY(maxY);
                    }
                    break;
                }
            }
        }

        private void ProcessCollisions(MovableCell cell, float oldPx, float oldPy)
        {
            float px = cell.GetPx();
            float py = cell.GetPy();
            float dx = px - oldPx;
            float dy = py - oldPy;

            if (dx > 0)
            {
                int cx = Util.Px2Cx(px) + 1;
                int cy = Util.Py2Cy(py);

                ProcessCollision(cell, cx, cy - 1);
                ProcessCollision(cell, cx, cy);
                ProcessCollision(cell, cx, cy + 1);
            }
            else if (dx < 0)
            {
                int cx = Util.Px2Cx(px) - 1;
                int cy = Util.Py2Cy(py);

                ProcessCollision(cell, cx, cy - 1);
                ProcessCollision(cell, cx, cy);
                ProcessCollision(cell, cx, cy + 1);
            }

            if (dy > 0)
            {
                int cx = Util.Px2Cx(px);
                int cy = Util.Py2Cy(py) + 1;

                ProcessCollision(cell, cx - 1, cy);
                ProcessCollision(cell, cx, cy);
                ProcessCollision(cell, cx + 1, cy);
            }
            else if (dy < 0)
            {
                int cx = Util.Px2Cx(px);
                int cy = Util.Py2Cy(py) - 1;

                ProcessCollision(cell, cx - 1, cy);
                ProcessCollision(cell, cx, cy);
                ProcessCollision(cell, cx + 1, cy);
            }
        }

        private void ProcessCollision(MovableCell cell, int cx, int cy)
        {
            if (cx >= 0 && cx < GetWidth() && cy >=0 && cy < GetHeight())
            {
                FieldCell other = cells.Get(cx, cy);

                if (other.IsObstacle())
                {
                    if (Collides(cell, other))
                    {
                        switch (cell.GetDirection())
                        {
                            case Direction.UP:
                            {
                                cell.SetPosY(Util.Cy2Py(cy + 1));
                                break;
                            }
                            case Direction.DOWN:
                            {
                                cell.SetPosY(Util.Cy2Py(cy - 1));
                                break;
                            }
                            case Direction.LEFT:
                            {
                                cell.SetPosX(Util.Cx2Px(cx + 1));
                                break;
                            }
                            case Direction.RIGHT:
                            {
                                cell.SetPosX(Util.Cx2Px(cx - 1));
                                break;
                            }
                        }
                    }
                }
            }
        }

        private bool Collides(MovableCell a, FieldCell b)
        {
            float acx = a.GetPx();
            float acy = a.GetPy();
            float bcx = b.GetPx();
            float bcy = b.GetPy();

            return Math.Abs(acx - bcx) < Constant.CELL_WIDTH && Math.Abs(acy - bcy) < Constant.CELL_HEIGHT;
        }

        private bool IsInsideField(int cx, int cy)
        {
            return cx >= 0 && cx < GetWidth() && cy >= 0 && cy < GetHeight();
        }

        //////////////////////////////////////////////////////////////////////////////

        #region TimerManager

        public Timer ScheduleTimer(TimerCallback callback, float delay)
        {
            return ScheduleTimer(callback, delay, false);
        }

        public Timer ScheduleTimer(TimerCallback callback, float delay, bool repeated)
        {
            return timerManager.Schedule(callback, delay, repeated);
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////

        public static Field Current()
        {
            return currentField;
        }
    }
}
