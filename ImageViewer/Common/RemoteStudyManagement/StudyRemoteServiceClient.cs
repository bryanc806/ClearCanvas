using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace ClearCanvas.ImageViewer.Common.RemoteStudyManagement
{
    public class StudyRemoteServiceClient : ClientBase<IStudyRemote>, IStudyRemote
    {
        public StudyRemoteServiceClient()
        {
        }

        public StudyRemoteServiceClient(string endpointConfigurationName)
            : base(endpointConfigurationName)
        {
        }

        public StudyRemoteServiceClient(Binding binding, EndpointAddress remoteAddress)
            : base(binding, remoteAddress)
        {
        }

        public StudyRemoteServiceClient(string endpointConfigurationName, EndpointAddress remoteAddress)
            : base(endpointConfigurationName, remoteAddress)
        {
        }

        #region IStudyRemote Members

        public string ServiceTest(string studyUid)
        {
            return base.Channel.ServiceTest(studyUid);
        }

        public RemoteStudyInsertResponse Insert(string serverName, string studyUid)
        {
            return base.Channel.Insert(serverName, studyUid);
        }

        public RemoteStudyUpdateResponse Update(RemoteStudyUpdateRequest request)
        {
            return base.Channel.Update(request);
        }

        public RemoteStudyQueryResponse Query(long identifier, string studyUid)
        {
            return base.Channel.Query(identifier, studyUid);
        }

        // Interfaces for Series

        public RemoteStudyInsertResponse InsertSeries(string serverName, string studyUid, string seriesUid)
        {
            return base.Channel.InsertSeries(serverName, studyUid, seriesUid);
        }

        #endregion
    }
}
