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

using System;
using System.Drawing;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Desktop;
using ClearCanvas.ImageViewer.Graphics;
using ClearCanvas.ImageViewer.InputManagement;
using ClearCanvas.ImageViewer.Mathematics;

namespace ClearCanvas.ImageViewer.InteractiveGraphics
{
    /// <summary>
    /// An interactive graphic that allows an individual <see cref=" RegionalPolylineGraphic"/> in a <see cref=" OARoiGraphic"/> to be moved
    /// and have those changes applied to its sibling graphics.
    /// </summary>
    [Cloneable]
    public class OARoiMoveControlGraphic : MoveControlGraphic
    {
        public delegate void OAVertexControlCallback(int region);
        public OAVertexControlCallback OnGraphicModify = null; //a callback that lets us sync the points.

        private int _region = 0; //the region the subject polygon graphic exists in

        public int Region
        {
            get { return _region; }
        }

        /// <summary>
        /// Constructs a new instance of <see cref="OARoiMoveControlGraphic"/>.
        /// </summary>
        /// <param name="subject">The subject graphic.</param>
        /// <param name="region">the OA image region the subject graphic exists in</param>
        /// <param name="modifyCallback">the method to be called to update sibling <see cref="RegionalPolylineGraphic"/> objects when a change occurs.</param>
        public OARoiMoveControlGraphic(IGraphic subject, int region, OAVertexControlCallback modifyCallback)
            : base(subject)
        {
            OnGraphicModify = modifyCallback;
            _region = region;
        }

        /// <summary>
        /// Moves the <see cref="RegionalPolylineGraphic"/> by a specified delta and
        /// calls OnGraphicModify to apply the changes to the other <see cref=" RegionalPolylineGraphic"/> objects in a <see cref=" OARoiGraphic"/>
        /// </summary>
        /// <param name="delta">The distance to move.</param>
        /// <remarks>
        /// Depending on the value of <see cref="CoordinateSystem"/>,
        /// <paramref name="delta"/> will be interpreted in either source
        /// or destination coordinates.
        /// </remarks>
        public override void Move(SizeF delta)
        {
            base.Move(delta);
            if (OnGraphicModify != null)
            {
                OnGraphicModify(_region);
            }
        }
    }
}