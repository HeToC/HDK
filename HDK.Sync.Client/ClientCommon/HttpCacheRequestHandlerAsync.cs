﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Synchronization.ClientServices.Common;
using Microsoft.Synchronization.Services.Formatters;

namespace Microsoft.Synchronization.ClientServices
{
    /// <summary>
    /// A Http transport implementation for processing a CachedRequest.
    /// </summary>
    internal class HttpCacheRequestHandler
    {
        private readonly Uri baseUri;
        private readonly ICredentials credentials;
        private readonly Type[] knownTypes;
        private readonly string scopeName;
        private readonly Dictionary<string, string> scopeParameters;
        private readonly SerializationFormat serializationFormat;

        private SyncReader syncReader;
        private SyncWriter syncWriter;

        public HttpCacheRequestHandler(Uri serviceUri, CacheControllerBehavior behaviors)
        {
            baseUri = serviceUri;
            serializationFormat = behaviors.SerializationFormat;
            scopeName = behaviors.ScopeName;
            credentials = behaviors.Credentials;
            knownTypes = new Type[behaviors.KnownTypes.Count];
            behaviors.KnownTypes.CopyTo(knownTypes, 0);
            scopeParameters = new Dictionary<string, string>(behaviors.ScopeParametersInternal);
        }

        protected SerializationFormat SerializationFormat
        {
            get { return serializationFormat; }
        }

        protected string ScopeName
        {
            get { return scopeName; }
        }

        protected Uri BaseUri
        {
            get { return baseUri; }
        }


        /// <summary>
        /// Called by the CacheController when it wants this CacheRequest to be processed asynchronously.
        /// </summary>
        /// <param name="request">CacheRequest to be processed</param>
        /// <param name="state">User state object</param>
        /// <param name="cancellationToken"> </param>
        public async Task<CacheRequestResult> ProcessCacheRequestAsync(CacheRequest request, object state,
                                                                       CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();

            var wrapper = new AsyncArgsWrapper
                              {
                                  UserPassedState = state,
                                  CacheRequest = request
                              };

            wrapper = await ProcessRequest(wrapper, cancellationToken);

            CacheRequestResult cacheRequestResult;

            if (wrapper.CacheRequest.RequestType == CacheRequestType.UploadChanges)
            {
                cacheRequestResult =
                    new CacheRequestResult(
                        wrapper.CacheRequest.RequestId,
                        wrapper.UploadResponse,
                        wrapper.CacheRequest.Changes.Count,
                        wrapper.Error,
                        wrapper.UserPassedState);
            }
            else
            {
                cacheRequestResult =
                    new CacheRequestResult(
                        wrapper.CacheRequest.RequestId,
                        wrapper.DownloadResponse,
                        wrapper.Error,
                        wrapper.UserPassedState);
            }
            return cacheRequestResult;
        }


        /// <summary>
        /// Method that does the actual processing. 
        /// 1. It first creates an HttpWebRequest
        /// 2. Fills in the required method type and parameters.
        /// 3. Attaches the user specified ICredentials.
        /// 4. Serializes the input params (Server blob for downloads and input feed for uploads)
        /// 5. If user has specified an BeforeSendingRequest callback then invokes it
        /// 6. Else proceeds to issue the request
        /// </summary>
        /// <param name="wrapper">AsyncArgsWrapper object</param>
        /// <param name="cancellationToken"> </param>
        private async Task<AsyncArgsWrapper> ProcessRequest(AsyncArgsWrapper wrapper,
                                                            CancellationToken cancellationToken)
        {
            HttpWebResponse webResponse = null;
            try
            {
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                var requestUri = new StringBuilder();
                requestUri.AppendFormat("{0}{1}{2}/{3}",
                                        BaseUri,
                                        (BaseUri.ToString().EndsWith("/")) ? string.Empty : "/",
                                        Uri.EscapeUriString(ScopeName),
                                        wrapper.CacheRequest.RequestType.ToString());

                string prefix = "?";
                // Add the scope params if any
                foreach (var kvp in scopeParameters)
                {
                    requestUri.AppendFormat("{0}{1}={2}", prefix, Uri.EscapeUriString(kvp.Key),
                                            Uri.EscapeUriString(kvp.Value));
                    if (prefix.Equals("?"))
                        prefix = "&";
                }

                // Create the WebRequest
                HttpWebRequest webRequest;

                if (credentials != null)
                {
                    // Create the Client Http request
                    webRequest = WebRequest.CreateHttp(new Uri(requestUri.ToString()));
                    // Add credentials
                    webRequest.Credentials = credentials;
                }
                else
                {
                    // Use WebRequest.Create the request. This uses any user defined prefix preferences for certain paths
                    webRequest = (HttpWebRequest)WebRequest.Create(requestUri.ToString());
                }

                // Set the method type
                webRequest.Method = "POST";
                webRequest.Accept = (SerializationFormat == SerializationFormat.ODataAtom)
                                        ? "application/atom+xml"
                                        : "application/json";
                webRequest.ContentType = (SerializationFormat == SerializationFormat.ODataAtom)
                                             ? "application/atom+xml"
                                             : "application/json";


                // Write on the stream
#if !NETFX_CORE
                using (Stream stream = await Task.Factory.FromAsync<Stream>(
                                    webRequest.BeginGetRequestStream, 
                                    webRequest.EndGetRequestStream, null))
#else
                using (Stream stream = await webRequest.GetRequestStreamAsync())
#endif
                {
                    if (wrapper.CacheRequest.RequestType == CacheRequestType.UploadChanges)
                        WriteUploadRequestStream(stream, wrapper);
                    else
                        WriteDownloadRequestStream(stream, wrapper);
                }

                // If error, return wrapper with error
                if (wrapper.Error != null)
                    return wrapper;

                // Get Response
                if (wrapper.CacheRequest.RequestType == CacheRequestType.UploadChanges)
                    wrapper.UploadResponse = new ChangeSetResponse();
                else
                    wrapper.DownloadResponse = new ChangeSet();

#if !NETFX_CORE
                    webResponse = (HttpWebResponse) (await Task.Factory.FromAsync<WebResponse>(
                                                    webRequest.BeginGetResponse,
                                                    webRequest.EndGetResponse, null));
#else
                webResponse = (HttpWebResponse)(await webRequest.GetResponseAsync());
#endif

                if (wrapper.CacheRequest.RequestType == CacheRequestType.UploadChanges)
                    await ReadUploadResponse(webResponse, wrapper);
                else
                    await ReadDownloadResponse(webResponse, wrapper);
            }
            catch (WebException we)
            {
                if (we.Response == null)
                {
                    wrapper.Error = we;

                }
                else
                {

                    var stream = we.Response.GetResponseStream();

                    var reader = SerializationFormat == SerializationFormat.ODataAtom ?
                        XmlReader.Create(stream) :
                        new XmlJsonReader(stream, XmlDictionaryReaderQuotas.Max);

                    if (reader.ReadToDescendant(FormatterConstants.ErrorDescriptionElementNamePascalCasing))
                        wrapper.Error = new Exception(reader.ReadElementContentAsString());
                }
            }
            catch (OperationCanceledException)
            {
                // Re throw the operation cancelled
                throw;
            }
            catch (Exception e)
            {
                if (ExceptionUtility.IsFatal(e))
                    throw;

                wrapper.Error = e;
                return wrapper;
            }

            return wrapper;
        }

        /// <summary>
        /// Callback for the Upload HttpWebRequest.beginGetRequestStream
        /// </summary>
        private void WriteUploadRequestStream(Stream requestStream, AsyncArgsWrapper wrapper)
        {
            try
            {
                // Create a SyncWriter to write the contents
                this.syncWriter = (SerializationFormat == SerializationFormat.ODataAtom)
                    ? new ODataAtomWriter(BaseUri)
                    : (SyncWriter)new ODataJsonWriter(BaseUri);


                syncWriter.StartFeed(wrapper.CacheRequest.IsLastBatch, wrapper.CacheRequest.KnowledgeBlob ?? new byte[0]);

                foreach (IOfflineEntity entity in wrapper.CacheRequest.Changes)
                {
                    // Skip tombstones that dont have a ID element.
                    if (entity.GetServiceMetadata().IsTombstone && string.IsNullOrEmpty(entity.GetServiceMetadata().Id))
                        continue;

                    string tempId = null;

                    // Check to see if this is an insert. i.e ServiceMetadata.Id is null or empty
                    if (string.IsNullOrEmpty(entity.GetServiceMetadata().Id))
                    {
                        if (wrapper.TempIdToEntityMapping == null)
                            wrapper.TempIdToEntityMapping = new Dictionary<string, IOfflineEntity>();

                        tempId = Guid.NewGuid().ToString();
                        wrapper.TempIdToEntityMapping.Add(tempId, entity);
                    }

                    syncWriter.AddItem(entity, tempId);
                }

                if (SerializationFormat == SerializationFormat.ODataAtom)
                    syncWriter.WriteFeed(XmlWriter.Create(requestStream));
                else
                    this.syncWriter.WriteFeed(new XmlJsonWriter(requestStream));

                requestStream.Flush();
            }
            catch (Exception e)
            {
                if (ExceptionUtility.IsFatal(e))
                {
                    throw;
                }
                wrapper.Error = e;
            }
        }

        /// <summary>
        /// Callback for the Download HttpWebRequest.beginGetRequestStream
        /// </summary>
        private void WriteDownloadRequestStream(Stream requestStream, AsyncArgsWrapper wrapper)
        {
            try
            {
                // Create a SyncWriter to write the contents
                this.syncWriter = (SerializationFormat == SerializationFormat.ODataAtom)
                    ? new ODataAtomWriter(BaseUri)
                    : (SyncWriter)new ODataJsonWriter(BaseUri);

                //syncWriter = new ODataAtomWriter(BaseUri);

                syncWriter.StartFeed(wrapper.CacheRequest.IsLastBatch, wrapper.CacheRequest.KnowledgeBlob ?? new byte[0]);

                if (SerializationFormat == SerializationFormat.ODataAtom)
                    syncWriter.WriteFeed(XmlWriter.Create(requestStream));
                else
                    this.syncWriter.WriteFeed(new XmlJsonWriter(requestStream));

                requestStream.Flush();
            }
            catch (Exception e)
            {
                if (ExceptionUtility.IsFatal(e))
                    throw;

                wrapper.Error = e;
            }
        }

        /// <summary>
        /// Callback for the Upload HttpWebRequest.BeginGetResponse call
        /// </summary>
        private async Task ReadUploadResponse(HttpWebResponse response, AsyncArgsWrapper wrapper)
        {
            try
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = response.GetResponseStream();

                    this.syncReader = (SerializationFormat == SerializationFormat.ODataAtom)
                        ? new ODataAtomReader(responseStream, this.knownTypes)
                        : (SyncReader)new ODataJsonReader(responseStream, this.knownTypes);

                    // Read the response
                    await Task.Factory.StartNew(() =>
                    {
                        while (syncReader.Next())
                        {
                            switch (syncReader.ItemType)
                            {
                                case ReaderItemType.Entry:
                                    IOfflineEntity entity = syncReader.GetItem();
                                    IOfflineEntity ackedEntity = entity;
                                    string tempId = null;

                                    // If conflict only one temp ID should be set
                                    if (syncReader.HasTempId() && syncReader.HasConflictTempId())
                                    {
                                        throw new CacheControllerException(
                                            string.Format(
                                                "Service returned a TempId '{0}' in both live and conflicting entities.",
                                                syncReader.GetTempId()));
                                    }

                                    // Validate the live temp ID if any, before adding anything to the offline context
                                    if (syncReader.HasTempId())
                                    {
                                        tempId = syncReader.GetTempId();
                                        CheckEntityServiceMetadataAndTempIds(wrapper, entity, tempId);
                                    }

                                    //  If conflict 
                                    if (syncReader.HasConflict())
                                    {
                                        Conflict conflict = syncReader.GetConflict();
                                        IOfflineEntity conflictEntity = (conflict is SyncConflict)
                                                                            ? ((SyncConflict)conflict).LosingEntity
                                                                            : ((SyncError)conflict).ErrorEntity;

                                        // Validate conflict temp ID if any
                                        if (syncReader.HasConflictTempId())
                                        {
                                            tempId = syncReader.GetConflictTempId();
                                            CheckEntityServiceMetadataAndTempIds(wrapper, conflictEntity, tempId);
                                        }

                                        // Add conflict                                    
                                        wrapper.UploadResponse.AddConflict(conflict);

                                        //
                                        // If there is a conflict and the tempId is set in the conflict entity then the client version lost the 
                                        // conflict and the live entity is the server version (ServerWins)
                                        //
                                        if (syncReader.HasConflictTempId() && entity.GetServiceMetadata().IsTombstone)
                                        {
                                            //
                                            // This is a ServerWins conflict, or conflict error. The winning version is a tombstone without temp Id
                                            // so there is no way to map the winning entity with a temp Id. The temp Id is in the conflict so we are
                                            // using the conflict entity, which has the PK, to build a tombstone entity used to update the offline context
                                            //
                                            // In theory, we should copy the service metadata but it is the same end result as the service fills in
                                            // all the properties in the conflict entity
                                            //

                                            // Add the conflict entity                                              
                                            conflictEntity.GetServiceMetadata().IsTombstone = true;
                                            ackedEntity = conflictEntity;
                                        }
                                    }

                                    // Add ackedEntity to storage. If ackedEntity is still equal to entity then add non-conflict entity. 
                                    if (!String.IsNullOrEmpty(tempId))
                                    {
                                        wrapper.UploadResponse.AddUpdatedItem(ackedEntity);
                                    }
                                    break;

                                case ReaderItemType.SyncBlob:
                                    wrapper.UploadResponse.ServerBlob = syncReader.GetServerBlob();
                                    break;
                            }
                        }
                    });


                    if (wrapper.TempIdToEntityMapping != null && wrapper.TempIdToEntityMapping.Count != 0)
                    {
                        // The client sent some inserts which werent ack'd by the service. Throw.
                        var builder =
                            new StringBuilder(
                                "Server did not acknowledge with a permanent Id for the following tempId's: ");
                        builder.Append(string.Join(",", wrapper.TempIdToEntityMapping.Keys.ToArray()));
                        throw new CacheControllerException(builder.ToString());
                    }
                }
                else
                {
                    wrapper.UploadResponse.Error = new CacheControllerException(
                        string.Format("Remote service returned error status. Status: {0}, Description: {1}",
                                      response.StatusCode,
                                      response.StatusDescription));
                }
            }
            catch (Exception e)
            {
                if (ExceptionUtility.IsFatal(e))
                    throw;

                wrapper.Error = e;
            }
        }

        /// <summary>
        /// Check Metadata
        /// </summary>
        private void CheckEntityServiceMetadataAndTempIds(AsyncArgsWrapper wrapper, IOfflineEntity entity, string tempId)
        {
            // Check service ID 
            if (string.IsNullOrEmpty(entity.GetServiceMetadata().Id))
                throw new CacheControllerException(
                    string.Format("Service did not return a permanent Id for tempId '{0}'", tempId));

            // If an entity has a temp id then it should not be a tombstone                
            if (entity.GetServiceMetadata().IsTombstone)
                throw new CacheControllerException(string.Format(
                    "Service returned a tempId '{0}' in tombstoned entity.", tempId));

            // Check that the tempId was sent by client
            if (!wrapper.TempIdToEntityMapping.ContainsKey(tempId))
                throw new CacheControllerException(
                    "Service returned a response for a tempId which was not uploaded by the client. TempId: " + tempId);

            // Once received, remove the tempId from the mapping list.
            wrapper.TempIdToEntityMapping.Remove(tempId);
        }

        /// <summary>
        /// Callback for the Download HttpWebRequest.beginGetRequestStream. Deserializes the response feed to
        /// retrieve the list of IOfflineEntity objects and constructs an ChangeSet for that.
        /// </summary>
        private async Task ReadDownloadResponse(HttpWebResponse response, AsyncArgsWrapper wrapper)
        {
            try
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream responseStream = response.GetResponseStream();

                    // Create the SyncReader
                    this.syncReader = (SerializationFormat == SerializationFormat.ODataAtom)
                        ? new ODataAtomReader(responseStream, this.knownTypes)
                        : (SyncReader)new ODataJsonReader(responseStream, this.knownTypes);

                    await Task.Factory.StartNew(() =>
                    {
                        // Read the response
                        while (syncReader.Next())
                        {
                            switch (syncReader.ItemType)
                            {
                                case ReaderItemType.Entry:
                                    wrapper.DownloadResponse.AddItem(syncReader.GetItem());
                                    break;
                                case ReaderItemType.SyncBlob:
                                    wrapper.DownloadResponse.ServerBlob = syncReader.GetServerBlob();
                                    break;
                                case ReaderItemType.HasMoreChanges:
                                    wrapper.DownloadResponse.IsLastBatch = !syncReader.GetHasMoreChangesValue();
                                    break;
                            }
                        }

                    });

                }
                else
                {
                    wrapper.Error = new CacheControllerException(
                        string.Format("Remote service returned error status. Status: {0}, Description: {1}",
                                      response.StatusCode,
                                      response.StatusDescription));
                }
            }
            catch (Exception e)
            {
                if (ExceptionUtility.IsFatal(e))
                {
                    throw;
                }
                wrapper.Error = e;
            }
        }

        #region Nested type: AsyncArgsWrapper

        /// <summary>
        /// Wrapper class that holds multiple related arguments that is passed around from
        /// async call to its completion
        /// </summary>
        private class AsyncArgsWrapper
        {
            public CacheRequest CacheRequest;
            public ChangeSet DownloadResponse;
            public Exception Error;
            public Dictionary<string, IOfflineEntity> TempIdToEntityMapping;
            public ChangeSetResponse UploadResponse;
            public object UserPassedState;
        }

        #endregion
    }
}