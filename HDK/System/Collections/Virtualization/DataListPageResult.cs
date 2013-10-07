using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Virtualization
{
    public struct DataListPageResult<T>
    {
        // *** Constructors ***

        public DataListPageResult(int? totalItemCount, int? itemsPerPage, int? pageNumber, IList<T> page)
            : this()
        {
            this.TotalItemCount = totalItemCount;
            this.ItemsPerPage = itemsPerPage;
            this.PageNumber = pageNumber;
            this.Page = page;
        }

        // *** Properties ***

        public int? TotalItemCount { get; private set; }
        public int? ItemsPerPage { get; private set; }
        public int? PageNumber { get; private set; }
        public IList<T> Page { get; private set; }
    }
}
