using System.ComponentModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;

namespace IDI.Framework
{
    public class Bootstrap
    {
        private static Bootstrap _instance;
        private CompositionContainer _container;

        private Bootstrap()
        {
            Compose();
            _container.SatisfyImportsOnce(new SpeechRecognizerInfo());
        }

        public static Bootstrap Instance
        {
            get { return _instance ?? (_instance = new Bootstrap()); }
        }

        private void Compose()
        {
            var cat = new AggregateCatalog();            
            cat.Catalogs.Add(new DirectoryCatalog("Plugins"));
            cat.Catalogs.Add(new DirectoryCatalog(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)));

            _container = new CompositionContainer(cat);
            _container.ComposeParts(this);            
        }
    }
}