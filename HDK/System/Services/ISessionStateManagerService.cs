using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace System.Services
{
    public interface ISessionStateManagerService : IService
    {
        Dictionary<string, object> SessionState { get; }

        Task SaveAsync();
        Task RestoreAsync();

        void SetValue(string key, object value);
        object GetValue(string key);
    }

    [Shared]
    [ExportService("ISessionStateManagerService", "", typeof(ISessionStateManagerService))]
    public class SessionStateManagerService : ISessionStateManagerService
    {
        public Dictionary<string, object> SessionState { get; private set; }

        private const string filename = "SessionState.xml";
        private IStorageManagerService StorageServiceManager;
        private ILoggerService Logger;
        private StorageFolder defaultStorageFolder = ApplicationData.Current.RoamingFolder;

        [ImportingConstructor]
        public SessionStateManagerService([Import] ILoggerService logger, [Import]IStorageManagerService sms )//IServiceLocator locator)
        {
            StorageServiceManager = sms;
            Logger = logger;
        }

        public async Task SaveAsync()
        {
            Logger.Log(LogSeverity.Verbose, this, "Saving Requested");

            //if (PersistedObjects != null && PersistedObjects.Count > 0)
            //    PersistedObjects.Where(lazy => lazy.IsValueCreated).ForEach(lazy => lazy.Value.OnSerializing());

            var file = await defaultStorageFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

            await StorageServiceManager.StoreAsync<Dictionary<string, object>>(
                    file,
                    SessionState,
                    new DataContractSerializerSettings()
                    {
                        PreserveObjectReferences = true
                    });

            Logger.Log(LogSeverity.Verbose, this, "Saving Completed");
        }

        public async Task RestoreAsync()
        {
            Logger.Log(LogSeverity.Verbose, this, "Restoring Requested");

            // Get the input stream for the SessionState file.
            StorageFile file = await defaultStorageFolder.CreateFileAsync(filename, CreationCollisionOption.OpenIfExists);
            if (file == null) return;

            SessionState = await StorageServiceManager.RetrieveAsync<Dictionary<string, object>>(file, new DataContractSerializerSettings());
            if (SessionState == null)
                SessionState = new Dictionary<string, object>();

            Logger.Log(LogSeverity.Verbose, this, "Restoring Completed");
        }

        public void SetValue(string key, object value)
        {
            if (this.SessionState.ContainsKey(key))
                SessionState[key] = value;
            else
                SessionState.Add(key, value);
        }

        public object GetValue(string key)
        {
            if (SessionState.ContainsKey(key))
                return SessionState[key];
            return null;
        }

        public void Dispose()
        {
        }
    }
}
