using Windows.UI.Xaml.Controls;

namespace System.ComponentModel
{
    public interface IShellView : IView
    {
        Frame RootFrame { get; }
    }
}
