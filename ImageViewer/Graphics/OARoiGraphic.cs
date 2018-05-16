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
    /// A type of <see cref="CompositeGraphic"/> that's basically a list of <see cref="OAVertexControlGraphic"/> objects.
    /// These have as subjects <see cref="RegionalPolylineGraphic"/> (a type of <see cref="PolylineGraphic"/>) objects that are supposed to be drawn simultaneously.
    /// Individually, these appear to behave as <see cref=" PolygonControlGraphic"/> objects, but any changes to one are applied to all graphics in the list.
    /// </summary>
    public class OARoiGraphic: CompositeGraphic
    {
        /// <summary>
        /// Initialize list of <see cref=" RegionalPolylineGraphic"/> and associate each one with a unique region.
        /// </summary>
        /// <param name="reg">An array of <see cref=" Dicom.Iod.Region"/> to be associated with polygons.</param>
        public OARoiGraphic(Dicom.Iod.Region[] reg)
        {
            for (int i = 0; i< reg.Length; i++)
            {
                Graphics.Add(new OAVertexControlGraphic(true,new OARoiMoveControlGraphic(new RegionalPolylineGraphic(true, reg[i]),i,SyncPoints), i, SyncPoints));
            }
            _trackingIndex = 0;
        }

        /// <summary>
        /// Initialize list of <see cref=" RegionalPolylineGraphic"/> and associate each one with a unique region.
        /// Makes copies of a polygon that already exists in a region - one copy for each region, except of course for the original polygon's region -
        /// and wraps them in an <see cref="OARoiMoveControlGraphic"/> and an <see cref="OAVertexControlGraphic"/> so the points and polygons can be moved after drawing.
        /// </summary>
        /// <param name="reg">An array of <see cref=" Dicom.Iod.Region"/> to be associated with polygons.</param>
        /// <param name="copyGraphic">The polygon (as an <see cref=" IPointsGraphic"/>) to copy. Polygon must exist in a region.</param>
        /// <param name="referenceIndex">The array index of the region where the polygon to copy exists.</param>
        public OARoiGraphic(Dicom.Iod.Region[] reg, IPointsGraphic copyGraphic, int referenceIndex)
        {
            PolylineGraphic localgraphic = new PolylineGraphic();
            foreach (System.Drawing.PointF pf in copyGraphic.Points)
            {
                ((IPointsGraphic)localgraphic).Points.Add(new System.Drawing.PointF(pf.X - reg[referenceIndex].RegionLocationMinX0, pf.Y - reg[referenceIndex].RegionLocationMinY0));
            }
            for (int i = 0; i < reg.Length; i++)
            {
                if (i == referenceIndex)
                {
                    Graphics.Add(new OAVertexControlGraphic(true, new OARoiMoveControlGraphic(copyGraphic, i, SyncPoints), referenceIndex, SyncPoints));
                }
                else
                {
                    Graphics.Add(new OAVertexControlGraphic(true, new OARoiMoveControlGraphic(new RegionalPolylineGraphic(true, reg[i], localgraphic.Points), i, SyncPoints), i, SyncPoints));
                }
            }
            _trackingIndex = referenceIndex;
        }

        /// <summary>
        /// Gets the ROI that calculations are performed on. Uses the <see cref="Roi"/> object from the original polygon.
        /// </summary>
        public override Roi GetRoi()
        {
            if (_trackingIndex >= 0)
            {
                return ((Graphics[_trackingIndex] as OAVertexControlGraphic).Subject as RegionalPolylineGraphic).GetRoi();
            }
            return base.GetRoi();
        }

        private int _trackingIndex = 0; //Index of polygon to use for ROI calculations (polygons are identical, but we have to pick one so the calculation won't be, for example, n_regions * area). If polygons were copied from an initial one, this is set to that polygon's region index by the constructor.

        public int StartingRegionIndex
        {
            get { return _trackingIndex; }
        }

        /// <summary>
        /// Sets _trackingIndex so we know which polygon to use for an ROI calculation.
        /// </summary>
        /// <param name="index">The index of the region in the array of <see cref=" Dicom.Iod.Region"/>for the image.</param>
        public void SetCurrentRegion(int index)
        {
            _trackingIndex = index;
        }

        //when a graphic changes, make sure all the other ones do too.
        /// <summary>
        /// Apply the changes in a reference graphic to the rest of the graphics by just copying the new points.
        /// Not a fabulous way to do it but gets the job done.
        /// </summary>
        /// <param name="referenceRegion">Region containing the polygon that was initially changed.</param>
        public void SyncPoints(int referenceRegion)
        {
            IPointsGraphic referenceGraphic = ((Graphics[referenceRegion] as OAVertexControlGraphic).Subject as RegionalPolylineGraphic);

            //make a temporary graphic with all the points relative to a region.
            PolylineGraphic localgraphic = new PolylineGraphic();
            foreach (System.Drawing.PointF pf in referenceGraphic.Points)
            {
                System.Drawing.PointF pr = referenceGraphic.SpatialTransform.ConvertToDestination(new System.Drawing.PointF((referenceGraphic as RegionalPolylineGraphic).Region.RegionLocationMinX0, (referenceGraphic as RegionalPolylineGraphic).Region.RegionLocationMinY0));
                ((IPointsGraphic)localgraphic).Points.Add(new System.Drawing.PointF(pf.X - pr.X, pf.Y - pr.Y));
            }

            for (int i = 0; i < Graphics.Count; i++)
            {
                OAVertexControlGraphic graphic = (Graphics[i] as OAVertexControlGraphic);
                graphic.Suspended = true;
                (graphic.Subject as RegionalPolylineGraphic).SyncPoints(localgraphic.Points);
                graphic.Suspended = false;
            }
        }
    }
}
