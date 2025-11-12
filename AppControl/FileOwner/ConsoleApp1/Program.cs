using ETWhelper;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;

// LOOK MA! NO Main() { ... }
using (ETWatcher etw = new()
{   // suppose your agent has DETECT vs PROTECT mode, you can switch handler on-the-fly...
    ProcessStartHandler = Handler_ProcessStart,
    ImageLoadHandler = Handler_ImageLoad
})
{
    string owner = GetOwner(@"C:\Windows\System32\cmd.exe");
    Console.WriteLine(owner);
    etw.Start();
    Console.ReadKey();
    //Thread.Sleep(Timeout.Infinite); // Keeps the process alive forever...
}

void Handler_ProcessStart(ProcessTraceData obj)
{                                     // notice ImageFileName has NO PATH...
    Console.WriteLine($"\nPID {obj.ProcessID} {obj.ImageFileName} started");
    Console.WriteLine($"Cmdline: {obj.PayloadByName("CommandLine")}");
}

void Handler_ImageLoad(ImageLoadTraceData obj)
{
    if (obj.FileName.ToLower().IndexOf(".dll") > 0) return;
    // We check file ownership EXE full path, skipping all DLLs for now
    try
    {
        if (IsTrusted(obj.FileName)) return;
        Console.WriteLine("UNTRUSTED: " + obj.FileName);
        Process.GetProcessById(obj.ProcessID).Kill();
    }
    catch (Exception ex) 
    {
        Console.WriteLine(ex.Message);
    }
}

bool IsTrusted(string filePath)
{
    string owner = GetOwner(filePath).ToLower();
    if (owner == string.Empty) return true; //cases where can't even access, means owned by higher privilege
    if (owner == @"nt service\trustedinstaller") return true;
    if (owner == @"nt authority\system") return true;
    if (owner == @"nt authority\network service") return true;
    if (owner == @"builtin\administrators") return true;
    return false;
}

string GetOwner(string filePath)
{
    try
    {
        var fileInfo = new FileInfo(filePath);

        // Optionally skip reparse points (symlinks)
        if ((fileInfo.Attributes & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint)
            return string.Empty;

        FileSecurity fileSecurity = fileInfo.GetAccessControl();
        IdentityReference sid = fileSecurity.GetOwner(typeof(SecurityIdentifier));
        NTAccount ntAccount = sid.Translate(typeof(NTAccount)) as NTAccount;
        return ntAccount?.Value ?? string.Empty;
    }
    catch (UnauthorizedAccessException)
    {
        return string.Empty;
    }
    catch (FileNotFoundException)
    {
        return string.Empty;
    }
    catch (IOException)
    {
        return string.Empty;
    }
    catch (Exception)
    {
        return string.Empty;
    }
}