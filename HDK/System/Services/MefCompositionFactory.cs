using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Linq;
using System.Text;

namespace System.Services
{
    public class MefCompositionFactory<T> : ICompositionFactory<T>
         where T : class
    {
        internal MefCompositionFactory()
        {
        }

        /// <summary>
        /// Internal use.
        /// </summary>
        [Import(AllowDefault = true)]
        public ExportFactory<T> ExportFactory { get; set; }


        /// <summary>
        /// Creates and returns a new instance of T.
        /// </summary>
        public T NewInstance()
        {
            if (ExportFactory == null)
                throw new CompositionFailedException();

            return ExportFactory.CreateExport().Value;
        }
    }

}
