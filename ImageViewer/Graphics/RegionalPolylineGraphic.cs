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
using System.Collections.Generic;
using System.Linq;
using ClearCanvas.ImageViewer.InteractiveGraphics;
using System.Text;
using ClearCanvas.ImageViewer.RoiGraphics;
using System.Diagnostics;

namespace ClearCanvas.ImageViewer.Graphics
{
    /// <summary>
    /// A <see cref="PolylineGraphic"/> but with region info and methods for adding points within a region.
    /// </summary>
    public class RegionalPolylineGraphic : PolylineGraphic
    {
        Dicom.Iod.Region region = null; //the image region the polygon exists in.
        public Dicom.Iod.Region Region
        {
            get { return region; }
        }

        /// <summary>
        /// Initialize polyline graphic with associated OA region.
        /// </summary>
        /// <param name="closedOnly">Used by base methods to ensure polygon is treated as closed for the purposes of ROI calculations.</param>
        /// <param name="reg">The <see cref="Dicom.Iod.Region"/> where this graphic exists.</param>
        public RegionalPolylineGraphic(bool closedOnly, Dicom.Iod.Region reg)
            : base(closedOnly)
        {
            region = reg;
        }

        /// <summary>
        /// Initialize polyline graphic from a prexisting list of points and associate with OA region.
        /// </summary>
        /// <param name="closedOnly">Used by base methods to ensure polygon is treated as closed for the purposes of ROI calculations.</param>
        /// <param name="reg">The <see cref="Dicom.Iod.Region"/> where this graphic exists.</param>
        /// <param name="points">A list of vertices to form the polygon with. These should be relative
        /// to an individual region's origin and not absolute, since they'll be translated to the associated region.
        /// </param>
        public RegionalPolylineGraphic(bool closedOnly, Dicom.Iod.Region reg, IPointsList points)
            : base(closedOnly)
        {
            region = reg;
            foreach (System.Drawing.PointF point in points)
            {
                AddPoint(point);
            }
        }
        /// <summary>
        /// Reinit polygon vertices using list of new points. Intended to be used by <see cref=" OARoiGraphic"/> to sync points with other
        /// polygons after a move or edit.
        /// </summary>
        /// <param name="points">the new points, in destination coordinates, relative to a region.</param>
        public void SyncPoints(IPointsList points)
        {
            CoordinateSystem = ClearCanvas.ImageViewer.Graphics.CoordinateSystem.Destination;
            Points.Clear();
            foreach (System.Drawing.PointF p in points)
            {
                AddPointDest(p);
            }
            ResetCoordinateSystem();
        }

        /// <summary>
        /// Adds a point in destination coordinates. Used for consistency when syncing points when image is being edited.
        /// </summary>
        /// <param name="point">the point to add, in destination coordinates, relative to a region.</param>
        public void AddPointDest(System.Drawing.PointF point)
        {
            //need to convert region data
            System.Drawing.PointF regionmin = this.SpatialTransform.ConvertToDestination(new System.Drawing.PointF(region.RegionLocationMinX0, region.RegionLocationMinY0));

            System.Drawing.PointF p = new System.Drawing.PointF(point.X + regionmin.X, point.Y + regionmin.Y);

            //check if new point is within region.
            //can't use hit test because of coordinate system though.

            System.Drawing.PointF regionmax = this.SpatialTransform.ConvertToDestination(new System.Drawing.PointF(region.RegionLocationMaxX1, region.RegionLocationMaxY1));
            //also we need to know how it's out of bounds

            if (p.X > regionmax.X)
            {
                p.X = regionmax.X;
            }
            if (p.Y > regionmax.Y)
            {
                p.Y = regionmax.Y;
            }
            if (p.X < regionmin.X)
            {
                p.X = regionmin.X;
            }
            if (p.Y < regionmin.Y)
            {
                p.Y = regionmin.Y;
            }
            Points.Add(p);
        }

        /// <summary>
        /// Used for initializing polygon points.
        /// </summary>
        /// <param name="point">the point to add</param>
        public void AddPoint(System.Drawing.PointF point)
        {
            System.Drawing.PointF p = new System.Drawing.PointF(point.X + region.RegionLocationMinX0, point.Y + region.RegionLocationMinY0);
            Points.Add(p);
        }

    }
}
