using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace System.Windows.Xaml
{
    /// <summary>
    /// <<Page ... local:HeaderedFrame.PageName=""> ...
    /// </summary>
    /// 
    /// TODO: Use Headered Content COntrol
    [TemplatePart(Name = FrameName, Type = typeof(Frame))]
    public class HeaderedFrame : Control
    {
        public const string FrameName = "PART_Frame";
        Type preType;
        object preParameter;

        public HeaderedFrame()
        {
            this.DefaultStyleKey = typeof(HeaderedFrame);
        }

        #region Header

        /// <summary>
        /// Header Dependency Property
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(object), typeof(HeaderedFrame),
                new PropertyMetadata((object)null));

        /// <summary>
        /// Gets or sets the Header property. This dependency property 
        /// indicates ....
        /// </summary>
        public object Header
        {
            get { return (object)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        #endregion

        #region BackButtonStype

        /// <summary>
        /// Header Dependency Property
        /// </summary>
        public static readonly DependencyProperty BackButtonStyleProperty =
            DependencyProperty.Register("BackButtonStyle", typeof(object), typeof(HeaderedFrame),
                new PropertyMetadata((object)null));

        /// <summary>
        /// Gets or sets the Header property. This dependency property 
        /// indicates ....
        /// </summary>
        public object BackButtonStyle
        {
            get { return (object)GetValue(BackButtonStyleProperty); }
            set { SetValue(BackButtonStyleProperty, value); }
        }

        #endregion

        #region GoBackCommand

        private ICommand _GoBackCommand;

        public ICommand GoBackCommand
        {
            get
            {
                return _GoBackCommand ?? (_GoBackCommand = new DelegateCommand((parameter) => GoBack(parameter)));
            }
        }

        public virtual void GoBack(object parameter)
        {
            InternalFrame.GoBack();
        }

        #endregion

        #region InternalFrame

        /// <summary>
        /// InternalFrame Dependency Property
        /// </summary>
        public static readonly DependencyProperty InternalFrameProperty =
            DependencyProperty.Register("InternalFrame", typeof(Frame), typeof(HeaderedFrame),
                new PropertyMetadata((Frame)null));

        /// <summary>
        /// Gets or sets the InternalFrame property. This dependency property 
        /// indicates ....
        /// </summary>
        public Frame InternalFrame
        {
            get { return (Frame)GetValue(InternalFrameProperty); }
            set { SetValue(InternalFrameProperty, value); }
        }

        #endregion

        #region CurrentPage

        /// <summary>
        /// CurrentPage Dependency Property
        /// </summary>
        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register("CurrentPage", typeof(Page), typeof(HeaderedFrame),
                new PropertyMetadata((Page)null,
                    new PropertyChangedCallback(OnCurrentPageChanged)));

        /// <summary>
        /// Gets or sets the CurrentPage property. This dependency property 
        /// indicates ....
        /// </summary>
        public Page CurrentPage
        {
            get { return (Page)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        /// <summary>
        /// Handles changes to the CurrentPage property.
        /// </summary>
        private static void OnCurrentPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HeaderedFrame target = (HeaderedFrame)d;
            Page oldCurrentPage = (Page)e.OldValue;
            Page newCurrentPage = target.CurrentPage;
            target.OnCurrentPageChanged(oldCurrentPage, newCurrentPage);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the CurrentPage property.
        /// </summary>
        protected virtual void OnCurrentPageChanged(Page oldCurrentPage, Page newCurrentPage)
        {
            SetParentFrame(newCurrentPage, this);
            if (oldCurrentPage != null)
                SetParentFrame(oldCurrentPage, null);

            OnCurrentPageChanged(EventArgs.Empty);
        }

        #endregion

        #region ParentFrame

        /// <summary>
        /// ParentFrame Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty ParentFrameProperty =
            DependencyProperty.RegisterAttached("ParentFrame", typeof(HeaderedFrame), typeof(HeaderedFrame),
                new PropertyMetadata((HeaderedFrame)null));

        /// <summary>
        /// Gets the ParentFrame property. This dependency property 
        /// indicates ....
        /// </summary>
        public static HeaderedFrame GetParentFrame(DependencyObject d)
        {
            return (HeaderedFrame)d.GetValue(ParentFrameProperty);
        }

        /// <summary>
        /// Sets the ParentFrame property. This dependency property 
        /// indicates ....
        /// </summary>
        public static void SetParentFrame(DependencyObject d, HeaderedFrame value)
        {
            d.SetValue(ParentFrameProperty, value);
        }

        #endregion

        #region PageName

        /// <summary>
        /// PageName Attached Dependency Property
        /// </summary>
        public static readonly DependencyProperty PageNameProperty =
            DependencyProperty.RegisterAttached("PageName", typeof(string), typeof(HeaderedFrame),
                new PropertyMetadata((string)null));

        /// <summary>
        /// Gets the PageName property. This dependency property 
        /// indicates ....
        /// </summary>
        public static string GetPageName(DependencyObject d)
        {
            return (string)d.GetValue(PageNameProperty);
        }

        /// <summary>
        /// Sets the PageName property. This dependency property 
        /// indicates ....
        /// </summary>
        public static void SetPageName(DependencyObject d, string value)
        {
            d.SetValue(PageNameProperty, value);
        }

        #endregion

        #region CurrentPageChanged

        public event EventHandler CurrentPageChanged;

        protected virtual void OnCurrentPageChanged(EventArgs e)
        {
            var handler = this.CurrentPageChanged;
            if (handler != null)
                handler(this, e);
        }




        #endregion

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            InternalFrame = (Frame)GetTemplateChild(FrameName);
            if (InternalFrame == null)
                throw new Exception("missing template part");

            InternalFrame.Navigated += InternalFrame_Navigated;

            if (preType != null)
            {
                InternalFrame.Navigate(preType, preParameter);

                preType = null;
                preParameter = null;
            }
        }

        void InternalFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (InternalFrame.Content is Page)
                CurrentPage = (Page)InternalFrame.Content;
        }

        public void Navigate(Type type, object parameter = null)
        {
            if (InternalFrame != null)
                InternalFrame.Navigate(type, parameter);
            else
            {
                preParameter = parameter;
                preType = type;
            }
        }

    }
}
