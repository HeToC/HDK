using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media.Animation;

namespace System.Windows.Xaml
{
    [ContentProperty(Name = "Content")]
    public class HeaderedContentControl : ContentControl
    {
        public HeaderedContentControl()
        {
            this.DefaultStyleKey = typeof(HeaderedContentControl);
        }

        #region Header

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(object), typeof(HeaderedContentControl),
                new PropertyMetadata((object)null));

        public object Header
        {
            get { return (object)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        #endregion

        #region HeaderTemplate

        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(HeaderedContentControl),
                new PropertyMetadata((DataTemplate)null));

        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        #endregion

        #region HeaderTransitions

        public static readonly DependencyProperty HeaderTransitionsProperty =
            DependencyProperty.Register("HeaderTransitions", typeof(TransitionCollection), typeof(HeaderedContentControl),
                new PropertyMetadata((TransitionCollection)null));

        public TransitionCollection HeaderTransitions
        {
            get { return (TransitionCollection)GetValue(HeaderTransitionsProperty); }
            set { SetValue(HeaderTransitionsProperty, value); }
        }

        #endregion


        #region HeaderTemplateSelector

        public static readonly DependencyProperty HeaderTemplateSelectorProperty =
            DependencyProperty.Register("HeaderTemplateSelector", typeof(DataTemplateSelector), typeof(HeaderedContentControl),
                new PropertyMetadata((DataTemplateSelector)null));

        public DataTemplateSelector HeaderTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(HeaderTemplateSelectorProperty); }
            set { SetValue(HeaderTemplateSelectorProperty, value); }
        }

        #endregion
    }
}
