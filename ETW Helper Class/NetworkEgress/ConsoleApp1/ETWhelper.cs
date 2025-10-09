using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using System.Diagnostics;
using Microsoft.Diagnostics.Tracing;

namespace ETWhelper
{
    // generalised so that the specific logic are done in either Program or specific handler class method
    delegate void TraceHandler<TraceType>(TraceType traceData);

    public static class FilterHelper
    {
        public static Func<string, string, EventFilterResponse> CreateProviderFilter(
            string providerName,
            bool rejectNonMatchingProviders = true)
        {
            return (string currentProvider, string _) =>
                (currentProvider == providerName)
                    ? EventFilterResponse.AcceptEvent
                    : (rejectNonMatchingProviders
                        ? EventFilterResponse.RejectProvider
                        : EventFilterResponse.RejectEvent);
        }
    }

    class ETWatcher : IDisposable
    {
        TraceEventSession _session;
        string _sessionName = "ETWhelper_" + Process.GetCurrentProcess().ProcessName;
        public TraceHandler<ProcessTraceData> ?ProcessStartHandler;
        public TraceHandler<ProcessTraceData> ?ProcessStopHandler;
        public TraceHandler<ImageLoadTraceData> ?ImageLoadHandler;
        public TraceHandler<FileIONameTraceData> ?FileCreateHandler;
        public TraceHandler<TraceEvent> ?AllEventHandler;
        public TraceHandler<TraceEvent> ?NetworkEventHandler;
        public TraceHandler<TraceEvent> ?ParentSpoofHandler;
        public TraceHandler<TraceEvent> ?NameCreateHandler;
        public TraceHandler<TraceEvent> ?CrossProcessHandler;
        private bool _disposed = false;
       
        public ETWatcher()
        {
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true; // Prevent immediate exit
                Dispose();
            };
            _session = new TraceEventSession(_sessionName);
            _session.BufferQuantumKB = 128;

            // customise here to "listen" to various keywords e.g.  KernelTraceEventParser.Keywords.DiskFileIO |
            _session.EnableKernelProvider(KernelTraceEventParser.Keywords.Process |
                                          KernelTraceEventParser.Keywords.ImageLoad);

            // NT kernel logger GUID aka MSNT_SystemTrace provider, not really useful since ProcessID tend to be -1
            //_session.EnableProvider(Guid.Parse("{9E814AAD-3204-11D2-9A82-006008A86939}"), TraceEventLevel.Verbose, 0x10);
            _session.EnableProvider("Microsoft-Windows-Kernel-Process", TraceEventLevel.Verbose, (0x10 | 0x40));
            _session.EnableProvider("Microsoft-Windows-Kernel-Network", TraceEventLevel.Verbose, ulong.MaxValue);
            _session.EnableProvider("Microsoft-Windows-Kernel-File", matchAnyKeywords: (0x800 | 0x10));
            _session.EnableProvider("Microsoft-Windows-Kernel-Audit-API-Calls", TraceEventLevel.Verbose, 0x10);

            // setup handlers for respective events
            _session.Source.Kernel.ProcessStart += (data => ProcessStartHandler?.Invoke(data));
            _session.Source.Kernel.ProcessStop += (data => ProcessStopHandler?.Invoke(data));
            _session.Source.Kernel.ImageLoad += (data => ImageLoadHandler?.Invoke(data));
            //_session.Source.Kernel.FileIOFileCreate += (data => FileCreateHandler?.Invoke(data));

            // setup parsers for various specific events...
            var registeredParser = new RegisteredTraceEventParser(_session.Source);
            //registeredParser.All += (data => AllEventHandler?.Invoke(data)); // try to avoid .All since the method will end up very messy
            // register handler for 1 x specific EventName, you will need to create EventName correct otherwise handler won't run
            registeredParser.AddCallbackForProviderEvent("Microsoft-Windows-Kernel-File", "NameCreate", data => NameCreateHandler?.Invoke(data));
            // this ProcessStart allows tracking of parent spoofing, the previous Kernel.ProcessStart does not have the field
            registeredParser.AddCallbackForProviderEvent("Microsoft-Windows-Kernel-Process", "ProcessStart/Start", data => ParentSpoofHandler?.Invoke(data));
            // ID 5 == KERNEL_AUDIT_API_OPENPROCESS see https://www.perplexity.ai/search/research-microsoft-windows-ker-4IOnrf7gSWyjg_yaEsZb_g
            registeredParser.AddCallbackForProviderEvent("Microsoft-Windows-Kernel-Audit-API-Calls", "EventID(5)", data => CrossProcessHandler?.Invoke(data));
            // above filters specific events, this gets all events
            registeredParser.AddCallbackForProviderEvents(FilterHelper.CreateProviderFilter("Microsoft-Windows-Kernel-Network"),
                data => NetworkEventHandler?.Invoke(data)
            );
        }

        public void Start()
        {
            Debug.WriteLine("starting process ETW");
            Thread t = new Thread(() => { _session.Source.Process(); })
            {
                Priority = ThreadPriority.AboveNormal
            };
            t.Start();
        }
        public void Dispose()
        {
            if (!_disposed)
            {
                _session?.Stop();
                _session?.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        ~ETWatcher()
        {
            Dispose();
        }
        
    }
}
