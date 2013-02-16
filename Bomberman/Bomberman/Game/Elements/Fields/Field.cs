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
using BomberEngine.Util;

namespace Bomberman.Game.Elements.Fields
{
    public class Field : Updatable
    {
        private FieldCellArray cells;

        private PlayerList players;

        private static Field currentField;

        private TimerManager timerManager;

        private static readonly int[] POWERUPS_COUNT =
        {
            Settings.VAL_PU_FIELD_BOMB,
            Settings.VAL_PU_FIELD_FLAME,
            Settings.VAL_PU_FIELD_DISEASE,
            Settings.VAL_PU_FIELD_ABILITY_KICK,
            Settings.VAL_PU_FIELD_EXTRA_SPEED,
            Settings.VAL_PU_FIELD_ABLITY_PUNCH,
            Settings.VAL_PU_FIELD_ABILITY_GRAB,
            Settings.VAL_PU_FIELD_SPOOGER,
            Settings.VAL_PU_FIELD_GOLDFLAME,
            Settings.VAL_PU_FIELD_TRIGGER,
            Settings.VAL_PU_FIELD_JELLY,
            Settings.VAL_PU_FIELD_EBOLA,
            Settings.VAL_PU_FIELD_RANDOM,
        };

        public Field(Scheme scheme, PlayerList players)
        {
            this.players = players;

            currentField = this;
            timerManager = new TimerManager();

            SetupField(scheme.GetFieldData(), scheme.GetBrickDensity());
            SetupPlayers(players, scheme.GetPlayerLocations());
            SetupPowerups(scheme.GetPowerupInfo());
        }

        private void SetupField(FieldData data, int brickDensity)
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
                            cells.Set(x, y, new BlankCell(x, y));
                            break;
                        }

                        case FieldBlocks.Brick:
                        {
                            if (MathHelper.NextInt(100) <= brickDensity)
                            {
                                cells.Set(x, y, new BrickCell(x, y));
                            }
                            else
                            {
                                cells.Set(x, y, new BlankCell(x, y));
                            }
                            break;
                        }

                        case FieldBlocks.Solid:
                        {
                            cells.Set(x, y, new SolidCell(x, y));
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
            List<Player> playerList = players.list;
            foreach (Player player in playerList)
            {
                int index = player.GetIndex();
                PlayerLocationInfo info = locations[index];
                int cx = info.x;
                int cy = info.y;
                player.SetCell(info.x, info.y);

                ClearCell(cx - 1, cy);
                ClearCell(cx, cy - 1);
                ClearCell(cx + 1, cy);
                ClearCell(cx, cy + 1);
            }
        }

        private void SetupPowerups(PowerupInfo[] powerupInfo)
        {
            foreach (PowerupInfo info in powerupInfo)
            {
                if (info.bornWith)
                {
                    List<Player> playerList = players.list;
                    foreach (Player player in playerList)
                    {
                        player.TryAddPowerup(info.powerupIndex);
                    }
                }
            }

            BrickCell[] brickCells = new BrickCell[GetWidth() * GetHeight()];
            int count = GetBrickCells(brickCells);
            if (count == 0)
            {
                return;
            }

            ShuffleCells(brickCells, count);

            int brickIndex = 0;
            foreach (PowerupInfo info in powerupInfo)
            {
                if (info.forbidden || info.bornWith)
                {
                    continue;
                }

                int powerupIndex = info.powerupIndex;
                int powerupCount = POWERUPS_COUNT[powerupIndex];
                if (powerupCount < 0)
                {
                    if (MathHelper.NextInt(10) < -powerupCount)
                    {
                        continue;
                    }
                    powerupCount = 1;
                }

                for (int i = 0; i < powerupCount; ++i)
                {
                    BrickCell cell = brickCells[brickIndex++];
                    int cx = cell.GetCx();
                    int cy = cell.GetCy();

                    cell.powerup = powerupIndex;

                    if (brickIndex == count)
                    {
                        break;
                    }
                }

                if (brickIndex == count)
                {
                    break;
                }
            }
        }

        private int GetEmptyCells(FieldCell[] array)
        {
            int count = 0;
            FieldCell[] cellArray = cells.GetArray();
            foreach (FieldCell cell in cellArray)
            {
                if (cell.IsEmpty() && !IsPlayerAt(cell.GetCx(), cell.GetCy()))
                {
                    cellArray[count++] = cell;
                }
            }

            return count;
        }

        private int GetBrickCells(BrickCell[] array)
        {
            int count = 0;
            FieldCell[] cellArray = cells.GetArray();
            foreach (FieldCell cell in cellArray)
            {
                if (cell.IsBrick() && !IsPlayerAt(cell.GetCx(), cell.GetCy()))
                {
                    BrickCell brickCell = (BrickCell)cell;
                    if (brickCell.powerup == Powerups.None)
                    {
                        array[count++] = (BrickCell)cell;
                    }
                }
            }

            return count;
        }

        private bool IsPlayerAt(int cx, int cy)
        {
            return GetPlayer(cx, cy) != null;
        }

        private Player GetPlayer(int cx, int cy)
        {
            foreach (Player player in players.list)
            {
                if (player.GetCx() == cx && player.GetCy() == cy)
                {
                    return player;
                }
            }

            return null;
        }

        private void ShuffleCells(FieldCell[] array, int size)
        {
            Util.ShuffleArray(array, size);
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
                FieldCell cell = cellsArray[i];
                CheckCell(cell);
                cell.Update(delta);
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

        public void DestroyBrick(BrickCell brick)
        {
            int cx = brick.GetCx();
            int cy = brick.GetCy();
            int powerup = brick.powerup;
            if (powerup != Powerups.None)
            {
                SetCell(new PowerupCell(powerup, cx, cy));
            }
            else
            {
                ClearCell(cx, cy);
            }
        }

        public void PlacePowerup(int powerupIndex)
        {

        }

        /** Should only be called from Bomb.Blow() */
        public void BlowBomb(Bomb bomb)
        {   
            int cx = bomb.GetCx();
            int cy = bomb.GetCy();
            SetExplosion(bomb, cx, cy);

            bomb.player.OnBombBlown(bomb);

            bool up = true, down = true, left = true, right = true;
            int radius = bomb.GetRadius();

            for (int i = 1; i <= radius && (up || down || left || right); ++i)
            {
                left = left && SetExplosion(bomb, cx - i, cy);
                up = up && SetExplosion(bomb, cx, cy - i);
                down = down && SetExplosion(bomb, cx, cy + i);
                right = right && SetExplosion(bomb, cx + i, cy);
            }
        }

        private bool SetExplosion(Bomb bomb, int cx, int cy)
        {
            FieldCell cell = GetCell(cx, cy);
            if (cell == null)
            {
                return false; // bomb hits the wall
            }

            if (cell.IsBrick())
            {
                BrickCell brick = (BrickCell)cell;
                if (!brick.destroyed)
                {
                    brick.Destroy();
                }
                return false; // bomb destroyed a brick
            }

            if (cell.IsSolid())
            {
                return false; // bomb hits solid block
            }

            if (cell.IsBomb() && cell != bomb)
            {
                cell.AsBomb().Blow();
                return true;
            }

            if (cell.IsPowerup())
            {
                ClearCell(cx, cy);
                return false;
            }

            SetCell(new FlameCell(cx, cy));
            return true;
        }

        public PlayerList GetPlayers()
        {
            return players;
        }

        public Player GetPlayer(int index)
        {
            return players.TryGet(index);
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
            FieldCell cell = GetCell(cx, cy);
            if (cell != null && !cell.IsEmpty() && !cell.IsSolid())
            {
                SetCell(new BlankCell(cx, cy));
            }
        }

        public bool IsObstacleCell(int cx, int cy)
        {
            FieldCell cell = GetCell(cx, cy);
            return cell == null || cell.IsObstacle();
        }

        public void CheckCell(FieldCell expectedCell)
        {
            FieldCell actualCell = GetCell(expectedCell.cx, expectedCell.cy);
            Debug.Assert(actualCell == expectedCell);
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

        public void MoveablePosChanged(MovableCell movable)
        {
            ProcessCollisions(movable);
        }

        public void MovableCellChanged(MovableCell movable, int oldCx, int oldCy)
        {
            ClearCell(oldCx, oldCy);
            SetCell(movable);
        }

        public void PlayerCellChanged(Player player, int oldCx, int oldCy)
        {

        }

        private void ProcessCollisions(MovableCell movable)
        {   
            float px = movable.px;
            float py = movable.py;
            float dx = movable.moveDx;
            float dy = movable.moveDy;

            if (dx > 0)
            {
                float maxX = GetMaxPx();
                if (movable.GetPx() > maxX)
                {
                    movable.SetPosX(maxX);
                    movable.OnHitWall();
                }
                else
                {
                    int cx = Util.Px2Cx(px) + 1;
                    int cy = Util.Py2Cy(py);

                    CheckCollision(movable, cx, cy - 1);
                    CheckCollision(movable, cx, cy);
                    CheckCollision(movable, cx, cy + 1);
                }
            }
            else if (dx < 0)
            {
                float minX = GetMinPx();
                if (movable.GetPx() < minX)
                {
                    movable.SetPosX(minX);
                    movable.OnHitWall();
                }
                else
                {
                    int cx = Util.Px2Cx(px) - 1;
                    int cy = Util.Py2Cy(py);

                    CheckCollision(movable, cx, cy - 1);
                    CheckCollision(movable, cx, cy);
                    CheckCollision(movable, cx, cy + 1);
                }
            }

            if (dy > 0)
            {
                float maxY = GetMaxPy();
                if (movable.GetPy() > maxY)
                {
                    movable.SetPosY(maxY);
                    movable.OnHitWall();
                }
                else
                {
                    int cx = Util.Px2Cx(px);
                    int cy = Util.Py2Cy(py) + 1;

                    CheckCollision(movable, cx - 1, cy);
                    CheckCollision(movable, cx, cy);
                    CheckCollision(movable, cx + 1, cy);
                }
            }
            else if (dy < 0)
            {
                float minY = GetMinPy();
                if (movable.GetPy() < minY)
                {
                    movable.SetPosY(minY);
                    movable.OnHitWall();
                }
                else
                {
                    int cx = Util.Px2Cx(px);
                    int cy = Util.Py2Cy(py) - 1;

                    CheckCollision(movable, cx - 1, cy);
                    CheckCollision(movable, cx, cy);
                    CheckCollision(movable, cx + 1, cy);
                }
            }
        }

        private bool CheckCollision(MovableCell movable, int cx, int cy)
        {   
            FieldCell cell = cells.Get(cx, cy);
            if (cell == null)
            {
                return false; // wall
            }

            if (cell.IsEmpty())
            {
                return false; // do not collide with empty cells
            }

            if (Collides(movable, cell))
            {
                return ProcessCollision(movable, cell);
            }

            return false;
        }

        private bool ProcessCollision(MovableCell movable, FieldCell cell)
        {
            if (cell.IsObstacle())
            {   
                if (movable.IsPlayer() && cell.IsBomb())
                {
                    ProcessCollision(movable.AsPlayer(), cell.AsBomb());
                }
                else
                {
                    MoveOutOfCollision(movable, cell);
                    movable.OnHitObstacle(cell);
                }
            }
            else if (cell.IsPowerup())
            {
                ProcessCollision(movable, cell.AsPowerup());
            }
            else if (cell.IsFlame())
            {
                if (movable.IsBomb())
                {
                    movable.AsBomb().Blow();
                }                
            }

            return false;
        }

        private void ProcessCollision(Player player, Bomb bomb)
        {
            if (player.HasKick())
            {
                Direction direction = player.direction;

                if (bomb.moving)
                {
                    MoveOutOfCollision(player, bomb);
                    bomb.Kick(direction);
                }
                else
                {
                    bool farEnoughForKick = false; // true, if distance between player's center and bomb's center is enough for a kick
                    switch (direction)
                    {
                        case Direction.UP:
                        case Direction.DOWN:
                            farEnoughForKick = Math.Abs(player.py - bomb.py) > Constant.CELL_HEIGHT_2;
                            break;
                        case Direction.LEFT:
                        case Direction.RIGHT:
                            farEnoughForKick = Math.Abs(player.px - bomb.px) > Constant.CELL_WIDTH_2;
                            break;
                        default:
                            Debug.Assert(false, "Unknown direction: " + direction);
                            break;
                    }

                    if (farEnoughForKick)
                    {
                        FieldCell blockingCell = bomb.NearCellDir(direction);
                        if (blockingCell != null && !blockingCell.IsObstacle()) // can we kick the bomb here?
                        {
                            bomb.Kick(direction);
                        }
                        MoveOutOfCollision(player, bomb);
                    }
                }
            }
            else
            {
                MoveOutOfCollision(player, bomb);
                player.OnHitObstacle(bomb);
            }
        }

        private void ProcessCollision(MovableCell movable, PowerupCell powerupCell)
        {
            if (movable.IsPlayer())
            {
                int powerup = powerupCell.powerup;
                movable.AsPlayer().TryAddPowerup(powerup);
            }
            ClearCell(powerupCell.cx, powerupCell.cy);
        }

        private bool MoveOutOfCollision(FieldCell a, FieldCell b)
        {
            bool hadCollisiton = false;

            float overlapX = Constant.CELL_WIDTH - MathHelper.Abs(a.px - b.px);
            if (overlapX > 0 && MathHelper.Abs(a.oldPx - b.oldPx) >= Constant.CELL_WIDTH)
            {
                MovableCell ma = a.AsMovable();
                MovableCell mb = b.AsMovable();

                float moveA = Math.Abs(a.moveDx);
                float moveB = Math.Abs(b.moveDx);
                float kA = moveA / (moveA + moveB);
                float kB = 1 - kA;

                if (ma != null) ma.MoveBackX(kA * overlapX);
                if (mb != null) mb.MoveBackX(kB * overlapX);

                hadCollisiton = true;
            }

            float overlapY = Constant.CELL_HEIGHT - MathHelper.Abs(a.py - b.py);
            if (overlapY > 0 && MathHelper.Abs(a.oldPy - b.oldPy) >= Constant.CELL_HEIGHT)
            {
                MovableCell ma = a.AsMovable();
                MovableCell mb = b.AsMovable();

                float moveA = Math.Abs(a.moveDy);
                float moveB = Math.Abs(b.moveDy);
                float kA = moveA / (moveA + moveB);
                float kB = 1 - kA;

                if (ma != null) ma.MoveBackY(kA * overlapY);
                if (mb != null) mb.MoveBackY(kB * overlapY);

                hadCollisiton = true;
            }

            return hadCollisiton;
        }

        private bool Collides(MovableCell a, FieldCell b)
        {
            return Collides(a.px, a.py, b);
        }

        private static bool Collides(float px, float py, FieldCell b)
        {
            return Math.Abs(px - b.px) < Constant.CELL_WIDTH && Math.Abs(py - b.py) < Constant.CELL_HEIGHT;
        }

        private bool IsInsideField(int cx, int cy)
        {
            return cx >= 0 && cx < GetWidth() && cy >= 0 && cy < GetHeight();
        }

        public float GetMinPx()
        {
            return Constant.CELL_WIDTH_2;
        }

        public float GetMinPy()
        {
            return Constant.CELL_HEIGHT_2;
        }

        public float GetMaxPx()
        {
            return Constant.FIELD_WIDTH - Constant.CELL_WIDTH_2;
        }

        public float GetMaxPy()
        {
            return Constant.FIELD_HEIGHT - Constant.CELL_HEIGHT_2;
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
