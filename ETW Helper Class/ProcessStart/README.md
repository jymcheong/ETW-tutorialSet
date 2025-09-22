## What is it doing?
- Process start by default are NOT shown in Windows Event Viewer.
- This simple example shows PID (Process Identifier), file name (no path), and command line arguments & finally full path within ImageLoad event handler.
- If you remove the if line that filters out DLL paths, you will see the amount of DLLs that are loaded per process.

## Observations?
- Look at the event volume (how many lines printed out from the example Console app) before/after removing the DLL filter line
- Did ProcessStart handler provide full path? 
- [Which trace provider](https://github.com/microsoft/krabsetw/blob/master/docs/UsingMessageAnalyzerToFindETWSources.md) for what?

## What else can you try?
![](eventViewer.png)

- [Turn on 4688 audit event](https://www.perplexity.ai/search/i-need-powershell-commands-to-OZzHDW9TQmSRJGO_YbJ8CQ) & look at Windows Event Viewer
- You can also try Google / AI tools to see if you turn on security audit for DLL loads; you won't find such audit event because there's none.
- Look at the constructor for the helper class
- This is a building block example towards Application Control with ETW in a later part of the series.

https://github.com/microsoft/krabsetw/blob/master/docs/UsingMessageAnalyzerToFindETWSources.md is an excellent resource to learn how to use Microsoft Message Analyzer to find new ETW Sources!

>Next, you may want to look at the topic of [Parent Spoofing](../ParentSpoof/README.md)!
