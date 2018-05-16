using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;
using ClearCanvas.Common.Serialization;
using ClearCanvas.ImageViewer.Common;
using ClearCanvas.ImageViewer.Common.WorkItem;
using ClearCanvas.Dicom.ServiceModel.Query;
using ClearCanvas.Dicom.Iod;
using ClearCanvas.Dicom;

namespace ClearCanvas.ImageViewer.Common.RemoteStudyManagement
{
    public static class ImageViewerStudyRemoteNamespace
    {
        public const string Value = ImageViewerNamespace.Value + "/StudyRemote";
    }

    [ServiceContract(SessionMode = SessionMode.Allowed, ConfigurationName = "IStudyRemote", Namespace = ImageViewerStudyRemoteNamespace.Value)]
    public interface IStudyRemote
    {
        /// <summary>
        /// Retrive study from server
        /// </summary>
        [OperationContract(IsOneWay = false)]
        string ServiceTest(string studyUid);

        [OperationContract(IsOneWay = false)]
        RemoteStudyInsertResponse Insert(string serverName, string studyUid);

        [OperationContract(IsOneWay = false)]
        RemoteStudyUpdateResponse Update(RemoteStudyUpdateRequest request);

        [OperationContract(IsOneWay = false)]
        RemoteStudyQueryResponse Query(long identifier, string studyUid);

        // Add interface for Series operation

        [OperationContract(IsOneWay = false)]
        RemoteStudyInsertResponse InsertSeries(string serverName, string studyUid, string seriesUid);
    }

    #region Service Models

    [DataContract(Namespace = ImageViewerStudyRemoteNamespace.Value)]
    public class RemoteStudyInsertResponse : DataContractBase
    {
        /// <summary>
        /// The Identifier for the WorkItem.
        /// </summary>
        [DataMember(IsRequired = true)]
        public long Identifier { get; set; }
    }

    [DataContract(Namespace = ImageViewerStudyRemoteNamespace.Value)]
    public class RemoteStudyUpdateRequest : DataContractBase
    {
        [DataMember(IsRequired = true)]
        public long Identifier { get; set; }

        [DataMember]
        public string Priority { get; set; }

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public DateTime? ProcessTime { get; set; }

        [DataMember]
        public DateTime? ExpirationTime { get; set; }

        [DataMember]
        public bool? Cancel { get; set; }

        [DataMember]
        public bool? Delete { get; set; }

    }

    [DataContract(Namespace = ImageViewerStudyRemoteNamespace.Value)]
    public class RemoteStudyUpdateResponse : DataContractBase
    {
        [DataMember]
        public int Result { get; set; }
    }

    [DataContract(Namespace = ImageViewerStudyRemoteNamespace.Value)]
    public class RemoteStudyQueryRequest : DataContractBase
    {
        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string Status { get; set; }

        [DataMember]
        public string StudyInstanceUid { get; set; }

        [DataMember]
        public long? Identifier { get; set; }
    }

    [DataContract(Namespace = ImageViewerStudyRemoteNamespace.Value)]
    public class RemoteStudyQueryResponse : DataContractBase
    {
        [DataMember]
        public IList<RemoteStudyQueryResponseItem> Items { get; set; }
    }

    [DataContract(Namespace = ImageViewerStudyRemoteNamespace.Value)]
    public class RemoteStudyQueryResponseItem : DataContractBase
    {
        /// <summary>
        /// The Identifier for the WorkItem.
        /// </summary>
        [DataMember(IsRequired = true)]
        public long Identifier { get; set; }

        /// <summary>
        /// The Priority of the WorkItem
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Priority { get; set; }

        /// <summary>
        /// The current status of the WorkItem
        /// </summary>
        [DataMember(IsRequired = true)]
        public string Status { get; set; }

        [DataMember(IsRequired = true)]
        public string Type { get; set; }

        [DataMember(IsRequired = false)]
        public string StudyInstanceUid { get; set; }

        [DataMember(IsRequired = false)]
        public DateTime ProcessTime { get; set; }

        [DataMember(IsRequired = false)]
        public DateTime RequestedTime { get; set; }

        [DataMember(IsRequired = false)]
        public DateTime ScheduledTime { get; set; }

        [DataMember(IsRequired = false)]
        public DateTime ExpirationTime { get; set; }

        [DataMember(IsRequired = false)]
        public DateTime? DeleteTime { get; set; }

        //TODO (CR Jun 2012) - This is stored as a smallint in the database, but modeled as an int, should probabaly change the DB.
        [DataMember(IsRequired = false)]
        public int FailureCount { get; set; }

        [DataMember(IsRequired = false)]
        public string RetryStatus {get; set; }

        #region Prgress Info

        [DataMember(IsRequired = false)]
        public string Progress_Status { get; set; }

        [DataMember(IsRequired = false)]
        public string Progress_StatusDetails { get; set; }

        [DataMember(IsRequired = false)]
        public virtual Decimal Progress_PercentComplete { get; set; }

        [DataMember(IsRequired = false)]
        public virtual Decimal Progress_PercentFailed { get; set; }

        [DataMember(IsRequired = false)]
        public bool Progress_IsCancelable { get; set; }

        #endregion

        #region Request Info

        [DataMember(IsRequired = false)]
        public string Request_ConcurrencyType { get; set; }

        [DataMember(IsRequired = false)]
        public string Request_Priority { get; set; }

        [DataMember(IsRequired = false)]
        public string Request_WorkItemType { get; set; }

        [DataMember(IsRequired = false)]
        public string Request_UserName { get; set; }

        [DataMember(IsRequired = false)]
        public string Request_ActivityDescription { get; set; }

        [DataMember(IsRequired = false)]
        public string Request_ActivityTypeString { get; set; }

        [DataMember(IsRequired = false)]
        public bool Request_CancellationCanResultInPartialStudy { get; set; }

        #endregion
    }
    

    #region WorkItem

    //[DataContract(Namespace = ImageViewerStudyRemoteNamespace.Value)]
    //public class RemoteStudyQueryResponse2 : DataContractBase
    //{
    //    [DataMember]
    //    public IEnumerable<RemoteWorkItemData> Items { get; set; }
    //}

    //[DataContract(Name = "RemoteWorkItemPriority", Namespace = ImageViewerStudyRemoteNamespace.Value)]
    //public enum RemoteWorkItemPriorityEnum
    //{
    //    [EnumMember]
    //    Stat = 1,
    //    [EnumMember]
    //    High = 2,
    //    [EnumMember]
    //    Normal = 3
    //}

    //[DataContract(Name = "RemoteWorkItemStatus", Namespace = ImageViewerStudyRemoteNamespace.Value)]
    //public enum RemoteWorkItemStatusEnum
    //{
    //    [EnumMember]
    //    Pending = 1,
    //    [EnumMember]
    //    InProgress = 2,
    //    [EnumMember]
    //    Complete = 3,
    //    [EnumMember]
    //    Idle = 4,
    //    [EnumMember]
    //    Deleted = 5,
    //    [EnumMember]
    //    Canceled = 6,
    //    [EnumMember]
    //    Failed = 7,
    //    [EnumMember]
    //    DeleteInProgress = 8,
    //    [EnumMember]
    //    Canceling = 9,
    //}

    ///// <summary>
    ///// Base WorkItem representing a unit of Work to be done.
    ///// </summary>
    //[DataContract(Namespace = ImageViewerStudyRemoteNamespace.Value)]
    //public class RemoteWorkItemData : DataContractBase
    //{
    //    /// <summary>
    //    /// The Identifier for the WorkItem.
    //    /// </summary>
    //    [DataMember(IsRequired = true)]
    //    public long Identifier { get; set; }

    //    /// <summary>
    //    /// The Priority of the WorkItem
    //    /// </summary>
    //    [DataMember(IsRequired = true)]
    //    public RemoteWorkItemPriorityEnum Priority { get; set; }

    //    /// <summary>
    //    /// The current status of the WorkItem
    //    /// </summary>
    //    [DataMember(IsRequired = true)]
    //    public RemoteWorkItemStatusEnum Status { get; set; }

    //    [DataMember(IsRequired = true)]
    //    public string Type { get; set; }

    //    [DataMember(IsRequired = false)]
    //    public string StudyInstanceUid { get; set; }

    //    [DataMember(IsRequired = true)]
    //    public DateTime ProcessTime { get; set; }

    //    [DataMember(IsRequired = true)]
    //    public DateTime RequestedTime { get; set; }

    //    [DataMember(IsRequired = true)]
    //    public DateTime ScheduledTime { get; set; }

    //    [DataMember(IsRequired = true)]
    //    public DateTime ExpirationTime { get; set; }

    //    [DataMember(IsRequired = false)]
    //    public DateTime? DeleteTime { get; set; }

    //    //TODO (CR Jun 2012) - This is stored as a smallint in the database, but modeled as an int, should probabaly change the DB.
    //    [DataMember(IsRequired = true)]
    //    public int FailureCount { get; set; }

    //    [DataMember(IsRequired = false)]
    //    public RemoteWorkItemRequest Request { get; set; }

    //    [DataMember(IsRequired = false)]
    //    public RemoteWorkItemProgress Progress { get; set; }

    //    [DataMember(IsRequired = false)]
    //    public string RetryStatus
    //    {
    //        get
    //        {
    //            if (FailureCount == 0 || Status != RemoteWorkItemStatusEnum.Pending)
    //                return string.Empty;

    //            return string.Format("{0} : {1}", FailureCount, ProcessTime.ToString("H:mm"));
    //        }
    //    }

    //    [DataMember(IsRequired = false)]
    //    public IPatientData Patient { get; set; }

    //    [DataMember(IsRequired = false)]
    //    public IStudyData Study { get; set; }
    //}

    //[DataContract(Namespace = ImageViewerStudyRemoteNamespace.Value)]
    //[WorkItemKnownType]
    //public abstract class RemoteWorkItemRequest : DataContractBase
    //{

    //    [DataMember]
    //    public RemoteWorkItemPriorityEnum Priority { get; set; }

    //    [DataMember]
    //    public string WorkItemType { get; set; }

    //    [DataMember]
    //    public string UserName { get; set; }

    //    public abstract string ActivityDescription { get; }

    //    public abstract string ActivityTypeString { get; }

    //    [DataMember]
    //    public bool CancellationCanResultInPartialStudy { get; protected set; }
    //}

    //[DataContract(Namespace = ImageViewerStudyRemoteNamespace.Value)]
    //[WorkItemKnownType]
    //public class RemoteWorkItemProgress : DataContractBase
    //{
    //    public virtual string Status { get { return string.Empty; } }

    //    [DataMember(IsRequired = false)]
    //    public string StatusDetails { get; set; }

    //    public virtual Decimal PercentComplete { get { return new decimal(0.0); } }

    //    public virtual Decimal PercentFailed { get { return new decimal(0.0); } }

    //    [DataMember(IsRequired = true)]
    //    public bool IsCancelable { get; set; }
    //}

    //[DataContract(Namespace = ImageViewerStudyRemoteNamespace.Value)]
    //[WorkItemKnownType]
    //public class RemoteWorkItemPatient : PatientRootPatientIdentifier
    //{
    //    public RemoteWorkItemPatient()
    //    { }

    //    public RemoteWorkItemPatient(IPatientData p)
    //        : base(p)
    //    { }

    //    public RemoteWorkItemPatient(DicomAttributeCollection c)
    //        : base(c)
    //    { }
    //}

    //[DataContract(Namespace = ImageViewerStudyRemoteNamespace.Value)]
    //[WorkItemKnownType]
    //public class RemoteWorkItemStudy : StudyIdentifier
    //{
    //    public RemoteWorkItemStudy()
    //    { }

    //    public RemoteWorkItemStudy(IStudyData s)
    //        : base(s)
    //    { }

    //    public RemoteWorkItemStudy(DicomAttributeCollection c)
    //        : base(c)
    //    {
    //        string modality = c[DicomTags.Modality].ToString();
    //        ModalitiesInStudy = new[] { modality };
    //    }
    //}

    #endregion

    #endregion
}
