using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace System.Services
{
    public interface IStorageManagerService : IService
    {
        Task<T> RetrieveAsync<T>(StorageFile file, DataContractSerializerSettings settings);
        Task<T> RetrieveAsync<T>(StorageFolder folder, string name, DataContractSerializerSettings settings);
        Task StoreAsync<T>(StorageFile file, T value, DataContractSerializerSettings settings);
        Task StoreAsync<T>(StorageFolder folder, string name, T value, DataContractSerializerSettings settings);

        Task<T> RetrieveAsync<T>(StorageFile file);
        Task<T> RetrieveAsync<T>(StorageFolder folder, string name);
        Task StoreAsync<T>(StorageFile file, T value);
        Task StoreAsync<T>(StorageFolder folder, string name, T value);
    }

    [ExportService("StorageManagerService", "IO Storage service", typeof(IStorageManagerService)), Shared]
    public class StorageManagerService : IStorageManagerService
    {
        private ILoggerService Logger;
  
        [ImportingConstructor]
        public StorageManagerService([Import] ILoggerService logger)
        {
            Logger = logger;
        }

        public async Task<T> RetrieveAsync<T>(StorageFile file, DataContractSerializerSettings settings)
        {
            // Validate parameters

            if (file == null)
                throw new ArgumentNullException("file");

            // Open the file from the file stream

            using (Stream fileStream = await file.OpenStreamForReadAsync())
            {
                // Copy the file to a MemoryStream (as we can do this async)
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    await fileStream.CopyToAsync(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    try
                    {
                        DataContractSerializer serializer = new DataContractSerializer(typeof(T), settings);
                        return (T)serializer.ReadObject(memoryStream);
                    }
                    catch (Exception exc)
                    {
                        exc.Data.Add("File", file.Path);
                        exc.Data.Add("DataType", typeof(T).ToString());

                        Logger.Log(LogSeverity.Warning, this, "Unable to deserialize object.", exc);
                        throw exc;
                    }
                }
            }
        }

        public async Task<T> RetrieveAsync<T>(StorageFolder folder, string name, DataContractSerializerSettings settings)
        {
            // Validate parameters

            if (folder == null)
                throw new ArgumentNullException("folder");

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            // Open the file, if it doesn't exist then return default, otherwise pass on

            try
            {
                StorageFile file = await folder.GetFileAsync(name);
                return await RetrieveAsync<T>(file, settings);
            }
            catch (FileNotFoundException exc)
            {
                exc.Data.Add("Folder", folder.Path);
                exc.Data.Add("File", name);
                Logger.Log(LogSeverity.Warning, this, "Unable to Retrieve file.", exc);
                
                return default(T);
            }
        }

        public async Task StoreAsync<T>(StorageFile file, T value, DataContractSerializerSettings settings)
        {

            if (file == null)
                throw new ArgumentNullException("file");

            // Write the object to a MemoryStream using the DataContractSerializer
            using (MemoryStream dataStream = new MemoryStream())
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(T), settings);
                serializer.WriteObject(dataStream, value);

                // Save the data to the file stream

                using (Stream fileStream = await file.OpenStreamForWriteAsync())
                {
                    dataStream.Seek(0, SeekOrigin.Begin);
                    await dataStream.CopyToAsync(fileStream);
                }
            }
        }

        public async Task StoreAsync<T>(StorageFolder folder, string name, T value, DataContractSerializerSettings settings)
        {
            // Validate parameters

            if (folder == null)
                throw new ArgumentNullException("folder");

            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("name");

            // Create the new file, overwriting the existing data, then pass on

            StorageFile file = await folder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
            await StoreAsync<T>(file, value, settings);
        }

        public Task<T> RetrieveAsync<T>(StorageFile file)
        {
            return this.RetrieveAsync<T>(file, new DataContractSerializerSettings());
        }

        public Task<T> RetrieveAsync<T>(StorageFolder folder, string name)
        {
            return RetrieveAsync<T>(folder, name, new DataContractSerializerSettings());
        }

        public async Task StoreAsync<T>(StorageFile file, T value)
        {
            await StoreAsync<T>(file, value, new DataContractSerializerSettings());
        }

        public async Task StoreAsync<T>(StorageFolder folder, string name, T value)
        {
            await StoreAsync<T>(folder, name, value, new DataContractSerializerSettings());
        }

        public void Dispose()
        {
        }

        //[ImportMany(typeof(IPersistedObject))]
        //public ObservableCollection<Lazy<IPersistedObject, IPersistedObjectMetadata>> PersistedObjects { get; set; }
    }
}
