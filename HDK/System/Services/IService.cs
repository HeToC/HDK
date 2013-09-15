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
        //void StartService();

        //void StopService();
    }

    public interface IServiceMetadata
    {
        string ServiceName { get; set; }
        string Description { get; set; }
        int SequenceNumber { get; set; }

        Type ServiceType { get; set; }
    }

    public class ServiceMetadata : IServiceMetadata
    {
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public int SequenceNumber { get; set; }

        public Type ServiceType { get; set; }
    }

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportServiceAttribute : ExportAttribute, IServiceMetadata
    {
        public string ServiceName { get; set; }
        public string Description { get; set; }
        public int SequenceNumber { get; set; }

        public Type ServiceType { get; set; }

        public ExportServiceAttribute(string serviceName, string description, Type serviceType, int sequenceNumber = 0)
            : base(typeof(IService))
        {
            ServiceName = serviceName;
            Description = description;
            SequenceNumber = sequenceNumber;
            ServiceType = serviceType;
        }

        public override bool Equals(object obj)
        {
            var v = (ExportServiceAttribute)obj;
            if (v == null)
                return false;

            return this.ServiceName.Equals(v.ServiceName) &&
                   this.Description.Equals(v.Description) &&
                   this.ServiceType.Equals(v.ServiceType) &&
                   this.SequenceNumber.Equals(v.SequenceNumber);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

}
