﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BomberEngine.Core;
using BomberEngine.Debugging;
using Bomberman.Game.Elements.Cells;
using Bomberman.Game.Elements.Players;
using BomberEngine.Util;

namespace Bomberman.Game.Elements.Fields
{
    public class FieldCell : BaseObject, IUpdatable, IResettable, IDestroyable
    {
        public FieldCellType type;

        /* Coordinates in points */
        private float m_px;
        private float m_py;

        /* Position on previous tick */
        private float m_oldPx;
        private float m_oldPy;

        /* Linked list stuff */
        public int slotIndex;

        public FieldCell(FieldCellType type, int cx, int cy)
        {
            this.type = type;

            slotIndex = -1;
            SetCell(cx, cy);
        }

        public virtual void Reset()
        {   
            m_px = m_py = 0.0f;
            m_oldPx = m_oldPy = 0.0f;
            slotIndex = -1;
        }

        public virtual void Destroy()
        {
        }

        public virtual void Update(float delta)
        {   
        }

        public virtual void UpdateAnimation(float delta)
        {
        }

        public virtual void UpdateDumb(float delta)
        {
            UpdateAnimation(delta);
        }

        public void SetCell()
        {
            SetCell(cx, cy);
        }

        public void SetCell(int cx, int cy)
        {
            SetPos(Util.Cx2Px(cx), Util.Cy2Py(cy));
            oldPx = px;
            oldPy = py;
        }
        
        public virtual void SetPos(float px, float py)
        {
            this.m_px = px;
            this.m_py = py;
        }

        public virtual bool IsSolid()
        {
            return false;
        }

        public virtual SolidCell AsSolid()
        {
            return null;
        }

        public virtual bool IsBrick()
        {
            return false;
        }

        public virtual BrickCell AsBrick()
        {
            return null;
        }

        public virtual bool IsObstacle()
        {
            return false;
        }

        public virtual bool IsMovable()
        {
            return false;
        }

        public virtual MovableCell AsMovable()
        {
            return null;
        }

        public virtual bool IsBomb()
        {
            return false;
        }

        public virtual Bomb AsBomb()
        {
            return null;
        }

        public virtual bool IsPlayer()
        {
            return false;
        }

        public virtual Player AsPlayer()
        {
            return null;
        }

        public virtual bool IsFlame()
        {
            return false;
        }

        public virtual FlameCell AsFlame()
        {
            return null;
        }

        public virtual bool IsPowerup()
        {
            return false;
        }

        public virtual PowerupCell AsPowerup()
        {
            return null;
        }

        public int GetCx()
        {
            return cx;
        }

        public int GetCy()
        {
            return cy;
        }

        public float GetPx()
        {
            return px;
        }

        public float GetPy()
        {
            return py;
        }

        public float CellCenterPx()
        {
            return Util.Cx2Px(cx);
        }

        public float CellCenterPy()
        {
            return Util.Cy2Py(cy);
        }

        public float CenterOffsetX()
        {
            return px - CellCenterPx();
        }

        public float CenterOffsetY()
        {
            return py - CellCenterPy();
        }

        public FieldCellSlot GetNearSlot(int dcx, int dcy)
        {
            return GetField().GetSlot(cx + dcx, cy + dcy);
        }

        public FieldCellSlot GetNearSlot(Direction dir)
        {
            switch (dir)
            {
                case Direction.DOWN:
                    return GetNearSlot(0, 1);
                case Direction.UP:
                    return GetNearSlot(0, -1);
                case Direction.LEFT:
                    return GetNearSlot(-1, 0);
                case Direction.RIGHT:
                    return GetNearSlot(1, 0);
                default:
                    Debug.Assert(false, "Unknown dir: " + dir);
                    break;
            }

            return null;
        }

        public bool HasNearObstacle(int dcx, int dcy)
        {
            FieldCellSlot slot = GetNearSlot(dcx, dcy);
            return slot == null || slot.ContainsObstacle();
        }

        public void RemoveFromField()
        {
            GetField().RemoveCell(this);
        }

        protected Field GetField()
        {
            return Field.Current();
        }

        protected FieldCellSlot GetSlot(int cx, int cy)
        {
            return GetField().GetSlot(cx, cy);
        }

        //////////////////////////////////////////////////////////////////////////////

        #region TimerManager

        protected Timer ScheduleTimer(TimerCallback callback, float delay = 0.0f, bool repeated = false)
        {
            return GetField().ScheduleTimer(callback, delay, repeated);
        }

        protected Timer ScheduleTimerOnce(TimerCallback callback, float delay = 0.0f, bool repeated = false)
        {
            return GetField().ScheduleTimerOnce(callback, delay, repeated);
        }

        protected void CancelTimer(TimerCallback callback)
        {
            GetField().CancelTimer(callback);
        }

        protected void CancelAllTimers()
        {
            GetField().CancelAllTimers(this);
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////

        #region Properties

        public int cx
        {
            get { return Util.Px2Cx(px); }
        }

        public int cy
        {
            get { return Util.Py2Cy(py); }
        }

        public float px
        {
            get { return m_px; }
        }

        public float py
        {
            get { return m_py; }
        }

        public float oldPx
        {
            get { return m_oldPx; }
            protected set { m_oldPx = value; }
        }

        public float oldPy
        {
            get { return m_oldPy; }
            protected set { m_oldPy = value; }
        }

        public float moveDx
        {
            get { return m_px - m_oldPx; }
        }

        public float moveDy
        {
            get { return m_py - m_oldPy; }
        }

        #endregion

        public int GetPriority()
        {
            return (int)type;
        }

        public static float OverlapX(FieldCell a, FieldCell b)
        {
            float overlapX = Constant.CELL_WIDTH - Math.Abs(a.px - b.px);
            return overlapX > 0 ? overlapX : 0;
        }

        public static float OverlapY(FieldCell a, FieldCell b)
        {
            float overlapY = Constant.CELL_HEIGHT - Math.Abs(a.py - b.py);
            return overlapY > 0 ? overlapY : 0;
        }
    }
}
