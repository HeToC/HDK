namespace System.ComponentModel
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class NavigationBoundPropertyAttribute : Attribute
    {
        public string Name { get; private set; }
        public NavigationBoundPropertyAttribute(string uriParameterName)
        {
            Name = uriParameterName;
        }
    }
}
