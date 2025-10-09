## Why Egress Matters?
Egress refers to the activities that happen when data leaves our network, whether it's going to the internet or within intranet. The question is, which process is responsible for these activities? 

>Could it be a system update that contacts Microsoft cloud infrastructure, or is it malware downloading more files to stay hidden?

## What typically happens after Code-Execution?
W.r.t to Tactics, Techniques & Procedures, the blue arrows below requires some form of network access:
![](../img/ttpEgress.png)￼

Initial Code-Execution may:
1. report to C2 (Command & Control) server so that attackers know they got into target
2. download more malware or backdoors to install into the system

As part of reconnaisance, attackers may scan internal networks for next or more targets...

Going through reports from https://thedfirreport.com & the [likes](https://www.perplexity.ai/search/other-sites-like-https-thedfir-H0hUsCD4SdmEru6yvjomAg#0), you will notice that almost all incidents that use some sort of malware, will almost involve at least one or more of those boxes above. 

[Mitre](https://attack.mitre.org) has a comprehensive enumeration of techniques per tactic, but this post is not about taxonomy.

>This post is about when & how to take advantage of ETW tracing for network egress.

## What to look out for?
1. Which process is accessing network?
2. What/where is the destination?

## Why these two?
- To narrow down any offending process(es) & respond (e.g. kill/block it).
- Block the bad destinations at the host &/or network level with firewall rules when we can't block at file level.
- A backdoor process controlled via the Internet may start scanning/accessing Intranet (e.g. org chart webpage),  **accessing both external & internal destinations simultaneously**: 
  - _how often does that occur in your environment?_
  - _which programs/processes are involved?_

>You can think of such processes with both Internet & Intranet network activities as **pivot processes**.

## How to track egress?
There's really no need for ETW when the objective is simply just recording for compliance or forensics.

Windows [audit event ID 5156 is your friend](https://www.perplexity.ai/search/how-to-turn-on-windows-audit-5-P.lrwnH2QHKOw6LUdOSD8g#0). 

>The common thing to do after gathering process destinations, is to match against ["bad" lists](https://www.perplexity.ai/search/which-is-the-most-active-and-w-9_fxwvxMQKm.KfwqU7HHZA#0).

## When will ETW be useful?
- Want some automated response that COTS can't do.
- Want custom profiling of process-network events.

## What does the ETW examples cover?
The simpler version within this folder introduces how to configure the ETW helper class to receive network events. 

In case you missed the comments within the example, the following powershell command will be useful for any ETW provider (in this case `Microsoft-Windows-Kernel-Network`):
```powershell
(Get-WinEvent -ListProvider Microsoft-Windows-Kernel-Network).Events | Select Id, Description

Id Description
-- -----------
10 TCPv4: %2 bytes transmitted from %4:%6 to %3:%5.
11 TCPv4: %2 bytes received from %4:%6 to %3:%5.
12 TCPv4: Connection attempted between %4:%6 and %3:%5.
13 TCPv4: Connection closed between %4:%6 and %3:%5.
14 TCPv4: %2 bytes retransmitted from %4:%6 to %3:%5.
15 TCPv4: Connection established between %4:%6 and %3:%5.
16 TCPv4: Reconnect attempt between %4:%6 and %3:%5.
17 TCPv4: Connection attempt failed with error code %2.
18 TCPv4: %2 bytes copied in protocol on behalf of user for connection between %4:%6 and %3:%5.
26 TCPv6: %2 bytes transmitted from %4:%6 to %3:%5.
27 TCPv6: %2 bytes received from %4:%6 to %3:%5.
28 TCPv6: Connection attempted between %4:%6 and %3:%5.
29 TCPv6: Connection closed between %4:%6 and %3:%5.
30 TCPv6: %2 bytes retransmitted from %4:%6 to %3:%5.
31 TCPv6: Connection established between %4:%6 and %3:%5.
32 TCPv6: Reconnect attempt between %4:%6 and %3:%5.
34 TCPv6: %2 bytes copied in protocol on behalf of user for connection between %4:%6 and %3:%5.
42 UDPv4: %2 bytes transmitted from %4:%6 to %3:%5.
43 UDPv4: %2 bytes received from %4:%6 to %3:%5.
49 UDPv4: Connection attempt failed with error code %2.
58 UDPv6: %2 bytes transmitted from %4:%6 to %3:%5.
59 UDPv6: %2 bytes received from %4:%6 to %3:%5.
```            
## What to do further to learn more?
- Which event IDs would your look at to figure out egress?
- What attributes you can get out of the above events that are not present in 5156 or Sysmon NetworkConnect?
- What kind of analysis can we do, beyond simplistic "known bad" matching?

## What's Next?
>Profiling of process-network events.