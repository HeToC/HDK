using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace System.Services
{
    public interface IApplicationLifeTimeService : IService
    {
        void Attach(Application app);
    }

    [ExportService("ApplicationLifeTimeService", "descr", typeof(IApplicationLifeTimeService)), Shared]
    public class ApplicationLifeTimeService : IApplicationLifeTimeService
    {
        public ApplicationLifeTimeService()
        {
        }

        public void Attach(Application app)
        {
        }

        public void Dispose()
        {
        }
    }

}
