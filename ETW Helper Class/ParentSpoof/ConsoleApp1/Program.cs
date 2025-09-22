using ETWhelper;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;

// LOOK MA! NO Main() { ... }
using (ETWatcher etw = new()
{   // suppose your agent has DETECT vs PROTECT mode, you can switch handler on-the-fly...
    ProcessStartHandler = Handler_ProcessStart,
    ParentSpoofHandler = Handler_ParentSpoof
})
{
    etw.Start();
    Console.ReadKey();
    //Thread.Sleep(Timeout.Infinite); // Keeps the process alive forever...
}  

void Handler_ProcessStart(ProcessTraceData obj)
{                                     // notice ImageFileName has NO PATH...
    Console.WriteLine($"\nPID {obj.ProcessID} {obj.ImageFileName} started");
    Console.WriteLine($"Cmdline: {obj.PayloadByName("CommandLine") }");
    Console.WriteLine($"Parent PID: {obj.PayloadByName("ParentID") }");
}

void Handler_ParentSpoof(TraceEvent obj)
{   // this data.ProcessID refers to the parent/creator, whereas data.PayloadByName("ProcessID") refers to the NEW/child process
    
    if (obj.ProcessID == (int)obj.PayloadByName("ParentProcessID")) return;

    Console.WriteLine($"\nTrue PPID is {obj.ProcessID} but reported PPID is {(int)obj.PayloadByName("ParentProcessID")}");
}