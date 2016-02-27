using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Win32;
using ASCOM;

namespace AstroSimulator
{
    public partial class App : Application
    {
        #region API access

        [Flags]
        enum CLSCTX : uint
        {
            CLSCTX_INPROC_SERVER = 0x1,
            CLSCTX_INPROC_HANDLER = 0x2,
            CLSCTX_LOCAL_SERVER = 0x4,
            CLSCTX_INPROC_SERVER16 = 0x8,
            CLSCTX_REMOTE_SERVER = 0x10,
            CLSCTX_INPROC_HANDLER16 = 0x20,
            CLSCTX_RESERVED1 = 0x40,
            CLSCTX_RESERVED2 = 0x80,
            CLSCTX_RESERVED3 = 0x100,
            CLSCTX_RESERVED4 = 0x200,
            CLSCTX_NO_CODE_DOWNLOAD = 0x400,
            CLSCTX_RESERVED5 = 0x800,
            CLSCTX_NO_CUSTOM_MARSHAL = 0x1000,
            CLSCTX_ENABLE_CODE_DOWNLOAD = 0x2000,
            CLSCTX_NO_FAILURE_LOG = 0x4000,
            CLSCTX_DISABLE_AAA = 0x8000,
            CLSCTX_ENABLE_AAA = 0x10000,
            CLSCTX_FROM_DEFAULT_CONTEXT = 0x20000,
            CLSCTX_INPROC = CLSCTX_INPROC_SERVER | CLSCTX_INPROC_HANDLER,
            CLSCTX_SERVER = CLSCTX_INPROC_SERVER | CLSCTX_LOCAL_SERVER | CLSCTX_REMOTE_SERVER,
            CLSCTX_ALL = CLSCTX_SERVER | CLSCTX_INPROC_HANDLER
        }

        [Flags]
        enum COINIT : uint
        {
            /// Initializes the thread for multi-threaded object concurrency.
            COINIT_MULTITHREADED = 0x0,
            /// Initializes the thread for apartment-threaded object concurrency. 
            COINIT_APARTMENTTHREADED = 0x2,
            /// Disables DDE for Ole1 support.
            COINIT_DISABLE_OLE1DDE = 0x4,
            /// Trades memory for speed.
            COINIT_SPEED_OVER_MEMORY = 0x8
        }

        [Flags]
        enum REGCLS : uint
        {
            REGCLS_SINGLEUSE = 0,
            REGCLS_MULTIPLEUSE = 1,
            REGCLS_MULTI_SEPARATE = 2,
            REGCLS_SUSPENDED = 4,
            REGCLS_SURROGATE = 8
        }

        // CoInitializeEx() can be used to set the apartment model
        // of individual threads.
        [DllImport("ole32.dll")]
        static extern int CoInitializeEx(IntPtr pvReserved, uint dwCoInit);

        // CoUninitialize() is used to uninitialize a COM thread.
        [DllImport("ole32.dll")]
        static extern void CoUninitialize();

        // PostThreadMessage() allows us to post a Windows Message to
        // a specific thread (identified by its thread id).
        // We will need this API to post a WM_QUIT message to the main 
        // thread in order to terminate this application.
        [DllImport("user32.dll")]
        static extern bool PostThreadMessage(uint idThread, uint Msg, UIntPtr wParam,
            IntPtr lParam);

        // GetCurrentThreadId() allows us to obtain the thread id of the
        // calling thread. This allows us to post the WM_QUIT message to
        // the main thread.
        [DllImport("kernel32.dll")]
        static extern uint GetCurrentThreadId();

        #endregion

        // Application ID
        private const String _APPID = "{B0DED76D-3B85-4E05-B137-05FD66DD3E32}";

        // ASCOM drivers
        private class ASCOMDriver
        {
            public String ASCOMType { get; private set; }
            public Type Driver { get; private set; }
            public ASCOMDriver(String ascomType, Type driver)
            {
                ASCOMType = ascomType;
                Driver = driver;
            }
        }

        private readonly ASCOMDriver[] _ascomDrivers = {
            new ASCOMDriver("Camera", typeof(CameraDriver)),
            new ASCOMDriver("Camera", typeof(GuiderDriver)),
            new ASCOMDriver("FilterWheel", typeof(FilterWheelDriver)),
            new ASCOMDriver("Telescope", typeof(TelescopeDriver)),
            new ASCOMDriver("Focuser", typeof(FocuserDriver))
        };

        // Main thread ID and flag to indicate if server started by COM
        public UInt32 MainThreadId { get; private set; }
        public Boolean StartedByCOM { get; private set; }
        private List<ClassFactory> _classFactories;
        private GarbageCollection _garbageCollector;
        private Int32 _objectCount = 0;
        private Int32 _lockCount = 0;
        public GuideStarCatalog GSC { get; private set; }
        public Preferences Preferences { get; private set; }
        public SiteInformation SiteInformation { get; private set; }
        public Camera Camera { get; private set; }
        public FilterWheel FilterWheel { get; private set; }
        public Guider Guider { get; private set; }
        public Telescope Telescope { get; private set; }
        public Focuser Focuser { get; private set; }

        // Lifetime management
        public void IncrementObjectCount()
        {
            lock (this)
            {
                ++_objectCount;
            }
        }

        public void DecrementObjectCount()
        {
            lock (this)
            {
                if (--_objectCount <= 0)
                    CheckTerminate();
            }
        }

        public void IncrementLockCount()
        {
            lock (this)
            {
                ++_lockCount;
            }
        }

        public void DecrementLockCount()
        {
            lock (this)
            {
                if (--_lockCount <= 0)
                    CheckTerminate();
            }
        }

        private void CheckTerminate()
        {
            // If started by COM and both object count and lock count are zero terminate
            // server by sending WM_QUIT message
            if (StartedByCOM && (_objectCount <= 0) && (_lockCount <= 0))
                Application.Current.Dispatcher.BeginInvoke((Action)delegate() { Application.Current.Shutdown(); });
        }

        // COM registration
        private void RegisterServer()
        {
            Console.WriteLine("Registering server ...");
            try
            {
                // Get assembly information
                Assembly assembly = Assembly.GetExecutingAssembly();
                String assemblyTitle = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute))).Title;
                String assemblyDescription = ((AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute))).Description;
                String assemblyPath = assembly.Location;

                // Add registry entries for HKCR\APPID\appid
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(String.Format("APPID\\{0}", _APPID)))
                {
                    key.SetValue(null, assemblyDescription);
                    key.SetValue("AppID", _APPID);
                    key.SetValue("AuthenticationLevel", 1, RegistryValueKind.DWord);
                }

                // Add registry entries for HKCR\APPID\exename.ext
                using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(String.Format("APPID\\{0}", assemblyPath.Substring(assemblyPath.LastIndexOf('\\') + 1))))
                    key.SetValue("AppId", _APPID);
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("Unexpected exception registering server: {0}", e.Message));
            }
        }

        private void RegisterASCOMDrivers()
        {
            Console.WriteLine("Registering ASCOM drivers ...");

            // Get assembly information
            Assembly assembly = Assembly.GetExecutingAssembly();
            String assemblyTitle = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute))).Title;
            String assemblyDescription = ((AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute))).Description;

            // Register ASCOM drivers
            foreach (ASCOMDriver driver in _ascomDrivers)
            {
                try
                {
                    // Get CLSID and PROGID from metadata
                    String clsid = Marshal.GenerateGuidForType(driver.Driver).ToString("B");
                    String progid = Marshal.GenerateProgIdForType(driver.Driver);

                    // Add registry entries for HKCR\CLSID\clsid
                    using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(String.Format("CLSID\\{0}", clsid)))
                    {
                        key.SetValue(null, progid);
                        key.SetValue("AppId", _APPID);
                        using (RegistryKey key2 = key.CreateSubKey("Implemented Categories"))
                            key2.CreateSubKey("{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}");
                        using (RegistryKey key2 = key.CreateSubKey("ProgId"))
                            key2.SetValue(null, progid);
                        key.CreateSubKey("Programmable");
                        using (RegistryKey key2 = key.CreateSubKey("LocalServer32"))
                            key2.SetValue(null, assembly.Location);
                    }

                    // Add registry entries for HKCR\progid
                    using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(progid))
                    {
                        key.SetValue(null, assemblyTitle);
                        using (RegistryKey key2 = key.CreateSubKey("CLSID"))
                            key2.SetValue(null, clsid);
                    }

                    // Register ASCOM driver
                    String chooserName = ((ServedClassNameAttribute)Attribute.GetCustomAttribute(driver.Driver, typeof(ServedClassNameAttribute))).DisplayName;
                    using (ASCOM.Utilities.Profile profile = new ASCOM.Utilities.Profile())
                    {
                        profile.DeviceType = driver.ASCOMType;
                        profile.Register(progid, chooserName);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(String.Format("Unexpected exception registering class {0}: {1}", driver.Driver.FullName, e.Message));
                }
            }
        }

        private void UnregisterASCOMDrivers()
        {
            Console.WriteLine("Unregistering ASCOM drivers ...");

            // Unregister ASCOM drivers
            foreach (ASCOMDriver driver in _ascomDrivers)
            {
                try
                {
                    // Get CLSID and PROGID from metadata
                    String clsid = Marshal.GenerateGuidForType(driver.Driver).ToString("B");
                    String progid = Marshal.GenerateProgIdForType(driver.Driver);

                    // Remove registry entries for HKCR\CLSID\progid
                    Registry.ClassesRoot.DeleteSubKey(String.Format("{0}\\CLSID", progid), false);
                    Registry.ClassesRoot.DeleteSubKey(progid, false);

                    // Remove registry entries for HKCR\CLSID\clsid
                    Registry.ClassesRoot.DeleteSubKey(String.Format("CLSID\\{0}\\Implemented Categories\\{{62C8FE65-4EBB-45e7-B440-6E39B2CDBF29}}", clsid), false);
                    Registry.ClassesRoot.DeleteSubKey(String.Format("CLSID\\{0}\\Implemented Categories", clsid), false);
                    Registry.ClassesRoot.DeleteSubKey(String.Format("CLSID\\{0}\\ProgId", clsid), false);
                    Registry.ClassesRoot.DeleteSubKey(String.Format("CLSID\\{0}\\LocalServer32", clsid), false);
                    Registry.ClassesRoot.DeleteSubKey(String.Format("CLSID\\{0}\\Programmable", clsid), false);
                    Registry.ClassesRoot.DeleteSubKey(String.Format("CLSID\\{0}", clsid), false);

                    // Unregister ASCOM driver
                    using (ASCOM.Utilities.Profile profile = new ASCOM.Utilities.Profile())
                    {
                        profile.DeviceType = driver.ASCOMType;
                        profile.Unregister(progid);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(String.Format("Unexpected exception unregistering class {0}: {1}", driver.Driver.FullName, e.Message));
                }
            }
        }

        private void UnregisterServer()
        {
            Console.WriteLine("Unregistering server ...");
            try
            {
                // Get assembly information
                Assembly assembly = Assembly.GetExecutingAssembly();
                String assemblyTitle = ((AssemblyTitleAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyTitleAttribute))).Title;
                String assemblyDescription = ((AssemblyDescriptionAttribute)Attribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute))).Description;
                String assemblyPath = assembly.Location;

                // Remove registry entries for HKCR\APPID\appid
                Registry.ClassesRoot.DeleteSubKey(String.Format("APPID\\{0}\\AuthenticationLevel", _APPID), false);
                Registry.ClassesRoot.DeleteSubKey(String.Format("APPID\\{0}\\AppId", _APPID), false);
                Registry.ClassesRoot.DeleteSubKey(String.Format("APPID\\{0}", _APPID), false);

                // Remove registry entries for HKCR\APPID\exename.ext
                Registry.ClassesRoot.DeleteSubKey(String.Format("APPID\\{0}", assemblyPath.Substring(assemblyPath.LastIndexOf('\\') + 1)), false);
            }
            catch (Exception e)
            {
                MessageBox.Show(String.Format("Unexpected exception unregistering server: {0}", e.Message));
            }
        }

        // COM class factory registration
        private void RegisterCOMClassFactories()
        {
            _classFactories = new List<ClassFactory>();
            foreach (ASCOMDriver driver in _ascomDrivers)
            {
                ClassFactory factory = new ClassFactory(driver.Driver);
                _classFactories.Add(factory);
                if (!factory.RegisterClassObject())
                    MessageBox.Show(String.Format("Error registering class factory for {0}", driver.Driver.FullName));
            }
            ClassFactory.ResumeClassObjects();
        }

        private void UnregisterCOMClassFactories()
        {
            ClassFactory.SuspendClassObjects();
            foreach (ClassFactory factory in _classFactories)
                factory.RevokeClassObject();
        }

        // Process command line arguments
        private Boolean ProcessCommandLineArgs(String[] args)
        {
            StartedByCOM = false;
            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "-embedding":
                        StartedByCOM = true;
                        return false;

                    case "-register":
                    case "/register":
                    case "-regserver":
                    case "/regserver":
                        RegisterServer();
                        RegisterASCOMDrivers();
                        return true;

                    case "-unregister":
                    case "/unregister":
                    case "-unregserver":
                    case "/unregserver":
                        UnregisterASCOMDrivers();
                        UnregisterServer();
                        return true;
                }
            }
            return false;
        }

        // Application lifecycle events
        protected override void OnStartup(StartupEventArgs e)
        {
            // Call base implementation
            base.OnStartup(e);
            log4net.Config.XmlConfigurator.Configure();

            // Banner
            Console.WriteLine("AstroSimulator");

            // Process command line arguments
            if (ProcessCommandLineArgs(e.Args))
                Shutdown();
            else
            {
                // Create preferences and site information
                Preferences = new Preferences();
                Preferences.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(Preferences_PropertyChanged);
                SiteInformation = new SiteInformation();

                // Create ASCOM objects
                Camera = new Camera();
                FilterWheel = new FilterWheel();
                Guider = new Guider();
                Telescope = new Telescope();
                Focuser = new Focuser();

                // Initialize guide start catalog
                if (!String.IsNullOrEmpty(Preferences.GSCPath))
                    GSC = new GuideStarCatalog(Preferences.GSCPath);

                // Register COM class factories
                RegisterCOMClassFactories();

                // Start up the garbage collection thread.
                _garbageCollector = new GarbageCollection(1000);
                Thread GCThread = new Thread(new ThreadStart(_garbageCollector.GCWatch));
                GCThread.Name = "Garbage Collection Thread";
                GCThread.Start();
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Remove COM class factories
            if (_classFactories != null)
                UnregisterCOMClassFactories();

            // Stop garbage collection thread
            if (_garbageCollector != null)
            {
                _garbageCollector.StopThread();
                _garbageCollector.WaitForThreadToStop();
            }

            // Call base implementation
            base.OnExit(e);
        }

        // Event handlers
        void Preferences_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "GSCPath")
                GSC = (String.IsNullOrEmpty(Preferences.GSCPath) ? null : new GuideStarCatalog(Preferences.GSCPath));
        }

        // Logging
        public void LogMessage(String message, params object[] args)
        {
            Dispatcher.Invoke((Action)delegate() { ((MainWindow)MainWindow).LogMessage(String.Format(message, args)); });
        }
    }
}
