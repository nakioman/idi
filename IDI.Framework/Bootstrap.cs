using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Reflection;
using log4net;

namespace IDI.Framework
{
    public class Bootstrap
    {
        private static Bootstrap _instance;
        private CompositionContainer _container;
        private ILog _logger;
        private readonly SpeechRecognizerInfo _speechRecognizerInfo;
       
        private Bootstrap()
        {
            _speechRecognizerInfo = new SpeechRecognizerInfo();
            
            Compose();
            _container.SatisfyImportsOnce(_speechRecognizerInfo);
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

            _logger = LogManager.GetLogger("IDI");
            log4net.Config.XmlConfigurator.Configure();

            _container = new CompositionContainer(cat);
            _container.ComposeExportedValue(_logger);
            _container.ComposeParts(this);            
        }
    }
}