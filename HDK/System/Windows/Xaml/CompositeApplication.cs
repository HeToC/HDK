using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.UI.Xaml;

namespace System.Windows.Xaml
{
    public class CompositeApplication : Application
    {
        public CompositionHost Container { get; private set; }

        public CompositeApplication()
        {
            Compose();
        }

        private async Task<IEnumerable<Assembly>> GetAssemblyListAsync(StorageFolder folder)
        {
            List<Assembly> assemblies = new List<Assembly>();
            foreach (StorageFile file in await folder.GetFilesAsync())
            {
                if (file.FileType == ".dll" || file.FileType == ".exe")
                {
                    AssemblyName name = new AssemblyName() { Name = file.DisplayName };
                    Assembly asm = Assembly.Load(name);
                    assemblies.Add(asm);
                }
            }

            return assemblies;
        }

        private async void Compose()
        {
            var configuration = new ContainerConfiguration();
            var folder = Package.Current.InstalledLocation;
            var Result = await GetAssemblyListAsync(folder);

            foreach (var asm in Result)
            {
                Debug.WriteLine("Adding assembly ref: {0}", asm.FullName);

                configuration.WithAssembly(asm);
            }

            Container = configuration.CreateContainer();
            Container.SatisfyImports(this);
        }
    }
}
