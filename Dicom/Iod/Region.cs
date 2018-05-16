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
using ClearCanvas.Dicom.Utilities;
using System.Drawing;
using System.Diagnostics;


namespace ClearCanvas.Dicom.Iod
{
	/// <summary>
	/// Represents a DICOM region of an image.
    /// C.8.5.5 US Region Calibration Module
    ///  The US Region Calibration Module has been introduced into the ultrasound IOD to provide access to the full range of data 
    ///  that may be present in a single US image. US images often contain multiple regions that have independent data regions, 
    ///   e.g., quad screen loops that may have different calibration information. The data presented in the various regions of a US image can represent
    ///   a multiplicity of physical parameters, e.g., spatial distance, blood velocity, time, volume, etc., and these are often contained in the value of the pixel itself.
    ///   It is therefore imperative that physical information be available for the various regions of a single region independent of each other.
	/// </summary>
	public class Region //: IEquatable<PixelSpacing>
    {
		#region Private Members

        protected Rectangle theRectangle;
        protected PixelSpacing thePixelSpacingMM;
        protected double xMeasurement = 0;
        protected double yMeasurement = 0;

        bool _isValid = false;
        int  _RegionSpatialFormat;          // 0018,6012
        int  _RegionDataType;               // 0018,6014
        long _RegionFlags;                  // 0018,6016
        long _RegionLocationMinX0;         // 0018,6018
        long _RegionLocationMinY0;         // 0018,601A
        long _RegionLocationMaxX1;         // 0018,601C
        long _RegionLocationMaxY1;         // 0018,601E
        int  _PhysicalUnitsXDirection;      // 0018,6024
        int  _PhysicalUnitsYDirection;      // 0018,6026
        double _PhysicalDeltaX;            // 0018,602C
        double _PhysicalDeltaY;            // 0018,602E

		#endregion
		
		/// <summary>
		/// Constructor.
		/// </summary>
        public Region(int aRegionSpatialFormat,
                      int aRegionDataType,
                      long aRegionFlags,
                      long aRegionLocationMinX0,
                      long aRegionLocationMinY0,
                      long aRegionLocationMaxX1,
                      long aRegionLocationMaxY1,
                      int aPhysicalUnitsXDirection,
                      int aPhysicalUnitsYDirection,
                      double aPhysicalDeltaX,
                      double aPhysicalDeltaY)
		{
            _RegionSpatialFormat=aRegionSpatialFormat;
            _RegionDataType= aRegionDataType;
            _RegionFlags= aRegionFlags;
            _RegionLocationMinX0=aRegionLocationMinX0;
            _RegionLocationMinY0=aRegionLocationMinY0;
            _RegionLocationMaxX1=aRegionLocationMaxX1;
            _RegionLocationMaxY1=aRegionLocationMaxY1;
            _PhysicalUnitsXDirection=aPhysicalUnitsXDirection;
            _PhysicalUnitsYDirection=aPhysicalUnitsYDirection;
            _PhysicalDeltaX=aPhysicalDeltaX;
            _PhysicalDeltaY=aPhysicalDeltaY;

            // Intialize Default Rectangle Values
            theRectangle = new Rectangle(
                Convert.ToInt32(_RegionLocationMinX0),
                Convert.ToInt32(_RegionLocationMinY0),
                Convert.ToInt32(_RegionLocationMaxX1 - _RegionLocationMinX0),
                Convert.ToInt32(_RegionLocationMaxY1 - _RegionLocationMinY0));

            //  Intialize PixelSpacing
            //  If X,Y units are CM then PixelSpacing = DeltaX, DeltaY.
            //  Otherwise PixelSpacing = 0,0
            if ((_PhysicalUnitsXDirection == 3) && (_PhysicalUnitsYDirection == 3))
                thePixelSpacingMM = new PixelSpacing (PhysicalDeltaY * 10, PhysicalDeltaX * 10);
            else
                thePixelSpacingMM = new PixelSpacing(0,0);

            _isValid = true;
		}

        public Region(ClearCanvas.Dicom.DicomSequenceItem aDicomSequenceItem)
        {
            DicomAttribute aDicomAttribute;
            aDicomSequenceItem.TryGetAttribute(DicomTags.RegionSpatialFormat,out aDicomAttribute);
            _RegionSpatialFormat = aDicomAttribute.GetInt32(0,0);

            aDicomSequenceItem.TryGetAttribute(DicomTags.RegionDataType, out aDicomAttribute);
            _RegionDataType = aDicomAttribute.GetInt32(0, 0);

            aDicomSequenceItem.TryGetAttribute(DicomTags.RegionFlags, out aDicomAttribute);
            _RegionFlags = aDicomAttribute.GetInt64(0, 0);

            aDicomSequenceItem.TryGetAttribute(DicomTags.RegionLocationMinX0, out aDicomAttribute);
            _RegionLocationMinX0 = aDicomAttribute.GetInt64(0, 0);

            aDicomSequenceItem.TryGetAttribute(DicomTags.RegionLocationMinY0, out aDicomAttribute);
            _RegionLocationMinY0 = aDicomAttribute.GetInt64(0, 0);

            aDicomSequenceItem.TryGetAttribute(DicomTags.RegionLocationMaxX1, out aDicomAttribute);
            _RegionLocationMaxX1 = aDicomAttribute.GetInt64(0, 0);

            aDicomSequenceItem.TryGetAttribute(DicomTags.RegionLocationMaxY1, out aDicomAttribute);
            _RegionLocationMaxY1 = aDicomAttribute.GetInt64(0, 0);

            aDicomSequenceItem.TryGetAttribute(DicomTags.PhysicalUnitsXDirection, out aDicomAttribute);
            _PhysicalUnitsXDirection = aDicomAttribute.GetInt32(0, 0);

            aDicomSequenceItem.TryGetAttribute(DicomTags.PhysicalUnitsYDirection, out aDicomAttribute);
            _PhysicalUnitsYDirection = aDicomAttribute.GetInt32(0, 0);

            aDicomSequenceItem.TryGetAttribute(DicomTags.PhysicalDeltaX, out aDicomAttribute);
            _PhysicalDeltaX = aDicomAttribute.GetFloat64(0, 0);

            aDicomSequenceItem.TryGetAttribute(DicomTags.PhysicalDeltaY, out aDicomAttribute);
            _PhysicalDeltaY = aDicomAttribute.GetFloat64(0, 0);

            // Intialize Default Rectangle Values
            theRectangle = new Rectangle(
                Convert.ToInt32(_RegionLocationMinX0), 
                Convert.ToInt32(_RegionLocationMinY0), 
                Convert.ToInt32(_RegionLocationMaxX1 - _RegionLocationMinX0), 
                Convert.ToInt32(_RegionLocationMaxY1 - _RegionLocationMinY0));

            //  Initialize PixelSpacing (units = mm)
            //  PixelSpacing is non-zero only if PhysicalUnitX,Y are linear units (cm).
            if ((_PhysicalUnitsXDirection == 3) && (_PhysicalUnitsYDirection == 3))
                thePixelSpacingMM = new PixelSpacing (PhysicalDeltaY * 10, PhysicalDeltaX * 10);
            else
                thePixelSpacingMM = new PixelSpacing(0,0);

            _isValid = true;
        }


		/// <summary>
		/// Protected constructor.
		/// </summary>
		protected Region()
		{
		}

		#region Public Properties

		/// <summary>
		/// Gets whether or not this object represents a null value.
		/// </summary>
		public bool IsNull
		{
            get { return _isValid; }
		}

		/// <summary>
        /// Gets the spacing of the rows in the image. Units specified by PhysicalUnitsXDirection.
		/// </summary>
        public virtual double PhysicalDeltaY
        {
            get { return _PhysicalDeltaY; }
            protected set { PhysicalDeltaY = value; }
        }

		/// <summary>
        /// Gets the spacing of the columns in the image. Units specified by PhysicalUnitsYDirection.
		/// </summary>
        public virtual double PhysicalDeltaX
        {
            get { return _PhysicalDeltaX; }
            protected set { _PhysicalDeltaX = value; }
		}

        /// <summary>
        /// Gets the region offset.
        /// </summary>
        public virtual long RegionLocationMinX0
        {
            get { return _RegionLocationMinX0; }
            protected set { _RegionLocationMinX0 = value; }
        }

        /// <summary>
        /// Gets the region offset.
        /// </summary>
        public virtual long RegionLocationMaxX1
        {
            get { return _RegionLocationMaxX1; }
            protected set { _RegionLocationMaxX1 = value; }
        }

        /// <summary>
        /// Gets the region offset.
        /// </summary>
        public virtual long RegionLocationMinY0
        {
            get { return _RegionLocationMinY0; }
            protected set { _RegionLocationMinY0 = value; }
        }

        /// <summary>
        /// Gets the region offset.
        /// </summary>
        public virtual long RegionLocationMaxY1
        {
            get { return _RegionLocationMaxY1; }
            protected set { _RegionLocationMaxY1 = value; }
        }

        /// <summary>
        /// Gets the region pixel spacing (mm).
        /// </summary>
        public virtual PixelSpacing RegionPixelSpacing
        {
            get { return thePixelSpacingMM; }
        }

		/// <summary>
		/// Gets the pixel aspect ratio as a floating point value, or zero if <see cref="IsNull"/> is true.
		/// </summary>
		/// <remarks>
		/// The aspect ratio of a pixel is defined as the ratio of it's vertical and horizontal
		/// size(s), or <see cref="Row"/> divided by <see cref="Column"/>.
		/// </remarks>
		public double AspectRatio
		{
			get
			{
				if (IsNull)
					return 0;

                return _PhysicalDeltaY / _PhysicalDeltaX;
			}
		}

		#endregion

		#region Public Methods

        public bool HitTest(float x, float y)
        {
            Point aPoint = new Point((int)x, (int)y);
            return HitTest(aPoint);
        }

        public bool HitTest(int x, int y)
        {
            Point aPoint = new Point(x, y);
            return HitTest(aPoint);
        }

        public bool HitTest(Point aPoint)
        {
            bool bResult = theRectangle.Contains(aPoint);
            if (bResult)
            {
                xMeasurement = (aPoint.X - theRectangle.Left) * _PhysicalDeltaX * 0.0001F;  // Centimeter
                yMeasurement = (aPoint.Y - theRectangle.Top) * _PhysicalDeltaY * 0.0001F;  // Centimeter
            }
            return bResult;
        }

        ///// <summary>
        ///// Gets a string suitable for direct insertion into a <see cref="DicomAttributeMultiValueText"/> attribute.
        ///// </summary>
        //public override string ToString()
        //{
        //    return String.Empty;
        //    //return String.Format(@"{0:G12}\{1:G12}", _PhysicalDeltaY, _PhysicalDeltaX);
        //}

        //public static Region FromString(string multiValuedString)
        //{
        //    //double[] values;
        //    //if (DicomStringHelper.TryGetDoubleArray(multiValuedString, out values) && values.Length == 11)
        //    //    return new Region(values[0], values[1]);

        //    return null;
        //}

        //#region IEquatable<PixelSpacing> Members

        //public bool Equals(PixelSpacing other)
        //{
        //    if (other == null)
        //        return false;

        //    return _row == other._row && _column == other._column;
        //}

        //#endregion

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			return this.Equals(obj as Region);
		}

		/// <summary>
		/// Serves as a hash function for a particular type. <see cref="M:System.Object.GetHashCode"></see> is suitable for use in hashing algorithms and data structures like a hash table.
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"></see>.
		/// </returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion
	}
}
