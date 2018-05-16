using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClearCanvas.Common;
using ClearCanvas.ImageViewer.Common.RemoteStudyManagement;
using System.Threading;

namespace ClearCanvas.ImageViewer.DesktopServices.RemoteStudyManagement
{
    [ExtensionOf(typeof(ServiceProviderExtensionPoint))]
    public class StudyRemoteServiceProvider : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IStudyRemote))
                return new StudyRemoteProxy();

            return null;
        }
    }

    public class StudyRemoteProxy : IStudyRemote, IDisposable
    {

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        public string ServiceTest(string studyUid)
        {
            // Done for reasons of speed, as well as the fact that a call to the service from the same thread
            // that the service is hosted on (the main UI thread) will cause a deadlock.
            if (SynchronizationContext.Current == StudyRemoteServiceHostTool.HostSynchronizationContext)
            {
                return new StudyRemote().ServiceTest(studyUid);
            }
            else
            {
                using (StudyRemoteServiceClient client = new StudyRemoteServiceClient())
                {
                    return client.ServiceTest(studyUid);
                }
            }
        }


        public RemoteStudyInsertResponse Insert(string serverName, string studyUid)
        {
            if (SynchronizationContext.Current == StudyRemoteServiceHostTool.HostSynchronizationContext)
            {
                return new StudyRemote().Insert(serverName, studyUid);
            }
            else
            {
                using (StudyRemoteServiceClient client = new StudyRemoteServiceClient())
                {
                    return client.Insert(serverName, studyUid);
                }
            }
        }

        public RemoteStudyUpdateResponse Update(RemoteStudyUpdateRequest request)
        {
            if (SynchronizationContext.Current == StudyRemoteServiceHostTool.HostSynchronizationContext)
            {
                return new StudyRemote().Update(request);
            }
            else
            {
                using (StudyRemoteServiceClient client = new StudyRemoteServiceClient())
                {
                    return client.Update(request);
                }
            }
        }

        public RemoteStudyQueryResponse Query(long identifier, string studyUid)
        {
            if (SynchronizationContext.Current == StudyRemoteServiceHostTool.HostSynchronizationContext)
            {
                return new StudyRemote().Query(identifier, studyUid);
            }
            else
            {
                using (StudyRemoteServiceClient client = new StudyRemoteServiceClient())
                {
                    return client.Query(identifier, studyUid);
                }
            }
        }


        public RemoteStudyInsertResponse InsertSeries(string serverName, string studyUid, string seriesUid)
        {
            if (SynchronizationContext.Current == StudyRemoteServiceHostTool.HostSynchronizationContext)
            {
                return new StudyRemote().InsertSeries(serverName, studyUid, seriesUid);
            }
            else
            {
                using (StudyRemoteServiceClient client = new StudyRemoteServiceClient())
                {
                    return client.InsertSeries(serverName, studyUid, seriesUid);
                }
            }
        }
    }
}
