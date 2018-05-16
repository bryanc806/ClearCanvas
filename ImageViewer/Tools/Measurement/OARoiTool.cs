#region License

// Copyright (c) 2010, ClearCanvas Inc.
// All rights reserved.
//
// Redistribution and use in source and binary forms, with or without modification, 
// are permitted provided that the following conditions are met:
//
//    * Redistributions of source code must retain the above copyright notice, 
//      this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, 
//      this list of conditions and the following disclaimer in the documentation 
//      and/or other materials provided with the distribution.
//    * Neither the name of ClearCanvas Inc. nor the names of its contributors 
//      may be used to endorse or promote products derived from this software without 
//      specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, 
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR 
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR 
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, 
// OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE 
// GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
// STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN 
// ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY 
// OF SUCH DAMAGE.

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ClearCanvas.Common;
using ClearCanvas.Common.Utilities;
using ClearCanvas.Desktop;
using ClearCanvas.Desktop.Tools;
using ClearCanvas.Desktop.Actions;
using ClearCanvas.ImageViewer;
using ClearCanvas.ImageViewer.Automation;
using ClearCanvas.ImageViewer.BaseTools;
using ClearCanvas.ImageViewer.Graphics;
using ClearCanvas.ImageViewer.RoiGraphics;
using ClearCanvas.ImageViewer.Imaging;
using ClearCanvas.ImageViewer.InputManagement;
using ClearCanvas.ImageViewer.InteractiveGraphics;

namespace ClearCanvas.ImageViewer.Tools.Measurement
{
   [MenuAction("activate", "imageviewer-contextmenu/MenuOARoi", "Select", Flags = ClickActionFlags.CheckAction, InitiallyAvailable = false)]
    [MenuAction("activate", "global-menus/MenuTools/MenuMeasurement/MenuOARoi", "Select", Flags = ClickActionFlags.CheckAction)]
    [ButtonAction("activate", "global-toolbars/ToolbarMeasurement/ToolbarOARoi", "Select", Flags = ClickActionFlags.CheckAction)]
    [CheckedStateObserver("activate", "Active", "ActivationChanged")]
    [TooltipValueObserver("activate", "Tooltip", "TooltipChanged")]
    [MouseButtonIconSet("activate", "Icons.OARoiToolSmall.png", "Icons.OARoiToolMedium.png", "Icons.OARoiToolLarge.png")]
    [GroupHint("activate", "Tools.Image.Annotations.Measurement.Roi.OA")]
    [MouseToolButton(XMouseButtons.Left, false)]
    [ExtensionOf(typeof(ImageViewerToolExtensionPoint))]

    /// <summary>
    /// Tool that creates a <see cref="IPointsGraphic"/>, similarly to <see cref="PolygonalRoiTool"/>.
    /// Then copies this to other OA image regions.
    /// </summary>
    /// <remarks>
    /// Input (clicks to place points) is only allowed in OA regions defined for the image and once initial input is received,
    /// subsequent input is only permitted for the initial region until the graphic is complete.
    /// </remarks>
    public partial class OARoiTool : MeasurementTool
    {
        /// <summary>
        /// Determines OA image region where mouse was clicked. Returns region index in an array of <see cref="Dicom.Iod.Regions"/> for the current image.
        /// </summary>
        protected override int CheckRegion(IMouseInformation mouseInformation)
        {
            var image = Context.Viewer.SelectedPresentationImage;
            if (image is DicomColorPresentationImage)
            {
                try
                {
                    var curFrame = ((DicomColorPresentationImage)image).Frame;
                    var reg = (Dicom.Iod.Regions)curFrame.Regions;
                    if (reg.theRegions != null)
                    {
                        for (int i = 0; i < reg.theRegions.Length; i++)
                        {
                            System.Drawing.PointF imagePoint = TranslatePointToImage(mouseInformation.Location);
                            if (reg.theRegions[i].HitTest(imagePoint.X, imagePoint.Y))
                            {
                                return i;
                            }
                        }
                    }
                }
                catch(System.Exception exc)
	 	        {
	 	            Platform.Log(LogLevel.Error, "Exception Using Regions" + exc.Message);
	 	        }
            }
            return _invalidRegion;
        }

        /// <summary>
        /// Translate point from destination window coordinates into image coordinates
        /// so any subsequent operations performed relative to image (like the <see cref="Dicom.Iod.Regions"/> hit test) are valid.
        /// </summary>
        System.Drawing.PointF TranslatePointToImage(System.Drawing.PointF p)
        {
            System.Drawing.PointF point = p;
            var image = Context.Viewer.SelectedPresentationImage;
            if (!CanStart(image) || !(image is DicomColorPresentationImage))
                return point;

            var imageGraphic = ((IImageGraphicProvider)image).ImageGraphic;
            point = imageGraphic.SpatialTransform.ConvertToSource(p);
            return point;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <remarks>
        /// A no-args constructor is required by the framework.  Do not remove.
        /// </remarks>
        public OARoiTool()
            : base("Create ROI that is automatically copied to other OA regions")
        {
        }

        protected override RoiGraphic CreateRoiGraphic(bool initiallySelected)
        {
            RoiGraphic aRoiGraphic = base.CreateRoiGraphic(initiallySelected);
            aRoiGraphic.Callout.ShowAnalysis = false;
            return aRoiGraphic;
        }

        protected override string CreationCommandName
        {
            get { return SR.CommandCreateOARoi; }
        }

        protected override string RoiNameFormat
        {
            get { return SR.FormatOAName; }
        }

        public override bool Start(IMouseInformation mouseInformation)
        {
            if (!CheckValidOARegion(mouseInformation))
            {
                //don't start if outside valid or current region
                return false;
            }
            return base.Start(mouseInformation);
        }

        protected bool CheckValidOARegion(IMouseInformation mouseInformation)
        {
            //If we haven't started, check that the click in a valid region.
            //note that this will return false if we don't have regions.
            if (_startingRegion == -1)
            {
                return (CheckRegion(mouseInformation) >= 0);
            }
            //We have started. Check that click is in the same region.
            return (CheckRegion(mouseInformation) == _startingRegion);
        }

        /// <summary>
        /// Creates a <see cref="RegionalPolylineGraphic"/>. Associates initial graphic with current region.
        /// Returns null if region is invalid.
        /// </summary>
        protected override IGraphic CreateGraphic()
        {
            var image = Context.Viewer.SelectedPresentationImage;
            if (image is DicomColorPresentationImage)
            {
                try
                {
                    var curFrame = ((DicomColorPresentationImage)image).Frame;
                    var reg = (Dicom.Iod.Regions)curFrame.Regions;
                    if ((reg != null) && (_startingRegion != _invalidRegion))
                    {
                        return new RegionalPolylineGraphic(true, reg.theRegions[_startingRegion]);
                    }
                }
                catch (System.Exception exc)
                {
                    Platform.Log(LogLevel.Error, "Exception Using Regions" + exc.Message);
                }
            }
            return null;
        }

        protected override InteractiveGraphicBuilder CreateGraphicBuilder(IGraphic graphic)
        {
            return new InteractivePolygonGraphicBuilder((IPointsGraphic)graphic);
        }

        protected override IAnnotationCalloutLocationStrategy CreateCalloutLocationStrategy()
        {
            return new PolygonalRoiCalloutLocationStrategy();
        }

        /// <summary>
        /// Finishes graphic building and copies complete polygon to other OA regions.
        /// Resets current region.
        /// </summary>
        protected override void OnGraphicBuilderComplete(object sender, GraphicEventArgs e)
        {
            var image = Context.Viewer.SelectedPresentationImage;
            if (image is DicomColorPresentationImage)
            {
                try
                {
                    var curFrame = ((DicomColorPresentationImage)image).Frame;
                    var reg = (Dicom.Iod.Regions)curFrame.Regions;
                    var regArray = (Dicom.Iod.Region[])(reg.theRegions);

                    //if regions aren't valid for some reason, just treat this like a polygon roi.
                    //we shouldn't get here really.
                    if (regArray == null)
                    {
                        _startingRegion = _invalidRegion;
                        base.OnGraphicBuilderComplete(sender, e);
                    }
                    else
                    {
                        OARoiGraphic oacopies = new OARoiGraphic(reg.theRegions, (IPointsGraphic)(_graphicBuilder.Graphic as PolylineGraphic), _startingRegion);

                        var provider = (IOverlayGraphicsProvider)(Context.Viewer.SelectedPresentationImage);
                        _undoableCommand = new DrawableUndoableCommand(image);
                        _undoableCommand.Enqueue(new AddGraphicUndoableCommand(oacopies, provider.OverlayGraphics));
                        _undoableCommand.Name = CreationCommandName;
                        _undoableCommand.Execute();

                        _graphicBuilder.GraphicComplete -= OnGraphicBuilderComplete;
                        _graphicBuilder.GraphicCancelled -= OnGraphicBuilderCancelled;
                        _graphicBuilder.Graphic.ImageViewer.CommandHistory.AddCommand(_undoableCommand);
                        _graphicBuilder.Graphic.Draw();

                        _undoableCommand = null;

                        _graphicBuilder = null;
                        _startingRegion = _invalidRegion;
                    }
                }
                catch (System.Exception exc)
                {
                    Platform.Log(LogLevel.Error, "Exception Completing Regional Graphic" + exc.Message);
                }
            }
            else
            {
                _startingRegion = _invalidRegion;
                base.OnGraphicBuilderComplete(sender, e);
            }
        }

        /// <summary>
        /// Cancels graphic building, also resets current region.
        /// </summary>
        protected override void OnGraphicBuilderCancelled(object sender, GraphicEventArgs e)
        {
            _graphicBuilder.GraphicComplete -= OnGraphicBuilderComplete;
            _graphicBuilder.GraphicCancelled -= OnGraphicBuilderCancelled;

            _undoableCommand.Unexecute();
            _undoableCommand = null;

            _graphicBuilder = null;
            _startingRegion = _invalidRegion;
        }
    }

    #region Oto
    partial class OARoiTool : IDrawPolygon
    {
        AnnotationGraphic IDrawPolygon.Draw(CoordinateSystem coordinateSystem, string name, IList<System.Drawing.PointF> vertices)
        {
            var image = Context.Viewer.SelectedPresentationImage;
            if (!CanStart(image))
                throw new InvalidOperationException("Can't draw a OA polygon at this time.");

            var imageGraphic = ((IImageGraphicProvider)image).ImageGraphic;
            if (coordinateSystem == CoordinateSystem.Destination)
                vertices = vertices.Select(v => imageGraphic.SpatialTransform.ConvertToSource(v)).ToList();

            var overlayProvider = (IOverlayGraphicsProvider)image;
            var roiGraphic = CreateRoiGraphic(false);
            roiGraphic.Name = name;
            AddRoiGraphic(image, roiGraphic, overlayProvider);

            var subject = (IPointsGraphic)roiGraphic.Subject;

            foreach (var vertex in vertices)
                subject.Points.Add(vertex);

            roiGraphic.Callout.Update();
            roiGraphic.State = roiGraphic.CreateSelectedState();
            //roiGraphic.Draw();
            return roiGraphic;
        }
    }
    #endregion
}
