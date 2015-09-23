#ServiceBus
Asynchronous abstaction library for Windows ServiceBus or Azure ServiceBus.

### Microsoft ServiceBus Reference
Windows ServiceBus 1.1 (on premises) and Azure Service Bus need different references, but define exactly the same dll `Microsoft.ServiceBus.dll`.

As we want to support both you need to manually install the ServiceBus dependency for your target system.

##### For Service Bus 1.1 for Windows Server
Use [Microsoft ServiceBus 1.1](https://www.nuget.org/packages/ServiceBus.v1_1/) or execute `Install-Package ServiceBus.v1_1`

##### For Azure Service Bus
Use [Microsoft Azure Service Bus](https://www.nuget.org/packages/WindowsAzure.ServiceBus/2.7.6) or execute `Install-Package WindowsAzure.ServiceBus -Version 2.7.6`

The version is defined explicitly as from on 3.x the library was refactored and can no longer be replaced.

### Usage
The integration and specification tests provide you the first step.

Documentation follows.