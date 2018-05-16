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
using System.ServiceModel;
using ClearCanvas.Common;
using ClearCanvas.Dicom.ServiceModel.Query;
using ClearCanvas.ImageViewer.Common.ServerDirectory;
using ClearCanvas.ImageViewer.Common.StudyManagement;
using ClearCanvas.ImageViewer.Common.DicomServer;
using ClearCanvas.ImageViewer.Common;

namespace ClearCanvas.ImageViewer.Configuration
{
	//Configuration is not the right place for this, but it was in a lonely plugin
	//all by itself, which seems ridiculous.  Badly need to do some code reorg and refactoring.

	//TODO (Marmot): Move to Common?

	[ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall, UseSynchronizationContext = false, ConfigurationName = "StudyLocator", Namespace = QueryNamespace.Value)]
	public class StudyLocator : IStudyLocator, IStudyRootQuery
	{
		private delegate IList<T> QueryDelegate<T>(T criteria, IStudyRootQuery query) where T : Identifier;

		private class GenericQuery<T> where T : Identifier, new()
		{
			private readonly QueryDelegate<T> _query;
			private readonly bool _suppressQueryFailureFaults;

			public GenericQuery(QueryDelegate<T> query, bool suppressQueryFailureFaults)
			{
				_query = query;
				_suppressQueryFailureFaults = suppressQueryFailureFaults;
			}

			public IList<T> Query(T queryCriteria)
			{
				LocateFailureInfo[] failures;
				return Query(queryCriteria, out failures);
			}

			public IList<T> Query(T queryCriteria, out LocateFailureInfo[] failures)
			{
				if (queryCriteria == null)
				{
					const string message = "The argument cannot be null.";
					Platform.Log(LogLevel.Error, message);
					throw new FaultException(message);
				}

				var results = new List<T>();
				var failureList = new List<LocateFailureInfo>();
				try
				{
					foreach (var priorsServer in ServerDirectory.GetPriorsServers(true))
					{
						try
						{
							IList<T> r = null;
							priorsServer.GetService<IStudyRootQuery>(service => r = _query(queryCriteria, service));
							results.AddRange(r);
						}
						catch (Exception e)
						{
							QueryFailedFault fault = new QueryFailedFault();
							fault.Description = String.Format("Failed to query server {0}.", priorsServer.Name);
							Platform.Log(LogLevel.Error, e, fault.Description);
							if (_suppressQueryFailureFaults)
								failureList.Add(new LocateFailureInfo(fault, fault.Description) {ServerName = priorsServer.Name, ServerAE = priorsServer.AETitle});
							else
								throw new FaultException<QueryFailedFault>(fault, fault.Description);
						}
					}
				}
				catch (FaultException)
				{
					throw;
				}
				catch (Exception e)
				{
					QueryFailedFault fault = new QueryFailedFault();
					fault.Description = String.Format("An unexpected error has occurred.");
					Platform.Log(LogLevel.Error, e, fault.Description);
					throw new FaultException<QueryFailedFault>(fault, fault.Description);
				}

				failures = failureList.ToArray();
				return results;
			}
		}

		#region IStudyRootQuery Members

		public IList<StudyRootStudyIdentifier> StudyQuery(StudyRootStudyIdentifier queryCriteria)
		{
			QueryDelegate<StudyRootStudyIdentifier> query = (criteria, studyRootQuery) => studyRootQuery.StudyQuery(criteria);

			return new GenericQuery<StudyRootStudyIdentifier>(query, false).Query(queryCriteria);
		}

        /// <summary>
        /// Evan: Query study to remote server.
        /// </summary>
        /// <param name="queryCriteria"></param>
        /// <param name="dicomServerName"></param>
        /// <returns></returns>
        public IList<StudyRootStudyIdentifier> StudyQuery2(StudyRootStudyIdentifier queryCriteria, string dicomServerName)
        {
            DicomServerConfiguration localConfig = DicomServer.GetConfiguration();
            if (String.IsNullOrEmpty(dicomServerName)) dicomServerName = "DefaultServer";
            IDicomServiceNode serviceNode = ServerDirectory.GetRemoteServerByName(dicomServerName);

            DicomStudyRootQuery dicomStudyRooteQuery = new DicomStudyRootQuery(localConfig.AETitle, serviceNode.AETitle, serviceNode.ScpParameters.HostName, serviceNode.ScpParameters.Port);
            return dicomStudyRooteQuery.StudyQuery(queryCriteria);
        } 

		public IList<SeriesIdentifier> SeriesQuery(SeriesIdentifier queryCriteria)
		{
			QueryDelegate<SeriesIdentifier> query = (criteria, studyRootQuery) => studyRootQuery.SeriesQuery(criteria);

			return new GenericQuery<SeriesIdentifier>(query, false).Query(queryCriteria);
		}

        /// <summary>
        /// Evan: Query Series to remote server.
        /// </summary>
        /// <param name="queryCriteria"></param>
        /// <param name="dicomServerName"></param>
        /// <returns></returns>
        public IList<SeriesIdentifier> SeriesQuery2(SeriesIdentifier queryCriteria, string dicomServerName)
        {
            DicomServerConfiguration localConfig = DicomServer.GetConfiguration();
            // ???RJS For our purposes the configuration should not assume "DefaultServer" the server name must be provided.
            if (String.IsNullOrEmpty(dicomServerName)) dicomServerName = "DefaultServer";
            IDicomServiceNode serviceNode = ServerDirectory.GetRemoteServerByName(dicomServerName);

            DicomStudyRootQuery dicomStudyRooteQuery = new DicomStudyRootQuery(localConfig.AETitle, serviceNode.AETitle, serviceNode.ScpParameters.HostName, serviceNode.ScpParameters.Port);
            return dicomStudyRooteQuery.SeriesQuery(queryCriteria);
        } 

        /// <summary>
        /// Evan: Performs a Image level query with the path within specified series.
        /// </summary>
        /// <param name="queryCriteria"></param>
        /// <returns></returns>
        public IList<ImageFile> ImageQueryWithPath(ImageIdentifier queryCriteria)
        {
            // Define the return values
            IList<ImageFile> imageFiles = new List<ImageFile>();

            IList<ImageIdentifier> images = ImageQuery(queryCriteria);
            if (images == null || images.Count == 0) return imageFiles;

            // Get the study folder
            string storeDir = StudyStore.FileStoreDirectory;
            string studyFolder = System.IO.Path.Combine(storeDir, queryCriteria.StudyInstanceUid);

            // Conver the images with files
            foreach (ImageIdentifier image in images)
            {
                ImageFile file = new ImageFile(image);
                imageFiles.Add(file);
                // Refer to ClearCanvas.ImageViewer.StudyManagement.Core.Storage.StudyLocation
                string imagePath = System.IO.Path.Combine(studyFolder, String.Format("{0}.{1}", image.SopInstanceUid, "dcm"));

                file.SetPath(studyFolder, imagePath);
            }

            return imageFiles;
        }

		public IList<ImageIdentifier> ImageQuery(ImageIdentifier queryCriteria)
		{
			QueryDelegate<ImageIdentifier> query = (criteria, studyRootQuery) => studyRootQuery.ImageQuery(criteria);

			return new GenericQuery<ImageIdentifier>(query, false).Query(queryCriteria);
		}

		#endregion

		#region Implementation of IStudyLocator

		public LocateStudiesResult LocateStudies(LocateStudiesRequest request)
		{
			QueryDelegate<StudyRootStudyIdentifier> query = (criteria, studyRootQuery) => studyRootQuery.StudyQuery(criteria);

			LocateFailureInfo[] failures;
			var results = new GenericQuery<StudyRootStudyIdentifier>(query, true).Query(request.Criteria, out failures);
			return new LocateStudiesResult {Studies = results, Failures = failures};
		}

		public LocateSeriesResult LocateSeries(LocateSeriesRequest request)
		{
			QueryDelegate<SeriesIdentifier> query = (criteria, studyRootQuery) => studyRootQuery.SeriesQuery(criteria);

			LocateFailureInfo[] failures;
			var results = new GenericQuery<SeriesIdentifier>(query, true).Query(request.Criteria, out failures);
			return new LocateSeriesResult {Series = results, Failures = failures};
		}

		public LocateImagesResult LocateImages(LocateImagesRequest request)
		{
			QueryDelegate<ImageIdentifier> query = (criteria, studyRootQuery) => studyRootQuery.ImageQuery(criteria);

			LocateFailureInfo[] failures;
			var results = new GenericQuery<ImageIdentifier>(query, true).Query(request.Criteria, out failures);
			return new LocateImagesResult {Images = results, Failures = failures};
		}

		#endregion
	}
}