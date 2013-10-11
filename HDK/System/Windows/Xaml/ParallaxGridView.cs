using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace System.Windows.Xaml
{
    public class ParallaxGridView : GridView
    {
        private ItemsControl ParallacticLayersElement { get; set; }
        private ScrollViewer ScrollViewer { get; set; }

        public static readonly DependencyProperty ParallacticLayersProperty =
            DependencyProperty.Register("ParallacticLayers", typeof(List<FrameworkElement>), typeof(ParallaxGridView), new PropertyMetadata(new List<FrameworkElement>()));

        public List<FrameworkElement> ParallacticLayers
        {
            get { return (List<FrameworkElement>)GetValue(ParallacticLayersProperty); }
            set { SetValue(ParallacticLayersProperty, value); }
        }

        public ParallaxGridView()
        {
            this.DefaultStyleKey = typeof(ParallaxGridView);
        }

        protected override void OnApplyTemplate()
        {
            this.ParallacticLayersElement = this.GetTemplateChild("ParallacticLayersElement") as ItemsControl;

            foreach (FrameworkElement element in this.ParallacticLayers)
                this.ParallacticLayersElement.Items.Add(element);

            this.ScrollViewer = this.GetTemplateChild("ScrollViewer") as ScrollViewer;
            this.ScrollViewer.ViewChanged += ScrollViewer_ViewChanged;
            base.OnApplyTemplate();
        }

        private void ScrollViewer_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {
            foreach (FrameworkElement element in this.ParallacticLayersElement.Items.OfType<FrameworkElement>())
            {
                Thickness thickness = element.Margin;
                var deltaX = (this.ScrollViewer.HorizontalOffset / this.ScrollViewer.ScrollableWidth) * (element.ActualWidth - this.ScrollViewer.ViewportWidth);
                thickness.Left = -deltaX;
                element.Margin = thickness;
            }
        }
    }
}
