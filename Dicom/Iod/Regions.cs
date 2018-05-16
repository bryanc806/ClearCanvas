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
using System.Collections.Generic;

namespace ClearCanvas.Dicom.Iod
{
	/// <summary>
	/// Represents the set of DICOM Regions.
    /// C.8.5.5 US Region Calibration Module
    /// see ClearCanvas.Dicom.Iod.Region for the actual class that support a region.
	/// </summary>
	public class Regions //: IEquatable<PixelSpacing>
    {
		#region Private Members

        private Region[] _Regions=null;

		#endregion
		
		/// <summary>
		/// Constructor.
		/// </summary>
        public Regions(ClearCanvas.Dicom.DicomSequenceItem[] aDicomSequenceItem)
		{
            _Regions = new Region[aDicomSequenceItem.Length];
            for (int i = 0; i < aDicomSequenceItem.Length; i++)
            {
              _Regions[i] = new Dicom.Iod.Region(aDicomSequenceItem[i]);
            }
		}
 
		/// <summary>
		/// Protected constructor.
		/// </summary>
		protected Regions()
		{
		}

		#region Public Properties


        public Region[] theRegions
        {
           get {return _Regions; }
        }

		/// <summary>
		/// Gets whether or not this object represents a null value.
		/// </summary>
		public bool IsNull
		{
            get { return (_Regions == null ); }
		}

        #endregion

        #region Public Methods
        public override bool Equals(object obj)
		{
			if (obj == null)
				return false;

			return this.Equals(obj as Regions);
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
