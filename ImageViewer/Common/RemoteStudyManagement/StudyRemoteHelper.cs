using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClearCanvas.ImageViewer.Common.WorkItem;

namespace ClearCanvas.ImageViewer.Common.RemoteStudyManagement
{
    public class StudyRemoteHelper
    {
        public static RemoteStudyQueryResponseItem CovertWorkItemData(WorkItemData input)
        {
            RemoteStudyQueryResponseItem data = new RemoteStudyQueryResponseItem();
            if (input == null) return data;

            data.Identifier = input.Identifier;
            data.Priority = input.Priority.ToString();
            data.Status = input.Status.ToString();
            data.Type = input.Type;
            data.StudyInstanceUid = input.StudyInstanceUid;
            data.ProcessTime = input.ProcessTime;
            data.ScheduledTime = input.ScheduledTime;
            data.RequestedTime = input.RequestedTime;
            data.ExpirationTime = input.ExpirationTime;
            data.DeleteTime = input.DeleteTime;
            data.FailureCount = input.FailureCount;
            data.RetryStatus = input.RetryStatus;

            // Load the progress info
            if (input.Progress != null)
            {
                data.Progress_IsCancelable = input.Progress.IsCancelable;
                data.Progress_PercentComplete = input.Progress.PercentComplete;
                data.Progress_PercentFailed = input.Progress.PercentFailed;
                data.Progress_Status = input.Progress.Status;
                data.Progress_StatusDetails = input.Progress.StatusDetails;
            }

            // Load the request info
            if (input.Request != null)
            {
                data.Request_ActivityDescription = input.Request.ActivityDescription;
                data.Request_ActivityTypeString = input.Request.ActivityTypeString;
                data.Request_CancellationCanResultInPartialStudy = input.Request.CancellationCanResultInPartialStudy;
                data.Request_ConcurrencyType = input.Request.ConcurrencyType.ToString();
                data.Request_Priority = input.Request.Priority.ToString();
                data.Request_UserName = input.Request.UserName;
                data.Request_WorkItemType = input.Request.WorkItemType;
            }

            return data;
        }
    }
}
