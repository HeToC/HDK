using System;
using System.Net;
using System.ComponentModel;
using System.Threading;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Synchronization.ClientServices.Common;

namespace Microsoft.Synchronization.ClientServices
{
    /// <summary>
    /// Class used for synchronizing an offline cache with a remote sync service.
    /// </summary>
    public class CacheController
    {
        private OfflineSyncProvider localProvider;
        private Uri serviceUri;
        private CacheControllerBehavior controllerBehavior;
        private HttpCacheRequestHandler cacheRequestHandler;
        private Guid changeSetId;
        private object lockObject = new object(); // Object used for locking access to the cancelled flag
        private bool beginSessionComplete;

        /// <summary>
        /// Returns the reference to the CacheControllerBehavior object that can be used to 
        /// customize the CacheController's settings.
        /// </summary>
        public CacheControllerBehavior ControllerBehavior
        {
            get { return this.controllerBehavior; }
        }

        /// <summary>
        /// Constructor for CacheController
        /// </summary>
        /// <param name="serviceUri">Remote sync service Uri with a trailing "/" parameter.</param>
        /// <param name="scopeName">The scope name being synchronized</param>
        /// <param name="localProvider">The OfflineSyncProvider instance for the local store.</param>
        public CacheController(Uri serviceUri, string scopeName, OfflineSyncProvider localProvider)
        {
            if (serviceUri == null)
                throw new ArgumentNullException("serviceUri");

            if (string.IsNullOrEmpty(scopeName))
                throw new ArgumentNullException("scopeName");

            if (!serviceUri.Scheme.Equals("http", StringComparison.CurrentCultureIgnoreCase) &&
                !serviceUri.Scheme.Equals("https", StringComparison.CurrentCultureIgnoreCase))
                throw new ArgumentException("Uri must be http or https schema", "serviceUri");

            if (localProvider == null)
                throw new ArgumentNullException("localProvider");

            this.serviceUri = serviceUri;
            this.localProvider = localProvider;

            this.controllerBehavior = new CacheControllerBehavior();
            this.controllerBehavior.ScopeName = scopeName;
        }

        /// <summary>
        /// Method that synchronize the Cache by uploading all modified changes and then downloading the
        /// server changes.
        /// </summary>
        internal async Task<CacheRefreshStatistics> SynchronizeAsync()
        {
            return await SynchronizeAsync(CancellationToken.None);
        }

        /// <summary>
        /// Method that synchronize the Cache by uploading all modified changes and then downloading the
        /// server changes.
        /// </summary>
        internal async Task<CacheRefreshStatistics> SynchronizeAsync(CancellationToken cancellationToken)
        {
            CacheRefreshStatistics statistics = new CacheRefreshStatistics();
              
            try
            {
                // Check if cancellation has occured
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();
          
                // set start time
                statistics.StartTime = DateTime.Now;

                // First create the CacheRequestHandler
                this.cacheRequestHandler = new HttpCacheRequestHandler(this.serviceUri, this.controllerBehavior);

                // Then fire the BeginSession call on the local provider.
                this.localProvider.BeginSession();

                // Set the flag to indicate BeginSession was successful
                this.beginSessionComplete = true;

                // Do uploads first
                statistics = await this.EnqueueUploadRequest(statistics, cancellationToken);

                // Set end time
                statistics.EndTime = DateTime.Now;

                // Call EndSession only if BeginSession was successful.
                if (this.beginSessionComplete)
                    this.localProvider.EndSession();
            }
            catch (OperationCanceledException ex)
            {
                statistics.EndTime = DateTime.Now;
                statistics.Cancelled = true;
                statistics.Error = ex;
                
                this.localProvider.EndSession();
            }
            catch (Exception ex)
            {
                statistics.EndTime = DateTime.Now;
                statistics.Error = ex;

                this.localProvider.EndSession();
            }
            finally
            {
                // Reset the state
                this.ResetAsyncWorkerManager();
            }

            return statistics;

        }

        /// <summary>
        ///  Reset the state of the objects
        /// </summary>
        private void ResetAsyncWorkerManager()
        {
            lock (this.lockObject)
            {
                this.cacheRequestHandler = null;
                this.controllerBehavior.Locked = false;
                this.beginSessionComplete = false;
            }
        }

        /// <summary>
        /// Method that performs an upload. It gets the ChangeSet from the local provider and then creates an
        /// CacheRequest object for that ChangeSet and then passed the processing asynchronously to the underlying
        /// CacheRequestHandler.
        /// </summary>
        private async Task<CacheRefreshStatistics> EnqueueUploadRequest(CacheRefreshStatistics statistics, CancellationToken cancellationToken)
        {
            this.changeSetId = Guid.NewGuid();

            try
            {
                // Check if cancellation has occured
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                ChangeSet changeSet = this.localProvider.GetChangeSet(this.changeSetId);

                if (changeSet == null || changeSet.Data == null || changeSet.Data.Count == 0)
                {
                    // No data to upload. Skip upload phase.
                    statistics = await this.EnqueueDownloadRequest(statistics, cancellationToken);
                }
                else
                {
                    // Create a SyncRequest out of this.
                    CacheRequest request = new CacheRequest
                    {
                        RequestId = this.changeSetId,
                        Format = this.ControllerBehavior.SerializationFormat,
                        RequestType = CacheRequestType.UploadChanges,
                        Changes = changeSet.Data,
                        KnowledgeBlob = changeSet.ServerBlob,
                        IsLastBatch = changeSet.IsLastBatch
                    };

                    var args = await this.cacheRequestHandler.ProcessCacheRequestAsync(request, changeSet.IsLastBatch, cancellationToken);

                    statistics = await this.ProcessCacheRequestResults(statistics, args, cancellationToken);
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

                statistics.Error = e;
            }


            return statistics;
        }

        /// <summary>
        /// Method that performs a download. It gets the server blob anchor from the local provider and then creates an 
        /// CacheRequest object for that download request. It then passes the processing asynchronously to the underlying
        /// CacheRequestHandler.
        /// </summary>
        private async Task<CacheRefreshStatistics> EnqueueDownloadRequest(CacheRefreshStatistics statistics, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                // Create a SyncRequest for download.
                CacheRequest request = new CacheRequest
                {
                    Format = this.ControllerBehavior.SerializationFormat,
                    RequestType = CacheRequestType.DownloadChanges,
                    KnowledgeBlob = this.localProvider.GetServerBlob()
                };

                var args = await this.cacheRequestHandler.ProcessCacheRequestAsync(request, null , cancellationToken);

                statistics = await this.ProcessCacheRequestResults(statistics, args, cancellationToken);
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

                statistics.Error = e;
            }

            return statistics;
        }

        /// <summary>
        /// Called whenever the CacheRequestHandler proceeses an upload/download request. It is also responsible for
        /// issuing another request if it wasnt the last batch. In case of receiving an Upload response it calls the
        /// underlying provider with the status of the upload. In case of Download it notifies the local provider of the
        /// changes that it needs to save.
        /// </summary>
        private async Task<CacheRefreshStatistics> ProcessCacheRequestResults(CacheRefreshStatistics statistics, CacheRequestResult e, CancellationToken cancellationToken)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();

                if (e.Error != null)
                {
                    // Check to see if it was a UploadRequest in which case we will have to call OnChangeSetUploaded
                    // with error to reset the dirty bits.
                    if (e.ChangeSetResponse != null)
                    {
                        // its an response to a upload
                        this.localProvider.OnChangeSetUploaded(e.Id, e.ChangeSetResponse);
                    }

                    // Finally complete Refresh with error.
                    statistics.Error = e.Error;
                }
                else if (e.ChangeSetResponse != null)
                {
                    // its an response to a upload
                    this.localProvider.OnChangeSetUploaded(e.Id, e.ChangeSetResponse);

                    if (e.ChangeSetResponse.Error != null)
                    {
                        statistics.Error = e.ChangeSetResponse.Error;
                        return statistics;
                    }

                    // Increment the ChangeSets uploaded count
                    statistics.TotalChangeSetsUploaded++;
                    statistics.TotalUploads += e.BatchUploadCount;

                    // Update refresh stats
                    foreach (var e1 in e.ChangeSetResponse.ConflictsInternal)
                    {
                        if (e1 is SyncConflict)
                            statistics.TotalSyncConflicts++;
                        else
                            statistics.TotalSyncErrors++;
                    }

                    // Dont enqueue another request if its been cancelled
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        if (!((bool)e.State))
                        {
                            // Check to see if this was the last batch or else enqueue another pending Upload request
                            statistics = await this.EnqueueUploadRequest(statistics, cancellationToken);
                        }
                        else
                        {
                            // That was the last batch. Issue an Download request
                            statistics = await this.EnqueueDownloadRequest(statistics, cancellationToken);
                        }
                    }
                    else
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                }
                else // It means its an Download response
                {
                    Debug.Assert(e.ChangeSet != null, "Completion is not for a download request.");

                    // Increment the refresh stats
                    if (e.ChangeSet != null)
                    {
                        statistics.TotalChangeSetsDownloaded++;
                        statistics.TotalDownloads += (uint)e.ChangeSet.Data.Count;

                        await this.localProvider.SaveChangeSet(e.ChangeSet);

                        // Dont enqueue another request if its been cancelled
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            if (!e.ChangeSet.IsLastBatch)
                            {
                                // Enqueue the next download
                                statistics = await this.EnqueueDownloadRequest(statistics, cancellationToken);
                            }
                        }
                        else
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Re throw the operation cancelled
                throw;
            }
            catch (Exception exp)
            {
                if (ExceptionUtility.IsFatal(exp))
                    throw;
                statistics.Error = exp;
            }

            return statistics;
        }
    }
}
