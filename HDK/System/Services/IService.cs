using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Services
{
    public interface IService : IDisposable
    {
        void StartService();

        void StopService();
    }


    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportServiceAttribute : ExportAttribute//, IServiceMetadata
    {
        public string ServiceName { get; set; }
        public Type ServiceType { get; set; }
        public string Description { get; set; }
        public int SequenceNumber { get; set; }

        public ExportServiceAttribute()
        {
        }

        public ExportServiceAttribute(string serviceName, string description, Type serviceType, int sequenceNumber)
            : base(typeof(IService))
        {
            Debug.WriteLine(serviceName);
            ServiceName = serviceName;
            ServiceType = serviceType;
            Description = description;
            SequenceNumber = sequenceNumber;
        }

        public override bool Equals(object obj)
        {
            var v = (ExportServiceAttribute)obj;
            if (v == null)
                return false;

            return this.ServiceName.Equals(v.ServiceName) &&
                   this.ServiceType.Equals(v.ServiceType) &&
                   this.Description.Equals(v.Description) &&
                   this.SequenceNumber.Equals(v.SequenceNumber);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

}
