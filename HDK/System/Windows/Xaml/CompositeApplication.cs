using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Composition;
using System.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
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
        public IServiceLocator ServiceLocator { get; set; }

        [Import]
        public INavigationService NavigationService { get; set; }

        //[Import]
        public IMVVMLocatorService mvvmlocator { get; set; }

        private ICompositionProvider m_CompositionProvider;

        public CompositeApplication()
        {
            Initialize();
        }

        protected virtual ICompositionProvider CreateCompositionProvider(IServiceLocator serviceLocator)
        {
            return new MefCompositionProvider(serviceLocator);
        }

        protected virtual IServiceLocator CreateServiceLocator()
        {
            return new ServiceLocator();
        }

        protected virtual void InitializeServices(IServiceLocator serviceLocator, ICompositionProvider compositionProvider)
        {
            //serviceLocator.Add<INavigationService>(new NavigationService(), ConflictResolvingOptions.Skip);
            //serviceLocator.Add<ILoggerService>(new DefaultLogger(), ConflictResolvingOptions.Skip);
            serviceLocator.Add<ICompositionProvider>(() => compositionProvider, ConflictResolvingOptions.Skip);
            serviceLocator.Add<IServiceLocator>(() => ServiceLocator, ConflictResolvingOptions.Replace);

            compositionProvider.Compose(ServiceLocator);
        }

        private void Initialize()
        {
            AttachEvents();

            ServiceLocator = CreateServiceLocator();
            m_CompositionProvider = CreateCompositionProvider(ServiceLocator);

            InitializeServices(ServiceLocator, m_CompositionProvider);

            IMVVMLocatorService mvvmLocator = ServiceLocator.Resolve<IMVVMLocatorService>();

            m_CompositionProvider.Compose(this);
        }

        private void AttachEvents()
        {
            this.Suspending += OnSuspending;
        }

        protected abstract Type InitializeShell();

        protected virtual Frame CreateRootFrame()
        {
            return new Frame();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            Frame rootFrame =  Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = CreateRootFrame();
                NavigationService.AttachToFrame(rootFrame);


                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }



            if (rootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                //if (!rootFrame.Navigate(typeof(MainPage), args.Arguments))
                if (!NavigationService.Navigate(InitializeShell(), "/?TestProperty=ololoItWorks!!!!!!"))//args.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }
            // Ensure the current window is active
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
