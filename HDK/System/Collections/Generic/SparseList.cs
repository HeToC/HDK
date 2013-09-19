using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    //TODO: Add events and prepare for AsyncSparseList
    /// <summary>
    /// A list which can support a huge virtual item count
    /// by assuming that many items are never accessed. Space for items is allocated
    /// in pages.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SparseList<T> where T : class
    {
        private readonly int _pageSize;
        private readonly ISparsePageList<T> _allocatedPages;
        private ISparsePage<T> _currentPage;

        public SparseList(int pageSize)
        {
            _pageSize = pageSize;
            _allocatedPages = CreatePageList(_pageSize);
        }

        protected virtual ISparsePageList<T> CreatePageList(int pageSize)
        {
            return new SparsePageListBase<T>(pageSize);
        }

        /// <remarks>This method is optimised for sequential access. I.e. it performs
        /// best when getting and setting indicies in the same locality</remarks>
        public T this[int index]
        {
            get
            {
                var pageAndSubIndex = EnsureCurrentPage(index);
                return _currentPage[pageAndSubIndex.SubIndex];
            }
            set
            {
                var pageAndSubIndex = EnsureCurrentPage(index);
                _currentPage[pageAndSubIndex.SubIndex] = value;
            }
        }


        private PageAndSubIndex EnsureCurrentPage(int index)
        {
            var pageAndSubIndex = new PageAndSubIndex(index / _pageSize, index % _pageSize);

            if (_currentPage == null || _currentPage.PageIndex != pageAndSubIndex.PageIndex)
                _currentPage = _allocatedPages.GetOrCreatePage(pageAndSubIndex.PageIndex);

            return pageAndSubIndex;
        }


        public void RemoveRange(int firstIndex, int count)
        {
            var firstItem = new PageAndSubIndex(firstIndex / _pageSize, firstIndex % _pageSize);
            if (firstItem.SubIndex + count > _pageSize)
            {
                throw new NotImplementedException("RemoveRange is only implemented to work within page boundaries");
            }

            if (_allocatedPages.Contains(firstItem.PageIndex))
            {
                if (_allocatedPages[firstItem.PageIndex].Trim(firstItem.SubIndex, count))
                {
                    _allocatedPages.Remove(firstItem.PageIndex);
                }
            }
        }
    }

    public struct PageAndSubIndex
    {
        private readonly int _pageIndex;
        private readonly int _subIndex;

        public PageAndSubIndex(int pageIndex, int subIndex)
        {
            _pageIndex = pageIndex;
            _subIndex = subIndex;
        }

        public int PageIndex
        {
            get { return _pageIndex; }
        }

        public int SubIndex
        {
            get { return _subIndex; }
        }
    }


    public interface ISparsePage<TElement>
    {
        int PageIndex { get; }
        TElement this[int index] { get; set; }

        bool Trim(int firstIndex, int count);
    }


    public class SparsePageBase<TElement> : ISparsePage<TElement>
    {
        private readonly int _pageIndex;
        private readonly TElement[] _items;

        public SparsePageBase(int pageIndex, int pageSize)
        {
            _pageIndex = pageIndex;
            _items = new TElement[pageSize];
        }

        public int PageIndex
        {
            get { return _pageIndex; }
        }

        public TElement this[int index]
        {
            get
            {
                return _items[index];
            }
            set
            {
                _items[index] = value;
            }
        }

        public bool Trim(int firstIndex, int count)
        {
            for (int i = firstIndex; i < firstIndex + count; i++)
                _items[i] = default(TElement);

            for (int i = 0; i < _items.Length; i++)
                if (_items[i] != null)
                    return false;

            return true;
        }
    }

    public interface ISparsePageList<TElement>
    {
        ISparsePage<TElement> GetOrCreatePage(int key);

        bool Contains(int key);
        bool Remove(int key);

        ISparsePage<TElement> this[int key] { get; }
    }


    public class SparsePageListBase<TElement> : KeyedCollection<int, ISparsePage<TElement>>, ISparsePageList<TElement>
    {
        private readonly int _pageSize;

        public SparsePageListBase(int pageSize)
        {
            _pageSize = pageSize;
        }

        protected override int GetKeyForItem(ISparsePage<TElement> item)
        {
            return item.PageIndex;
        }

        public ISparsePage<TElement> GetOrCreatePage(int pageIndex)
        {
            if (!Contains(pageIndex))
                Add(CreatePage(pageIndex, _pageSize));

            return this[pageIndex];
        }

        protected virtual ISparsePage<TElement> CreatePage(int pageIndex, int pageSize)
        {
            return new SparsePageBase<TElement>(pageIndex, pageSize);
        }
    }
}
