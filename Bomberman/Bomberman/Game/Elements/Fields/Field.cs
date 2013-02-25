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
        private static Field currentField;

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

        private FieldCellArray cells;
        private PlayerList players;
        private TimerManager timerManager;

        private List<MovableCell> movingCells;

        public Field()
        {
            currentField = this;
            timerManager = new TimerManager();
            players = new PlayerList();
        }

        //////////////////////////////////////////////////////////////////////////////

        #region Setup

        public void Setup(Scheme scheme)
        {
            SetupField(scheme.GetFieldData(), scheme.GetBrickDensity());
            SetupPlayers(players, scheme.GetPlayerLocations());
            SetupPowerups(scheme.GetPowerupInfo());
        }

        private void SetupField(FieldData data, int brickDensity)
        {
            int width = data.GetWidth();
            int height = data.GetHeight();

            cells = new FieldCellArray(width, height);
            movingCells = new List<MovableCell>(width * height); // more than enough

            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    FieldBlocks block = data.Get(x, y);
                    switch (block)
                    {
                        case FieldBlocks.Blank:
                        {   
                            break;
                        }

                        case FieldBlocks.Brick:
                        {
                            if (MathHelp.NextInt(100) <= brickDensity)
                            {
                                AddCell(new BrickCell(x, y));
                            }
                            break;
                        }

                        case FieldBlocks.Solid:
                        {
                            AddCell(new SolidCell(x, y));
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

                if (cx == 0 && cy == 0)
                {
                    AddCell(player);
                }
                else
                {
                    player.SetCell(info.x, info.y);
                }

                ClearBrick(cx - 1, cy);
                ClearBrick(cx, cy - 1);
                ClearBrick(cx + 1, cy);
                ClearBrick(cx, cy + 1);
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
                    if (MathHelp.NextInt(10) < -powerupCount)
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

        private int GetBrickCells(BrickCell[] array)
        {
            int count = 0;
            FieldCellSlot[] slots = cells.GetSlots();
            foreach (FieldCellSlot slot in slots)
            {   
                BrickCell brickCell = slot.GetBrick();
                if (brickCell != null)
                {
                    if (brickCell.powerup == Powerups.None)
                    {
                        array[count++] = brickCell;
                    }
                }
            }

            return count;
        }

        private void ShuffleCells(FieldCell[] array, int size)
        {
            Util.ShuffleArray(array, size);
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////
        
        #region Updatable

        public void Update(float delta)
        {
            timerManager.Update(delta);
            UpdateCells(delta);
            UpdatePhysics(delta);
        }

        private void UpdateCells(float delta)
        {
            FieldCellSlot[] slots = cells.GetSlots();
            foreach (FieldCellSlot slot in slots)
            {
                UpdateSlot(delta, slot);
            }
        }

        public void UpdateSlot(float delta, FieldCellSlot slot)
        {
            FieldCell[] cells = slot.GetCells();
            foreach (FieldCell cell in cells)
            {
                if (cell != null)
                {
                    UpdateCell(delta, cell);
                }
            }
        }

        private void UpdateCell(float delta, FieldCell cell)
        {
            FieldCellIterator iter = CreateIterator(cell);
            while (iter.HasNext())
            {
                FieldCell c = iter.Next();
                Debug.Assert(Debug.flag && CheckCell(c));
                c.Update(delta);
            }
            iter.Destroy();
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////

        #region Physics

        private void UpdatePhysics(float delta)
        {
            UpdateMoving(delta);
            HandleCollisions();
        }

        private void UpdateMoving(float delta)
        {
            foreach (MovableCell cell in movingCells)
            {
                if (cell.moving)
                {
                    cell.UpdateMoving(delta);
                }
            }
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////

        #region Players

        public void AddPlayer(Player player)
        {
            players.Add(player);
        }

        public PlayerList GetPlayers()
        {
            return players;
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////

        #region Bombs

        public void SetBomb(Bomb bomb)
        {
            AddCell(bomb);

            FieldCellSlot slot = GetSlot(bomb);
            PowerupCell powerup = slot.GetPowerup();
            if (powerup != null)
            {
                powerup.RemoveFromField();
            }
        }

        /** Should only be called from Bomb.Blow() */
        public void BlowBomb(Bomb bomb)
        {
            int cx = bomb.GetCx();
            int cy = bomb.GetCy();

            bomb.RemoveFromField();
            SetFlame(bomb, cx, cy);

            bomb.player.OnBombBlown(bomb);

            bool up = true, down = true, left = true, right = true;
            int radius = bomb.GetRadius();

            for (int i = 1; i <= radius && (up || down || left || right); ++i)
            {
                left = left && SetFlame(bomb, cx - i, cy);
                up = up && SetFlame(bomb, cx, cy - i);
                down = down && SetFlame(bomb, cx, cy + i);
                right = right && SetFlame(bomb, cx + i, cy);
            }
        }

        /* Returns true if can be spread more */
        private bool SetFlame(Bomb bomb, int cx, int cy)
        {
            if (!IsInsideField(cx, cy))
            {
                return false;
            }

            FieldCellSlot slot = GetSlot(cx, cy);
            if (slot.ContainsSolid())
            {
                return false;
            }

            BrickCell brickCell = slot.GetBrick();
            if (brickCell != null)
            {   
                if (!brickCell.destroyed)
                {
                    brickCell.Destroy();
                }

                return false;
            }

            PowerupCell powerup = slot.GetPowerup();
            if (powerup != null)
            {
                powerup.RemoveFromField();
                return false;
            }

            Bomb anotherBomb = slot.GetBomb();
            if (anotherBomb != null)
            {
                anotherBomb.Blow();
            }

            Player player = slot.GetPlayer();
            for (FieldCell pc = player; pc != null; pc = pc.listNext)
            {
                pc.AsPlayer().Kill();
            }

            SetFlame(bomb.player, cx, cy);
            return true;
        }

        private void SetFlame(Player player, int cx, int cy)
        {
            FieldCellSlot slot = GetSlot(cx, cy);
            for (FieldCell c = slot.GetFlame(); c != null; c = c.listNext)
            {
                FlameCell flame = c.AsFlame();
                if (flame.player == player)
                {
                    flame.RemoveFromField();
                    break;
                }
            }

            AddCell(new FlameCell(player, cx, cy));
        }

        public void DestroyBrick(BrickCell brick)
        {
            brick.RemoveFromField();

            int powerup = brick.powerup;
            if (powerup != Powerups.None)
            {
                int cx = brick.GetCx();
                int cy = brick.GetCy();
                AddCell(new PowerupCell(powerup, cx, cy));
            }
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////

        #region Powerups

        public void PlacePowerup(int powerupIndex)
        {

        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////

        #region Cells

        public void MoveablePosChanged(MovableCell movable)
        {
            HandleWallCollisions(movable);
        }

        public void MovableCellChanged(MovableCell movable, int oldCx, int oldCy)
        {
            AddCell(movable);
        }

        public FieldCellSlot GetSlot(int index)
        {
            FieldCellSlot[] slots = cells.GetSlots();
            return slots[index];
        }

        public FieldCellSlot GetSlot(FieldCell cell)
        {   
            return GetSlot(cell.cx, cell.cy);
        }

        public FieldCellSlot GetSlot(int cx, int cy)
        {
            return IsInsideField(cx, cy) ? cells.Get(cx, cy) : null;
        }

        public FieldCell[] GetCells(int cx, int cy)
        {
            return GetSlot(cx, cy).GetCells();
        }

        public void AddCell(FieldCell cell)
        {
            cells.Add(cell.GetCx(), cell.GetCy(), cell);
            if (cell.IsMovable())
            {
                AddMovable(cell.AsMovable());
            }
        }

        public void RemoveCell(FieldCell cell)
        {
            int cx = cell.GetCx();
            int cy = cell.GetCy();

            cells.Remove(cell);
            if (cell.IsMovable())
            {
                RemoveMovable(cell.AsMovable());
            }
        }

        private void ClearBrick(int cx, int cy)
        {
            if (IsInsideField(cx, cy))
            {
                BrickCell brick = GetBrick(cx, cy);
                if (brick != null)
                {   
                    RemoveCell(brick);
                }
            }
        }

        private void AddMovable(MovableCell movableCell)
        {
            if (!movingCells.Contains(movableCell))
            {
                movingCells.Add(movableCell);
            }
        }

        private void RemoveMovable(MovableCell movableCell)
        {   
            movingCells.Remove(movableCell);
        }

        private FieldCellIterator CreateIterator(FieldCell cell)
        {
            FieldCellSlot slot = GetSlot(cell.slotIndex);
            return slot.CreateIterator(cell);
        }

        public bool IsObstacleCell(int cx, int cy)
        {
            if (IsInsideField(cx, cy))
            {
                return GetSlot(cx, cy).ContainsObstacle();
            }

            return true;
        }

        public bool IsObstacleCell(FieldCell cell)
        {
            return CellHelper.IsObstacleCell(cell);
        }

        private bool ContainsCell(FieldCell root, FieldCellType type)
        {
            return CellHelper.ContainsCell(root, type);
        }

        private FieldCell FindCell(FieldCell root, FieldCellType type)
        {
            return CellHelper.FindCell(root, type);
        }

        private int CellsCount(FieldCell root)
        {
            return CellHelper.CellsCount(root);
        }

        private void CopyToList(FieldCell root, List<FieldCell> list)
        {
            CellHelper.CopyToList(root, list);
        }

        public bool IsInsideField(int cx, int cy)
        {
            return cx >= 0 && cy >= 0 && cx < GetWidth() && cy < GetHeight();
        }

        private bool CheckCell(FieldCell cell)
        {
            return cells.Contains(cell);
        }

        public FieldCellArray GetCells()
        {
            return cells;
        }

        public FieldCellSlot[] GetSlots()
        {
            return cells.GetSlots();
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////

        public SolidCell GetSolid(int cx, int cy)
        {
            return (SolidCell)GetCell(cx, cy, FieldCellType.Solid);
        }

        public BrickCell GetBrick(int cx, int cy)
        {
            return (BrickCell)GetCell(cx, cy, FieldCellType.Brick);
        }

        public PowerupCell GetPowerup(int cx, int cy)
        {
            return (PowerupCell)GetCell(cx, cy, FieldCellType.Powerup);
        }

        public FlameCell GetFlame(int cx, int cy)
        {
            return (FlameCell)GetCell(cx, cy, FieldCellType.Flame);
        }

        public Bomb GetBomb(int cx, int cy)
        {
            return (Bomb)GetCell(cx, cy, FieldCellType.Bomb);
        }

        public Player GetPlayer(int cx, int cy)
        {
            return (Player)GetCell(cx, cy, FieldCellType.Player);
        }

        public bool ContainsNoObstacle(int cx, int cy)
        {
            FieldCellSlot slot = GetSlot(cx, cy);
            return slot != null && !slot.ContainsObstacle();
        }

        public FieldCell GetCell(int cx, int cy, FieldCellType type)
        {
            FieldCellSlot slot = GetSlot(cx, cy);
            return slot != null ? slot.Get(type) : null;
        }

        //////////////////////////////////////////////////////////////////////////////

        #region Collisions

        private void HandleCollisions()
        {
            FieldCellSlot[] slots = cells.GetSlots();
            for (int i = 0; i < slots.Length; ++i)
            {   
                HandleCollisions(slots[i]);
            }
        }

        private void HandleCollisions(FieldCellSlot slot)
        {
            FieldCell[] cells = slot.GetCells();
            for (int i = 0; i < cells.Length; ++i)
            {
                FieldCell cell = cells[i];
                if (cell != null)
                {
                    HandleCollisions(cell);
                }
            }
        }

        private void HandleCollisions(FieldCell cell)
        {
            FieldCellIterator iter1 = CreateIterator(cell);
            while (iter1.HasNext())
            {
                FieldCell c1 = iter1.Next();

                // check collisions with everyone from the same cell
                FieldCell c2 = c1.listNext;
                if (c2 != null)
                {
                    FieldCellIterator iter2 = CreateIterator(c2);
                    while (iter2.HasNext())
                    {
                        HandleCollisions(c1, iter2.Next());
                    }
                    iter2.Destroy();
                }

                if (c1.slotIndex != -1)
                {
                    // collision optimization: check only for right and down
                    int cx = c1.cx;
                    int cy = c1.cy;

                    HandleCollisions(c1, cx + 1, cy);
                    if (c1.slotIndex == -1) continue;

                    HandleCollisions(c1, cx, cy + 1);
                    if (c1.slotIndex == -1) continue;

                    HandleCollisions(c1, cx + 1, cy + 1);
                }
            }
            iter1.Destroy();
        }

        private void HandleCollisions(FieldCell cell, int neighborCx, int neighborCy)
        {
            if (IsInsideField(neighborCx, neighborCy))
            {
                FieldCell[] neighborCells = GetCells(neighborCx, neighborCy);
                for (int i = 0; i < neighborCells.Length; ++i)
                {
                    FieldCell neighborCell = neighborCells[i];
                    if (neighborCell != null)
                    {
                        FieldCellIterator iter = CreateIterator(neighborCell);
                        while (iter.HasNext())
                        {
                            HandleCollisions(cell, iter.Next());
                        }
                        iter.Destroy();
                    }
                }
            }
        }

        private void HandleCollisions(FieldCell c1, FieldCell c2)
        {
            if (Collides(c1, c2))
            {
                if (!c1.HandleCollision(c2))
                {
                    c2.HandleCollision(c1);
                }
            }
        }

        private void HandleWallCollisions(MovableCell movable)
        {   
            float dx = movable.moveDx;
            float dy = movable.moveDy;

            if (dx > 0)
            {
                float maxX = GetMaxPx();
                if (movable.GetPx() > maxX)
                {
                    movable.SetPosX(maxX);
                    movable.HandleWallCollision();
                }
            }
            else if (dx < 0)
            {
                float minX = GetMinPx();
                if (movable.GetPx() < minX)
                {
                    movable.SetPosX(minX);
                    movable.HandleWallCollision();
                }
            }

            if (dy > 0)
            {
                float maxY = GetMaxPy();
                if (movable.GetPy() > maxY)
                {
                    movable.SetPosY(maxY);
                    movable.HandleWallCollision();
                }
            }
            else if (dy < 0)
            {
                float minY = GetMinPy();
                if (movable.GetPy() < minY)
                {
                    movable.SetPosY(minY);
                    movable.HandleWallCollision();
                }
            }
        }

        private bool Collides(FieldCell a, FieldCell b)
        {
            return Math.Abs(a.px - b.px) < Constant.CELL_WIDTH && Math.Abs(a.py - b.py) < Constant.CELL_HEIGHT;
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////

        #region Getters/Setters

        public static Field Current()
        {
            return currentField;
        }

        public int GetWidth()
        {
            return cells.GetWidth();
        }

        public int GetHeight()
        {
            return cells.GetHeight();
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

        #endregion

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
    }
}
