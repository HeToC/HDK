using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Composition;
using System.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Navigation;
using System.Reflection;
using System.Services;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace System.Windows.Xaml
{
    public abstract class CompositeApplication : Application
    {
        [Import]
        public INavigationService NavigationService { get; set; }

        [Import]
        public IMVVMLocatorService mvvmlocator { get; set; }

        [Import]
        public IApplicationLifeTimeService lifetimeService { get; set; }

        private ICompositionProvider m_CompositionProvider = new MefCompositionProvider();

        public CompositeApplication()
        {
            Initialize();
        }

        private void Initialize()
        {
            AttachEvents();

            m_CompositionProvider.Compose(this);
        }

        private void AttachEvents()
        {
            this.Suspending += OnSuspending;
        }

        protected virtual IShellView CreateShell()
        {
            return (IShellView)mvvmlocator.CreateView("#Shell", null);
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            IShellView shellView = Window.Current.Content as IShellView;
            
            // create shell if it has not been initialized yet
            if (shellView == null)
            {
                shellView = CreateShell();
                Frame rootFrame = shellView.RootFrame;
                NavigationService.AttachToFrame(rootFrame);

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }
                Window.Current.Content = shellView as UIElement;
            }

            // navigate frame to start page
            //if (shellView.RootFrame.Content == null)
            //{
            //    // When the navigation stack isn't restored navigate to the first page,
            //    // configuring the new page by passing required information as a navigation
            //    // parameter
            //    //if (!rootFrame.Navigate(typeof(MainPage), args.Arguments))
            //    if (!NavigationService.Navigate(CreateShell(), "/?TestProperty=ololoItWorks!!!!!!"))//args.Arguments))
            //    {
            //        throw new Exception("Failed to create initial page");
            //    }
            //}

            
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
