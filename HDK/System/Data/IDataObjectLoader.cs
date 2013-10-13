using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Data
{
    public interface IDataObjectLoader<T>
        where T : DataObject
    {
        Task<T> FetchItem(long id, int stage);
    }

    public interface IDataObjectCollectionLoader<T> : IDataObjectLoader<T>
        where T: DataObject
    {
        Task<IEnumerable<long>> FetchIDs();
        Task<long> FetchCount();

        Task HeartBeat(CancellationTokenSource tokenSource);
    }

    public abstract class DataObjectCollectionLoader<T> : IDataObjectCollectionLoader<T>
        where T : DataObject
    {
        private readonly Func<DataObjectSet> _findContext;

        public DataObjectCollectionLoader(Func<DataObjectSet> findContext)
        {
            _findContext = findContext;
        }

        public DataObjectSet Context { get { return _findContext(); } }

        public abstract Task<IEnumerable<long>> FetchIDs();
        public abstract Task<long> FetchCount();
        public abstract Task<T> FetchItem(long id, int stage);

        private bool m_IsFirstTimeFetching = true;
        public async Task HeartBeat(CancellationTokenSource tokenSource)
        {
            while (tokenSource == null || !tokenSource.IsCancellationRequested)
            {
                if (m_IsFirstTimeFetching)
                    m_IsFirstTimeFetching = false;

                var collection = Context.GetEntityCollection<T>();
                if (collection == null)
                    continue;

                var atmIds = from item in collection
                             select item.Id;
                var actualIds = await FetchIDs();

                await FetchMoreData(tokenSource, atmIds);
                //var newItems = actualIds.Except(atmIds);
                //foreach(long id in newItems)
                //{
                //    var newItem = await FetchItem(id);
                //    Context.ImportEntity(newItem);
                //}

                await Task.Delay(3000);
            }
        }
        private async Task FetchMoreData(CancellationTokenSource tokenSource, IEnumerable<long> items)
        {
            var entities = from itemId in items
                           let entityObject = Context.FindEntity<T>(itemId)
                           let currentStage = entityObject.CurrentLoadingStage
                           let maxLoadingStage = entityObject.MaxLoadingStage
                           where currentStage <= maxLoadingStage
                           select entityObject;

            foreach (var entity in entities)
            {
                var updatedEntity = await FetchItem(entity.Id, entity.CurrentLoadingStage + 1);
                updatedEntity.CurrentLoadingStage++;
                entity.Copy(updatedEntity);
            }
        }
    }
}
