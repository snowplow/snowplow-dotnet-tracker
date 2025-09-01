# Snowplow .NET Tracker - CLAUDE.md

## Project Overview

The Snowplow .NET Tracker is a comprehensive analytics library that enables event tracking in .NET applications. It sends structured and unstructured events to Snowplow collectors for data analytics pipelines. The tracker supports multiple platforms (.NET Standard 1.4/2.0) and provides async/sync event emission, persistent storage, and rich context capabilities.

**Key Technologies:**
- .NET Standard 1.4/2.0
- MSTest for unit testing
- LiteDB for persistent storage
- Newtonsoft.Json for serialization
- NuGet package distribution

## Development Commands

```bash
# Build the main tracker library
dotnet build Snowplow.Tracker/Snowplow.Tracker.csproj

# Run all tests
dotnet test Snowplow.Tracker.Tests/Snowplow.Tracker.Tests.csproj

# Create NuGet package
dotnet pack Snowplow.Tracker/Snowplow.Tracker.csproj -c Release

# Run demo application
dotnet run --project Snowplow.Demo.Console/Snowplow.Demo.Console.csproj
```

## Architecture

### System Design
The tracker follows a layered architecture with clear separation of concerns:
- **Models Layer**: Event types, contexts, and payloads (immutable DTOs)
- **Emitters Layer**: Async/sync event transmission to collectors
- **Storage Layer**: Persistent queue implementation using LiteDB
- **Endpoints Layer**: HTTP collector communication abstraction
- **Core Tracker**: Singleton orchestrator managing the event pipeline

### Module Organization
```
Snowplow.Tracker/
├── Models/          # Event models and data structures
│   ├── Events/      # Event type implementations
│   ├── Contexts/    # Context data structures
│   └── Adapters/    # Payload serialization
├── Emitters/        # Event transmission logic
├── Storage/         # Persistence layer
├── Endpoints/       # Collector communication
├── Queues/          # Event queueing mechanisms
└── Logging/         # Logging abstractions
```

## Core Architectural Principles

### 1. Singleton Tracker Pattern
```csharp
// ✅ Use the singleton instance
var tracker = Tracker.Instance;
tracker.Start(endpoint, dbPath, method);

// ❌ Don't create new instances
var tracker = new Tracker(); // Constructor is private
```

### 2. Builder Pattern for Events
```csharp
// ✅ Use fluent builders for event construction
var pageView = new PageView()
    .SetPageUrl("https://example.com")
    .SetPageTitle("Example")
    .Build();

// ❌ Don't use property setters after Build()
pageView.PageUrl = "new-url"; // Immutable after build
```

### 3. Interface-Driven Design
```csharp
// ✅ Program against interfaces
IEvent event = new PageView().Build();
IEmitter emitter = new AsyncEmitter(...);

// ❌ Don't couple to concrete implementations
AsyncEmitter emitter = tracker.GetEmitter(); // Exposes internals
```

### 4. Immutable Event Models
```csharp
// ✅ Configure events through builders
var event = new Structured()
    .SetCategory("ui")
    .SetAction("click")
    .Build(); // Immutable after this

// ❌ Don't modify events after creation
event.Category = "new-category"; // Not allowed
```

## Layer Organization & Responsibilities

### Models Layer (`Snowplow.Tracker.Models`)
- **Events**: PageView, Structured, SelfDescribing, EcommerceTransaction
- **Contexts**: Desktop, Mobile, GeoLocation, Session contexts
- **Payloads**: Key-value dictionaries with schema validation
- All models implement builder pattern with `Build()` finalization

### Emitters Layer (`Snowplow.Tracker.Emitters`)
- **AsyncEmitter**: Background thread processing with retry logic
- **IEmitter**: Interface for custom emitter implementations
- Handles batching, network failures, and exponential backoff

### Storage Layer (`Snowplow.Tracker.Storage`)
- **LiteDBStorage**: Persistent event queue using LiteDB
- **IStorage**: Interface for custom storage backends
- Ensures events survive app restarts

### Endpoints Layer (`Snowplow.Tracker.Endpoints`)
- **SnowplowHttpCollectorEndpoint**: HTTP/HTTPS collector communication
- Supports GET and POST methods with appropriate URI suffixes
- Handles protocol selection and request construction

## Critical Import Patterns

### Standard Namespace Structure
```csharp
// ✅ Follow namespace hierarchy
using Snowplow.Tracker;
using Snowplow.Tracker.Models.Events;
using Snowplow.Tracker.Models.Contexts;

// ❌ Don't use aliasing for tracker namespaces
using Events = Snowplow.Tracker.Models.Events; // Confusing
```

### Event Model Imports
```csharp
// ✅ Import specific event types needed
using Snowplow.Tracker.Models.Events;
var pageView = new PageView().Build();

// ❌ Don't import individual event classes
using PageView = Snowplow.Tracker.Models.Events.PageView;
```

## Essential Library Patterns

### Tracker Initialization
```csharp
// ✅ Proper initialization sequence
var endpoint = new SnowplowHttpCollectorEndpoint("collector.domain.com");
Tracker.Instance.Start(endpoint, "events.db", HttpMethod.POST);

// ❌ Don't start without configuration
Tracker.Instance.Track(event); // Must call Start() first
```

### Event Tracking
```csharp
// ✅ Build event then track
var event = new Structured()
    .SetCategory("video")
    .SetAction("play")
    .Build();
Tracker.Instance.Track(event);

// ❌ Don't track incomplete events
Tracker.Instance.Track(new Structured()); // Missing required fields
```

### Context Management
```csharp
// ✅ Attach contexts to events
var contexts = new List<IContext> {
    new DesktopContext().Build(),
    new SessionContext("user-123", "session-456").Build()
};
event.SetCustomContext(contexts);

// ❌ Don't mix context types incorrectly
event.SetCustomContext(desktopContext); // Expects List<IContext>
```

## Model Organization Pattern

### Event Hierarchy
```
IEvent (interface)
└── AbstractEvent<T> (base class with common fields)
    ├── PageView (page view tracking)
    ├── Structured (custom structured events)
    ├── SelfDescribing (unstructured JSON events)
    ├── EcommerceTransaction (transaction events)
    └── Timing (performance timing events)
```

### Context Types
```
IContext (interface)
└── AbstractContext<T> (base context implementation)
    ├── DesktopContext (OS and hardware info)
    ├── MobileContext (mobile device specifics)
    ├── GeoLocationContext (GPS coordinates)
    └── SessionContext (user session data)
```

## Common Pitfalls & Solutions

### 1. Forgetting to Start Tracker
```csharp
// ❌ Tracking without initialization
var event = new PageView().SetPageUrl("url").Build();
Tracker.Instance.Track(event); // Throws exception

// ✅ Always start first
Tracker.Instance.Start(endpoint, dbPath, method);
Tracker.Instance.Track(event);
```

### 2. Missing Required Event Fields
```csharp
// ❌ PageView without URL
new PageView().Build(); // Throws ArgumentException

// ✅ Provide all required fields
new PageView().SetPageUrl("https://example.com").Build();
```

### 3. Incorrect Context Usage
```csharp
// ❌ Single context instead of list
event.SetCustomContext(new DesktopContext());

// ✅ Always use List<IContext>
event.SetCustomContext(new List<IContext> { 
    new DesktopContext().Build() 
});
```

### 4. Synchronous Operations in UI Thread
```csharp
// ❌ Blocking UI thread
Tracker.Instance.Flush(); // Blocks until all events sent

// ✅ Use async emitter for background processing
var emitter = new AsyncEmitter(endpoint, queue);
```

## File Structure Template

### New Event Type
```csharp
// Models/Events/CustomEvent.cs
namespace Snowplow.Tracker.Models.Events
{
    public class CustomEvent : AbstractEvent<CustomEvent>
    {
        // Required fields with builders
        public CustomEvent SetRequiredField(string value) { }
        
        // Build finalization
        public override CustomEvent Build() { }
        
        // Payload generation
        public override IPayload GetPayload() { }
    }
}
```

### New Context Type
```csharp
// Models/Contexts/CustomContext.cs
namespace Snowplow.Tracker.Models.Contexts
{
    public class CustomContext : AbstractContext<CustomContext>
    {
        // Context-specific data
        public CustomContext SetContextData(string value) { }
        
        // Schema definition
        public override CustomContext Build() 
        {
            SetSchema("iglu:com.acme/context/1-0-0");
            return this;
        }
    }
}
```

## Testing Patterns

### Unit Test Structure
```csharp
[TestClass]
public class EventNameTest
{
    [TestMethod]
    public void testValidEventCreation() { }
    
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void testInvalidEventThrows() { }
}
```

### Test Organization
- One test class per production class
- Test method naming: `test{Scenario}{ExpectedResult}`
- Use MSTest assertions and attributes
- Mock external dependencies (IEndpoint, IStorage)

## Quick Reference

### Event Types Checklist
- [ ] PageView: Web page tracking
- [ ] Structured: Category/action events  
- [ ] SelfDescribing: Custom JSON schemas
- [ ] EcommerceTransaction: Purchase events
- [ ] ScreenView: Mobile screen tracking
- [ ] Timing: Performance measurements

### Required Imports
```csharp
using Snowplow.Tracker;
using Snowplow.Tracker.Models.Events;
using Snowplow.Tracker.Models.Contexts;
using Snowplow.Tracker.Endpoints;
using Snowplow.Tracker.Emitters;
```

### Common Operations
```csharp
// Initialize tracker
Tracker.Instance.Start(endpoint, dbPath, method);

// Track page view
Tracker.Instance.Track(new PageView()
    .SetPageUrl("url").Build());

// Track structured event
Tracker.Instance.Track(new Structured()
    .SetCategory("cat")
    .SetAction("act").Build());

// Attach contexts
event.SetCustomContext(contextList);

// Flush events
Tracker.Instance.Flush();
```

## Contributing to CLAUDE.md

When adding or updating content in this document, please follow these guidelines:

### File Size Limit
- **CLAUDE.md must not exceed 40KB** (currently ~19KB)
- Check file size after updates: `wc -c CLAUDE.md`
- Remove outdated content if approaching the limit

### Code Examples
- Keep all code examples **4 lines or fewer**
- Focus on the essential pattern, not complete implementations
- Use `// ❌` and `// ✅` to clearly show wrong vs right approaches

### Content Organization
- Add new patterns to existing sections when possible
- Create new sections sparingly to maintain structure
- Update the architectural principles section for major changes
- Ensure examples follow current codebase conventions

### Quality Standards
- Test any new patterns in actual code before documenting
- Verify imports and syntax are correct for the codebase
- Keep language concise and actionable
- Focus on "what" and "how", minimize "why" explanations

### Multiple CLAUDE.md Files
- **Directory-specific CLAUDE.md files** can be created for specialized modules
- Follow the same structure and guidelines as this root CLAUDE.md
- Keep them focused on directory-specific patterns and conventions
- Maximum 20KB per directory-specific CLAUDE.md file

### Instructions for LLMs
When editing files in this repository, **always check for CLAUDE.md guidance**:

1. **Look for CLAUDE.md in the same directory** as the file being edited
2. **If not found, check parent directories** recursively up to project root
3. **Follow the patterns and conventions** described in the applicable CLAUDE.md
4. **Prioritize directory-specific guidance** over root-level guidance when conflicts exist