using System.Composition;

namespace System.ComponentModel
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ExportViewModelAttribute : ExportAttribute
    {
        public ExportViewModelAttribute()
            : base(typeof(IViewModel))
        {
        }

        public ExportViewModelAttribute(string token)
            : base(typeof(IViewModel))
        {
            CorrelationToken = token;
        }

        public string CorrelationToken { get; set; }
    }
}
