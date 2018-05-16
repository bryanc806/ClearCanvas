#region Comments

// This class is defined from Image that with its file path
// It extends from ImageIndentifier

#endregion

using System;
using System.Runtime.Serialization;
using ClearCanvas.Dicom.Iod;

namespace ClearCanvas.Dicom.ServiceModel.Query
{
    public interface IImageFile : IImageIdentifier
    {
        [DicomField(DicomTags.InstanceNumber)]
        new int? InstanceNumber { get; }

        string ImageFilePath { get; }
        string StudyFolder { get; }
    }

    /// <summary>
    /// Query identifier for a composite object instance.
    /// </summary>
    [DataContract(Namespace = QueryNamespace.Value)]
    public class ImageFile : Identifier, IImageFile
    {
        #region Private Fields

        private int? _instanceNumber;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ImageFile()
        {
        }

        public ImageFile(IImageIdentifier other)
            : base(other)
        {
            CopyFrom(other);
            InstanceNumber = other.InstanceNumber;
        }

        public ImageFile(ISopInstanceData other, IIdentifier identifier)
            : base(identifier)
        {
            CopyFrom(other);
        }

        public ImageFile(ISopInstanceData other)
        {
            CopyFrom(other);
        }

        /// <summary>
        /// Creates an instance of <see cref="ImageIdentifier"/> from a <see cref="DicomAttributeCollection"/>.
        /// </summary>
        public ImageFile(DicomAttributeCollection attributes)
            : base(attributes)
        {
        }

        #endregion

        private void CopyFrom(ISopInstanceData other)
        {
            if (other == null)
                return;

            StudyInstanceUid = other.StudyInstanceUid;
            SeriesInstanceUid = other.SeriesInstanceUid;
            SopInstanceUid = other.SopInstanceUid;
            SopClassUid = other.SopClassUid;
            InstanceNumber = other.InstanceNumber;
        }

        public override string ToString()
        {
            return String.Format("{0} | {1}", InstanceNumber, SopInstanceUid);
        }

        #region Public Properties

        /// <summary>
        /// Gets the level of the query - IMAGE.
        /// </summary>
        public override string QueryRetrieveLevel
        {
            get { return "IMAGE"; }
        }

        /// <summary>
        /// Gets or sets the Study Instance Uid of the identified sop instance.
        /// </summary>
        [DicomField(DicomTags.StudyInstanceUid, CreateEmptyElement = true, SetNullValueIfEmpty = true)]
        [DataMember(IsRequired = true)]
        public string StudyInstanceUid { get; set; }

        /// <summary>
        /// Gets or sets the Series Instance Uid of the identified sop instance.
        /// </summary>
        [DicomField(DicomTags.SeriesInstanceUid, CreateEmptyElement = true, SetNullValueIfEmpty = true)]
        [DataMember(IsRequired = true)]
        public string SeriesInstanceUid { get; set; }

        /// <summary>
        /// Gets or sets the Sop Instance Uid of the identified sop instance.
        /// </summary>
        [DicomField(DicomTags.SopInstanceUid, CreateEmptyElement = true, SetNullValueIfEmpty = true)]
        [DataMember(IsRequired = true)]
        public string SopInstanceUid { get; set; }

        /// <summary>
        /// Gets or sets the Sop Class Uid of the identified sop instance.
        /// </summary>
        [DicomField(DicomTags.SopClassUid, CreateEmptyElement = true, SetNullValueIfEmpty = true)]
        [DataMember(IsRequired = true)]
        public string SopClassUid { get; set; }

        /// <summary>
        /// Gets or sets the Instance Number of the identified sop instance.
        /// </summary>
        [DicomField(DicomTags.InstanceNumber, CreateEmptyElement = true, SetNullValueIfEmpty = true)]
        [DataMember(IsRequired = true)]
        public int? InstanceNumber
        {
            get { return _instanceNumber; }
            set { _instanceNumber = value; }
        }

        int ISopInstanceData.InstanceNumber
        {
            get { return _instanceNumber ?? 0; }
        }

        #endregion

        #region Implementation for IImageFile

        /// <summary>
        /// Get the image full path
        /// </summary>
        [DataMember(IsRequired = true)]
        public string ImageFilePath { get; set; }

        /// <summary>
        /// Get the study folder
        /// </summary>
        [DataMember(IsRequired = true)]
        public string StudyFolder { get; set; }

        /// <summary>
        /// Set the path for the image files
        /// </summary>
        /// <param name="imagePath">The full image path</param>
        /// <param name="studyFolder">The root for the study</param>
        public void SetPath(string studyFolder, string imagePath)
        {
            StudyFolder = studyFolder;
            ImageFilePath = imagePath;
        }

        #endregion
    }
}