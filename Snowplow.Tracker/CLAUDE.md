# Snowplow.Tracker Module - CLAUDE.md

## Module Overview

The core Snowplow.Tracker library implementing event tracking functionality for .NET applications. This module provides the main Tracker singleton, event models, emitters, storage mechanisms, and HTTP endpoint communication. It follows strict patterns for immutability, builder design, and interface-based abstractions.

## Module Structure

```
Snowplow.Tracker/
├── Tracker.cs              # Main singleton tracker orchestrator
├── Constants.cs            # All string constants and schemas
├── Utils.cs               # Helper utilities (timestamps, GUIDs)
├── Version.cs             # Version information
├── Models/                # Event and data models
│   ├── Events/           # Event type implementations
│   ├── Contexts/         # Context data structures
│   └── Adapters/         # Serialization adapters
├── Emitters/             # Event transmission layer
├── Endpoints/            # Collector communication
├── Storage/              # Persistence layer
├── Queues/               # Queue implementations
└── Logging/              # Logging abstractions
```

## Critical Patterns & Conventions

### Constants Management
```csharp
// ✅ Use Constants class for all string literals
payload.Add(Constants.EVENT, Constants.EVENT_PAGE_VIEW);

// ❌ Don't hardcode string values
payload.Add("e", "pv"); // Use constants instead
```

### Schema Definitions
```csharp
// ✅ Reference schema constants
SetSchema(Constants.SCHEMA_DESKTOP);

// ❌ Don't hardcode Iglu schemas
SetSchema("iglu:com.snowplowanalytics.snowplow/desktop_context/jsonschema/1-0-0");
```

### Event Model Pattern
```csharp
// ✅ Follow AbstractEvent<T> pattern
public class CustomEvent : AbstractEvent<CustomEvent>
{
    public override CustomEvent Self() => this;
    public override CustomEvent Build() { /* validate */ return this; }
    public override IPayload GetPayload() { /* create payload */ }
}

// ❌ Don't implement IEvent directly
public class CustomEvent : IEvent { } // Missing base functionality
```

## Models Layer Implementation

### Event Builder Pattern
Every event class must follow this structure:
```csharp
public class EventName : AbstractEvent<EventName>
{
    // 1. Private fields for required data
    private string requiredField;
    
    // 2. Builder methods return Self()
    public EventName SetField(string value) {
        this.requiredField = value;
        return Self();
    }
    
    // 3. Build() validates and finalizes
    public override EventName Build() {
        if (string.IsNullOrEmpty(requiredField))
            throw new ArgumentException("Field required");
        return this;
    }
    
    // 4. GetPayload() creates the payload
    public override IPayload GetPayload() {
        var payload = new Payload();
        AddDefaultPairs(payload);
        payload.Add(Constants.FIELD_KEY, requiredField);
        return payload;
    }
}
```

### Context Implementation
```csharp
// ✅ Extend AbstractContext with proper schema
public class CustomContext : AbstractContext<CustomContext>
{
    public override CustomContext Build() {
        SetSchema("iglu:com.company/context/jsonschema/1-0-0");
        SetData(new Dictionary<string,object> { 
            {"key", value} 
        });
        return this;
    }
}

// ❌ Don't forget schema in Build()
public override CustomContext Build() {
    SetData(data); // Missing SetSchema()
    return this;
}
```

### Payload Construction
```csharp
// ✅ Use Payload class methods
var payload = new Payload();
payload.Add(Constants.EVENT, eventType);
payload.AddDict(additionalData);

// ❌ Don't manipulate internal dictionary
payload.Payload["key"] = value; // Use Add() method
```

## Emitters Layer

### AsyncEmitter Implementation
```csharp
// ✅ Proper AsyncEmitter setup
var queue = new PersistentBlockingQueue(storage);
var emitter = new AsyncEmitter(
    endpoint: endpoint,
    queue: queue,
    sendLimit: 100,        // Batch size
    stopPollIntervalMs: 300 // Shutdown poll interval
);

// ❌ Don't use blocking operations
emitter.SendEvents(events); // Blocks thread
```

### Emitter Lifecycle
```csharp
// ✅ Manage emitter lifecycle properly
emitter.Start();
// ... track events ...
emitter.Stop(); // Graceful shutdown
emitter.Dispose(); // Clean up resources

// ❌ Don't forget cleanup
emitter.Start();
// Missing Stop() and Dispose()
```

## Storage Layer

### LiteDB Storage Pattern
```csharp
// ✅ Use IStorage interface
IStorage storage = new LiteDBStorage(dbPath);

// ❌ Don't expose LiteDB specifics
var db = storage.GetDatabase(); // Breaks abstraction
```

### Storage Operations
```csharp
// ✅ Atomic operations
storage.Put(eventRecord);
var record = storage.Get();
storage.Delete(record.Id);

// ❌ Don't batch inappropriately
foreach(var e in events) storage.Put(e); // Use transaction
```

## Endpoint Communication

### HTTP Endpoint Configuration
```csharp
// ✅ Configure endpoint properly
var endpoint = new SnowplowHttpCollectorEndpoint(
    host: "collector.domain.com",
    protocol: HttpProtocol.HTTPS,
    port: 443,
    method: HttpMethod.POST
);

// ❌ Don't hardcode URLs
var url = "https://collector.domain.com/com.snowplowanalytics.snowplow/tp2";
```

### Request Construction
```csharp
// ✅ Let endpoint build requests
var request = endpoint.GetRequest(payload);

// ❌ Don't construct manually
var request = new HttpRequestMessage();
request.RequestUri = new Uri(url); // Use endpoint
```

## Queue Management

### Persistent Queue Usage
```csharp
// ✅ Use IPersistentBlockingQueue
IPersistentBlockingQueue queue = new PersistentBlockingQueue(
    storage: storage,
    logger: logger
);

// ❌ Don't use in-memory for production
var queue = new InMemoryBlockingQueue(); // Data loss risk
```

## Logging Integration

### Logger Usage
```csharp
// ✅ Use ILogger interface
ILogger logger = new ConsoleLogger();
logger.Info("Event tracked");

// ❌ Don't use Console directly
Console.WriteLine("Event tracked"); // Not configurable
```

## Common Implementation Errors

### 1. Missing Self() Implementation
```csharp
// ❌ Forgetting Self() method
public override EventName Self() { 
    return null; // Must return this
}

// ✅ Correct Self() implementation
public override EventName Self() => this;
```

### 2. Mutable Event State
```csharp
// ❌ Public setters on events
public string PageUrl { get; set; }

// ✅ Private fields with builder methods
private string pageUrl;
public EventName SetPageUrl(string url) { 
    pageUrl = url; 
    return Self(); 
}
```

### 3. Schema Versioning
```csharp
// ❌ Wrong schema version format
"iglu:com.company/event/jsonschema/1.0.0"

// ✅ Correct format (major-minor-patch)
"iglu:com.company/event/jsonschema/1-0-0"
```

## Testing Guidelines

### Event Testing Pattern
```csharp
[TestMethod]
public void testEventConstruction()
{
    var event = new EventType()
        .SetRequiredField("value")
        .Build();
    
    var payload = event.GetPayload();
    Assert.AreEqual("value", payload.Payload["field"]);
}

[TestMethod]
[ExpectedException(typeof(ArgumentException))]
public void testMissingRequiredField()
{
    new EventType().Build(); // Should throw
}
```

## Quick Reference - Module Specifics

### Key Classes
- `Tracker`: Main singleton orchestrator
- `AsyncEmitter`: Background event transmission
- `SnowplowHttpCollectorEndpoint`: HTTP collector communication
- `LiteDBStorage`: Persistent event storage
- `Payload`: Event data container

### Required Constants Keys
```csharp
Constants.EVENT           // Event type
Constants.EID            // Event ID
Constants.TIMESTAMP      // Device timestamp
Constants.TRUE_TIMESTAMP // True timestamp
Constants.CONTEXT        // Context data
Constants.APP_ID         // Application ID
```

### Event Type Values
```csharp
Constants.EVENT_PAGE_VIEW      // "pv"
Constants.EVENT_STRUCTURED     // "se"
Constants.EVENT_UNSTRUCTURED   // "ue"
Constants.EVENT_ECOMM          // "tr"
Constants.EVENT_ECOMM_ITEM     // "ti"
```

## Module-Specific Patterns

### Thread Safety
```csharp
// ✅ Use locks for shared state
private readonly object _lock = new object();
lock (_lock) { 
    // Critical section
}

// ❌ Don't access shared state without synchronization
_running = true; // Race condition
```

### Resource Management
```csharp
// ✅ Implement IDisposable properly
public class Component : IDisposable
{
    private bool _disposed = false;
    
    public void Dispose()
    {
        if (!_disposed)
        {
            // Clean up
            _disposed = true;
        }
    }
}
```

### Validation Patterns
```csharp
// ✅ Validate in Build() method
public override T Build()
{
    if (string.IsNullOrEmpty(required))
        throw new ArgumentException("Field required");
    return this;
}

// ❌ Don't validate in setters
public T SetField(string value)
{
    if (string.IsNullOrEmpty(value)) // Too early
        throw new ArgumentException();
}
```