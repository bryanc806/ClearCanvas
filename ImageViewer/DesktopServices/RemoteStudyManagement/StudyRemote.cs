using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ServiceModel;
using System.Security.Principal;
using ClearCanvas.Common;
using ClearCanvas.Dicom.ServiceModel.Query;
using ClearCanvas.ImageViewer.Common.WorkItem;
using ClearCanvas.ImageViewer.Common.RemoteStudyManagement;

namespace ClearCanvas.ImageViewer.DesktopServices.RemoteStudyManagement
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, UseSynchronizationContext = false, ConfigurationName = "StudyRemote", Namespace = ImageViewerStudyRemoteNamespace.Value)]
    public class StudyRemote : IStudyRemote
    {
        public string ServiceTest(string studyUid)
        {
            return studyUid;
        }

        public RemoteStudyInsertResponse Insert(string serverName, string studyUid)
        {
            if (String.IsNullOrEmpty(serverName)) serverName = "DefaultServer";

            StudyRootStudyIdentifier study = new StudyRootStudyIdentifier();
            study.StudyInstanceUid = studyUid;
            WorkItemRequest insertRequest = new DicomRetrieveStudyRequest
                {
                    ServerName = serverName,
                    Study = new WorkItemStudy(study),
                    Patient = new WorkItemPatient(study)

                };
            DicomRetrieveProgress progress = new DicomRetrieveProgress();

            WorkItemInsertResponse response = null;

            // Set the user name for retrive the study
            insertRequest.UserName = GetUserName();


            Platform.GetService<IWorkItemService>(s => response = s.Insert(new WorkItemInsertRequest { Request = insertRequest, Progress = progress }));

            // TODO (CR Jun 2012): The passed-in WorkItem contract should not be updated;
            // it should be done by the service and a new instance returned, or something should be returned by this
            // method to let the caller decide what to do.

            RemoteStudyInsertResponse result = new RemoteStudyInsertResponse();
            result.Identifier = response.Item.Identifier;
            return result;
        }

        public RemoteStudyUpdateResponse Update(RemoteStudyUpdateRequest request)
        {
            WorkItemUpdateResponse response = null;

            // Reset the status (Refer to the method in the IWorItemService
            Platform.GetService<IWorkItemService>(s => response = s.Update(new WorkItemUpdateRequest
            {
                Status = WorkItemStatusEnum.Pending,
                ProcessTime = Platform.Time,
                Identifier = request.Identifier
            }));

            RemoteStudyUpdateResponse result = new RemoteStudyUpdateResponse();
            result.Result = 0;

            return result;
        }

        public RemoteStudyQueryResponse Query(long identifier, string studyUid)
        {
            WorkItemQueryRequest queryRequest = new WorkItemQueryRequest();
            if (identifier > 0) queryRequest.Identifier = identifier;
            queryRequest.StudyInstanceUid = studyUid;

            WorkItemQueryResponse queryResponse = null;
            Platform.GetService<IWorkItemService>(s => queryResponse = s.Query(queryRequest));

            RemoteStudyQueryResponse response = new RemoteStudyQueryResponse();

            var results = new List<RemoteStudyQueryResponseItem>();
            foreach (WorkItemData d in queryResponse.Items)
            {
                //
                // Filter the invalid results
                //

                // The input identifier should bigger than the Insert indentifier
                if (d.Identifier < identifier) continue;
                // The user name should be the same as we send the Insert().
                if (d.Request == null) continue;
                if (!String.IsNullOrEmpty(d.Request.UserName)
                    && !String.Equals(d.Request.UserName, GetUserName(), StringComparison.OrdinalIgnoreCase))
                    continue;

                // Typically, there will be two records with type as: DicomRetrieve / ProcessStudy
                results.Add(StudyRemoteHelper.CovertWorkItemData(d));
            }

            response.Items = results;

            return response;
        }

        public RemoteStudyInsertResponse InsertSeries(string serverName, string studyUid, string seriesUid)
        {
            if (String.IsNullOrEmpty(serverName)) serverName = "DefaultServer";

            StudyRootStudyIdentifier study = new StudyRootStudyIdentifier();
            study.StudyInstanceUid = studyUid;
            List<String> seriesList = new List<string>();
            seriesList.Add(seriesUid);

            WorkItemRequest insertRequest = new DicomRetrieveSeriesRequest
            {
                ServerName = serverName,
                Study = new WorkItemStudy(study),
                Patient = new WorkItemPatient(study),
                SeriesInstanceUids = seriesList
            };
            DicomRetrieveProgress progress = new DicomRetrieveProgress();

            WorkItemInsertResponse response = null;

            // Set the user name for retrive the study
            insertRequest.UserName = GetUserName();


            Platform.GetService<IWorkItemService>(s => response = s.Insert(new WorkItemInsertRequest { Request = insertRequest, Progress = progress }));

            // TODO (CR Jun 2012): The passed-in WorkItem contract should not be updated;
            // it should be done by the service and a new instance returned, or something should be returned by this
            // method to let the caller decide what to do.

            RemoteStudyInsertResponse result = new RemoteStudyInsertResponse();
            result.Identifier = response.Item.Identifier;
            return result;
        }

        private static string GetUserName()
        {
            IPrincipal p = Thread.CurrentPrincipal;
            if (p == null || string.IsNullOrEmpty(p.Identity.Name))
                return string.Format("{0}@{1}", Environment.UserName, Environment.UserDomainName);
            return p.Identity.Name;
        }
    }
}
