using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;

namespace System.Data
{
    public class DataObjectSet
    {
        public CoreDispatcher Dispatcher { get; set; }
        private readonly object _generateIdLock = new object();
        private long _nextGeneratedId = 1;
        private readonly object _foreignKeysLock = new object();
        private readonly Dictionary<string, Dictionary<long, IDataObjectCollection>> _foreignKeys = new Dictionary<string, Dictionary<long, IDataObjectCollection>>();
        private readonly Dictionary<Type, IDataObjectCollection> _resolveReferences = new Dictionary<Type, IDataObjectCollection>();

        public DataObjectCollection<T> RegisterEntityCollection<T>() where T : DataObject
        {
            if (_resolveReferences.ContainsKey(typeof(T))) throw new ArgumentException(" An DataObjectSet with that name already exists.");
            var entityCollection = new DataObjectCollection<T>(() => this);
            _resolveReferences.Add(typeof(T), entityCollection);
            return entityCollection;
        }

        public DataObjectCollection<T> GetEntityCollection<T>() where T : DataObject
        {
            return (DataObjectCollection<T>)_resolveReferences[typeof(T)];
        }

        public T FindEntity<T>(long id) where T : DataObject
        {
            var collection = _resolveReferences[typeof(T)];
            var foundEntity = collection.FindByPrimaryKeyBase(id);
            if (foundEntity != null)
            {
                return (T)foundEntity;
            }
            return null;
        }

        public T CreateEntity<T>(long id = -1) where T : DataObject
        {
            if (id == -1)
                id = GenerateId();
            Type t = typeof(T);
            return (T)Activator.CreateInstance(t, new object[] { this, id });
        }

        public Task<T> CreateEntityAsync<T>(long id = -1) where T : DataObject
        {
            return Task.Run(() => CreateEntity<T>(id));
        }

        public void AddEntity<T>(T entity) where T : DataObject
        {
            var collection = _resolveReferences[typeof(T)];
            if (collection != null)
            {
                collection.Add(entity);
            }
        }

        public Task AddEntityAsync<T>(T entity) where T : DataObject
        {
            return Task.Run(() => AddEntity(entity));
        }

        public long GenerateId()
        {
            lock (_generateIdLock)
            {
                return _nextGeneratedId++;
            }
        }

        public void ImportEntity<T>(T entity) where T : DataObject
        {
            var copy = CreateEntity<T>(entity.Id);
            copy.Copy(entity);
            AddEntity(copy);
        }

        public void SetForeignKey<T>(string relationName, long foreignKey, long oldKey, T entity) where T : DataObject
        {
            lock (_foreignKeysLock)
            {
                var relation = CreateRelationWhenNotExist<T>(relationName, foreignKey);
                var entityCollections = _foreignKeys[relationName];
                if (entityCollections.ContainsKey(oldKey))
                {
                    var oldRelation = entityCollections[oldKey];
                    var entityInOldRelation = oldRelation.FindByPrimaryKeyBase(entity.Id);
                    if (entityInOldRelation != null)
                    {
                        oldRelation.Remove(entity);
                    }
                }
                if (foreignKey == 0) return;
                var entityInNewRelation = relation.FindByPrimaryKeyBase(entity.Id);
                if (entityInNewRelation == null)
                {
                    relation.Add(entity);
                }
            }
        }

        private IDataObjectCollection CreateRelationWhenNotExist<T>(string relationName, long foreignKey) where T : DataObject
        {
            if (foreignKey == 0) return new DataObjectCollection<T>(() => this);
            if (!_foreignKeys.ContainsKey(relationName))
            {
                _foreignKeys[relationName] = new Dictionary<long, IDataObjectCollection>();
            }
            var entityCollections = _foreignKeys[relationName];
            if (!entityCollections.ContainsKey(foreignKey))
            {
                entityCollections[foreignKey] = new DataObjectCollection<T>(() => this);
            }
            var relation = entityCollections[foreignKey];
            return relation;
        }

        public DataObjectCollection<T> GetEntitiesByForeignKey<T>(string relationName, long foreignKey) where T : DataObject
        {
            lock (_foreignKeysLock)
            {
                return (DataObjectCollection<T>)CreateRelationWhenNotExist<T>(relationName, foreignKey);
            }
        }
    }
}
