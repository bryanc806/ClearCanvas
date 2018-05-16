#region License

// Copyright (c) 2013, ClearCanvas Inc.
// All rights reserved.
// http://www.clearcanvas.ca
//
// This file is part of the ClearCanvas RIS/PACS open source project.
//
// The ClearCanvas RIS/PACS open source project is free software: you can
// redistribute it and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// The ClearCanvas RIS/PACS open source project is distributed in the hope that it
// will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General
// Public License for more details.
//
// You should have received a copy of the GNU General Public License along with
// the ClearCanvas RIS/PACS open source project.  If not, see
// <http://www.gnu.org/licenses/>.

#endregion

using System.Drawing;
using ClearCanvas.Common.Utilities;
using ClearCanvas.ImageViewer.Graphics;
using System.Diagnostics;

namespace ClearCanvas.ImageViewer.InteractiveGraphics
{
    /// <summary>
    /// A control graphic intended to sync <see cref="IPointsGraphic"/> subject changes with other graphics.
    /// </summary>
    [Cloneable]
    public class OAVertexControlGraphic: PolygonControlGraphic
    {
        public delegate void OAVertexControlCallback(int region);
        public OAVertexControlCallback OnGraphicModify = null; //a callback that lets us sync the points.

        private bool _suspendAllEvents = false;

        public bool Suspended //used to suspend events so we don't keep firing them off
        {
            get { return _suspendAllEvents; }
            set { 
                _suspendAllEvents = value;
                if (value)
                    SuspendControlPointEvents();
                else
                    ResumeControlPointEvents();
            }
        }

        private int _region = 0;

        public int Region
        {
            get { return _region; }
        }

        /// <summary>
        /// Init the graphic with a callback to sync points and an associated region.
        /// </summary>
        /// <param name="canAddRemoveVertices">A value indicating whether or not the user can dynamically add or remove vertices on the subject.</param>
        /// <param name="subject">An <see cref="IPointsGraphic"/> or an <see cref="IControlGraphic"/> chain whose subject is an <see cref="IPointsGraphic"/>.</param>
        /// <param name="region">The index of the OA image region associated with the subject graphic</param>
        /// <param name="modifyCallback">The callback for syncing points.</param>
        public OAVertexControlGraphic(bool canAddRemoveVertices, IGraphic subject, int region, OAVertexControlCallback modifyCallback)
            : base(canAddRemoveVertices, subject)
        {
            OnGraphicModify = modifyCallback;
            _region = region;
            ResyncEndPoints();
        }
        
        protected OAVertexControlGraphic(PolygonControlGraphic source, ICloningContext context)
            : base(source, context)
        {
            context.CloneFields(source, this);
        }


        protected override void DeleteVertex()
        {
            base.DeleteVertex();
            ResyncEndPoints();
            if (!Suspended && (OnGraphicModify != null))
                OnGraphicModify(_region);
        }
        
        protected override void OnControlPointChanged(int index, PointF point)
        {
            base.OnControlPointChanged(index, point);

            IPointsGraphic pointsGraphic = this.Subject;
            if (pointsGraphic.Points.Count > 1)
            {
                if (index == 0)
                    base.OnControlPointChanged(pointsGraphic.Points.Count - 1, point);
                if (index == pointsGraphic.Points.Count - 1)
                    base.OnControlPointChanged(0, point);
            }
            if (!Suspended && (OnGraphicModify != null))
            {
                Suspended = true;
                OnGraphicModify(_region);
                Suspended = false;
            }
        }
    }
}