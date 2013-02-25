﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bomberman.Game.Elements.Cells;
using Bomberman.Game.Elements.Players;
using BomberEngine;
using BomberEngine.Debugging;
using BomberEngine.Util;

namespace Bomberman.Game.Elements.Fields
{
    public class FieldCellSlot
    {
        public FieldCell[] cells;

        public FieldCellSlot()
        {
            int size = (int)FieldCellType.Count;
            cells = new FieldCell[size];
        }

        //////////////////////////////////////////////////////////////////////////////

        #region Add/Remove

        public void AddCell(FieldCell cell)
        {
            int index = GetIndex(cell);
            cells[index] = ListUtils.Add(cells[index], cell);
        }

        public void RemoveCell(FieldCell cell)
        {
            FieldCellIterator.CellRemoved(cell);

            int index = GetIndex(cell);
            cells[index] = ListUtils.Remove(cells[index], cell);
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////

        #region Getters/Setters

        public FieldCell[] GetCells()
        {
            return cells;
        }

        public FieldCell Get(FieldCellType type)
        {
            int index = GetIndex(type);
            return cells[index];
        }

        public int Size()
        {
            return cells.Length;
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////

        #region Helpers

        public bool ContainsSolid()
        {
            return Contains(FieldCellType.Solid);
        }

        public SolidCell GetSolid()
        {
            return (SolidCell)Get(FieldCellType.Solid);
        }

        public bool ContainsBrick()
        {
            return Contains(FieldCellType.Brick);
        }

        public BrickCell GetBrick()
        {
            return (BrickCell)Get(FieldCellType.Brick);
        }

        public bool ContainsPowerup()
        {
            return Contains(FieldCellType.Powerup);
        }

        public PowerupCell GetPowerup()
        {
            return (PowerupCell)Get(FieldCellType.Powerup);
        }

        public bool ContainsFlame()
        {
            return Contains(FieldCellType.Flame);
        }

        public FlameCell GetFlame()
        {
            return (FlameCell)Get(FieldCellType.Flame);
        }

        public bool ContainsBomb()
        {
            return Contains(FieldCellType.Bomb);
        }

        public Bomb GetBomb()
        {
            return (Bomb)Get(FieldCellType.Bomb);
        }

        public bool ContainsPlayer()
        {
            return Contains(FieldCellType.Player);
        }

        public Player GetPlayer()
        {
            return (Player)Get(FieldCellType.Player);
        }

        public bool ContainsObstacle()
        {
            foreach (FieldCell cell in cells)
            {
                if (cell != null && cell.IsObstacle())
                {
                    return true;
                }
            }

            return false;
        }

        public bool Contains(FieldCell cell)
        {
            int index = GetIndex(cell);
            for (FieldCell c = cells[index]; c != null; c = c.listNext)
            {
                if (c == cell)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Contains(FieldCellType type)
        {
            return Get(type) != null;
        }

        private int GetIndex(FieldCell cell)
        {
            return (int)cell.type;
        }

        private int GetIndex(FieldCellType type)
        {
            return (int)type;
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////

        #region Iterators

        private FieldCellIterator iteratorRoot;

        internal void AddIterator(FieldCellIterator iterator)
        {
            if (iteratorRoot != null)
            {
                iteratorRoot.prev = iterator;
            }
            iterator.prev = null;
            iterator.next = iteratorRoot;
            iteratorRoot = iterator;
        }

        internal void RemoveIterator(FieldCellIterator iterator)
        {
            Debug.Assert(iteratorRoot != null);

            FieldCellIterator prev = iterator.prev;
            FieldCellIterator next = iterator.next;
            if (prev != null)
            {
                prev.next = next;
            }
            else
            {
                iteratorRoot = next;
            }

            if (next != null)
            {
                next.prev = prev;
            }
        }

        public FieldCellIterator CreateIterator(FieldCell cell)
        {
            FieldCellIterator iter = FieldCellIterator.GetFree(this, cell);
            if (iteratorRoot != null)
            {
                iteratorRoot.prev = iter;
            }
            iter.next = iteratorRoot;
            iteratorRoot = iter;
            return iter;
        }

        #endregion
    }
}
