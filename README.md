
>This is a personal project & DOES NOT in any way represent my current & previous employers.

## Roadmap
![](roadmap.png)
## What is this series about & who is it for?  
>This is NOT a step-by-step ETW tutorial, it is a sharing of practical experience & notes related ETW beyond audit log collection.

This series is for professionals who already understand Event Tracing for Windows (ETW) and work in technical roles such as:  
- Security product developers/engineers  
- Threat hunters and malware analysts  
- System programmers looking to pivot into security  
- Product managers/architects designing product capabilities  

  



***

## When to use ETW?  

### Beyond audit logging data-sets 
Windows audit events are often enough for basic detection and accountability. For those cases, ETW development won't make sense.  

ETW agent development makes sense when you need to:  
- Correlate different event types (e.g. process + network details).  
- Handle detection use-cases where audit data is insufficient:  
  - True Parent ProcessID is absent in EID-4688 and even Sysmon.  
  - File access auditing typically needs per-resource setup; even with Global Object Access Auditing, capturing everything for backend processing is impractical.  
- Reduce backend complexity and storage costs by pushing processing to endpoints.
- Detect & mitigate disarming of commercial EDRs, have a backup plan...

I will also share how to repurpose certain “fileless” offensive technique to simplify agent deployment and updates.  

***

### Near real-time response
Typical collect-to-analyze centrally workflow will incur a round-trip that is too high latency to be useful for near-real-time response. ETW provides the data to decide when to trigger actions on the endpoint instantly, either directly from events or after correlation within endpoint, avoiding the need to send a firehose of data to a backend.

This series will show how to use ETW for near-real-time application and egress control (as shown in the earlier roadmap), using high-level C# code, without kernel-level development.  

***

## Why C#?  
- Lower learning curve than driver development  
- Easier to maintain  
- No driver signing costs  
- No [BSOD risks](https://cloudsecurityalliance.org/blog/2025/07/03/what-we-can-learn-from-the-2024-crowdstrike-outage) (that brought global IT down) since we stay out of kernel space  

I will cover turning any C# console app into a SYSTEM process, and discuss the weaknesses of the common driver-based approach and how to overcome them with a simpler approach.  

***

## Alternatives  
### Windows Audit Events  
For near-real-time response, audit events may be too slow. But if your use case tolerates delays, you can tap into Windows audit events with just a few lines of C#, also covered in this series.  

### Sysmon

Sysmon excels as a monitoring tool when you need visibility beyond basic process creation and termination (like EID 4688 and 4689). It provides deep telemetry, such as detailed network connection events, that are otherwise unavailable or tedious to correlate through standard audit facilities. This series also includes an example of interacting directly with the Sysmon ETW provider, enabling you to tap into Sysmon's events programmatically for advanced correlation, custom detection, and near real-time response scenarios.

### Non-Microsoft Options

There’s no shortage of open-source ETW projects on GitHub, but one project stands out for its polish and active development: **Fibratus**. [Fibratus](https://github.com/rabbitstack/fibratus) is purpose-built for adversary detection, protection, and threat hunting using ETW. It covers a wide spectrum of system activity, provides a flexible detection rule engine, and supports real-time and forensic workflows. The project is mature, well-maintained, and highly extensible, making it a strong alternative to Sysmon for advanced defenders and developers seeking maximum control over system event instrumentation.

## What's Next?
The [next entry](ETW%20Helper%20Class/README.md) in this series will be on **Reusable Helper Class**. It will cover:
* Setup of development environment
* Explanation of the helper class design and usage
* Testing approaches

Feel free to fork or connect @ [LinkedIn](https://www.linkedin.com/in/jymcheong/)!