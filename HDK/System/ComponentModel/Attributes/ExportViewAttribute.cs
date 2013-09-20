using System.Composition;

namespace System.ComponentModel
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportViewAttribute : ExportAttribute
    {
        public ExportViewAttribute()
            : base(typeof(IView))
        {
        }

        public ExportViewAttribute(string Token)
            : base(typeof(IView))
        {
            CorrelationToken = Token;
        }

        public ExportViewAttribute(Type ViewType, string Token)
            : base(typeof(IView))
        {
            CorrelationToken = Token;
            this.ViewType = ViewType;
        }

        public Type ViewType { get; set; }
        public string CorrelationToken { get; set; }
    }
}
