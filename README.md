# Akka.NET Extensions [![NuGet](https://img.shields.io/nuget/v/SchJan.Akka.svg)](https://www.nuget.org/packages/SchJan.Akka) [![Build status](https://ci.appveyor.com/api/projects/status/sxe09hsa558mhv1b/branch/master?svg=true)](https://ci.appveyor.com/project/schjan/akka-net-extensions/branch/master) 
[![codecov.io](https://codecov.io/github/schjan/Akka.NET-Extensions/coverage.svg?branch=master)](https://codecov.io/github/schjan/Akka.NET-Extensions?branch=master)

(Usefull) extensions for [Akka.NET](https://github.com/akkadotnet/akka.net)

## Publish Subscribe Pattern
PublishMessageActor (as _TypedPublishMessageActorBase, UntypedPublishMessageActorBase_ or _PublishMessageReceiveActorBase_) enables Actors to publish predefined types of messages and handles the subscribers for you.

```csharp
//Publisher
[PublishMessage(typeof (ArrivingAtStationMessage))]
[PublishMessage(typeof (PositionChangedMessage))]
public class TrainActor : TypedPublishMessageActorBase
{
    //You can call
    PublishMessage(new ArrivingAtStationMessage("King's Cross"));
    //or
    PublishMessage(new PositionChangedMessage(...));
}

//subscriber
public class StationActor : TypedPublishMessageActorBase
{
    public StationActor()
    {
        TrainActorRef.Tell(new Subscribe(Self, typeof (ArrivingAtStationMessage));
    }
    
    public void Handle(ArrivingAtStationMessage message)
    {
        ...
    }
}
```
