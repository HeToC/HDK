using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace System.Windows.Xaml
{
    public class Expander : HeaderedContentControl
    {
        public Expander()
        {
            this.DefaultStyleKey = typeof(Expander);
        }

        #region IsExpanded

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(Expander),
                new PropertyMetadata((bool)true,
                    new PropertyChangedCallback(OnIsExpandedChanged)));

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        private static void OnIsExpandedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Expander target = (Expander)d;
            bool oldIsExpanded = (bool)e.OldValue;
            bool newIsExpanded = target.IsExpanded;
            target.OnIsEnabledChanged(oldIsExpanded, newIsExpanded);
        }

        protected virtual void OnIsEnabledChanged(bool oldIsEnabled, bool newIsEnabled)
        {
            Visibility vis = newIsEnabled ? Visibility.Visible : Visibility.Collapsed;
            ContentVisibility = vis;
        }

        #endregion

        #region ContentVisibility

        public static readonly DependencyProperty ContentVisibilityProperty =
            DependencyProperty.Register("ContentVisibility", typeof(Visibility), typeof(Expander),
                new PropertyMetadata((Visibility)Visibility.Visible));

        public Visibility ContentVisibility
        {
            get { return (Visibility)GetValue(ContentVisibilityProperty); }
            private set { SetValue(ContentVisibilityProperty, value); }
        }

        #endregion

    }
}
