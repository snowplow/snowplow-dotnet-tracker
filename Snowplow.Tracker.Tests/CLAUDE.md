# Snowplow.Tracker.Tests Module - CLAUDE.md

## Module Overview

The test suite for the Snowplow .NET Tracker, using MSTest framework for unit and integration testing. Tests validate event construction, payload generation, emitter behavior, storage operations, and end-to-end tracking scenarios. Tests follow consistent naming conventions and comprehensive coverage patterns.

## Test Structure

```
Snowplow.Tracker.Tests/
├── IntegrationTest.cs      # End-to-end tracker testing
├── LoadTest.cs            # Performance and stress testing
├── UtilsTest.cs           # Utility function tests
├── Models/                # Model layer tests
│   ├── Events/           # Event class tests
│   └── Contexts/         # Context class tests
├── Emitters/             # Emitter behavior tests
├── Endpoints/            # Endpoint communication tests
├── Storage/              # Storage layer tests
├── Queues/               # Queue implementation tests
└── Logging/              # Logging tests
```

## Testing Patterns & Conventions

### Test Class Naming
```csharp
// ✅ Match production class with "Test" suffix
[TestClass]
public class PageViewTest { }  // Tests PageView class

// ❌ Don't use inconsistent naming
[TestClass]
public class TestPageView { }  // Prefix style not used
```

### Test Method Naming
```csharp
// ✅ Use test{Scenario}{ExpectedOutcome} pattern
[TestMethod]
public void testInitPageViewWithValidData() { }

[TestMethod]
public void testInitPageViewWithNullUrlThrowsException() { }

// ❌ Don't use unclear test names
[TestMethod]
public void test1() { }  // Not descriptive
```

### Exception Testing
```csharp
// ✅ Use ExpectedException attribute
[TestMethod]
[ExpectedException(typeof(ArgumentException), "PageUrl cannot be null")]
public void testNullPageUrlThrows()
{
    new PageView().SetPageUrl(null).Build();
}

// ❌ Don't use try-catch for expected exceptions
[TestMethod]
public void testException()
{
    try {
        new PageView().Build();
        Assert.Fail(); // Should have thrown
    } catch(ArgumentException) { }
}
```

## Event Testing Patterns

### Required Field Validation
```csharp
[TestMethod]
[ExpectedException(typeof(ArgumentException))]
public void testMissingRequiredFieldThrows()
{
    // Build without setting required field
    new EventType().Build();
}

[TestMethod]
public void testEmptyStringRequiredFieldThrows()
{
    Assert.ThrowsException<ArgumentException>(() =>
        new EventType().SetRequiredField("").Build()
    );
}
```

### Payload Verification
```csharp
[TestMethod]
public void testEventPayloadContainsCorrectData()
{
    var event = new Structured()
        .SetCategory("category")
        .SetAction("action")
        .Build();
    
    var payload = event.GetPayload();
    
    // Verify event type
    Assert.AreEqual(Constants.EVENT_STRUCTURED, 
                   payload.Payload[Constants.EVENT]);
    
    // Verify event data
    Assert.AreEqual("category", 
                   payload.Payload[Constants.SE_CATEGORY]);
    Assert.AreEqual("action", 
                   payload.Payload[Constants.SE_ACTION]);
    
    // Verify default fields
    Assert.IsTrue(payload.Payload.ContainsKey(Constants.EID));
    Assert.IsTrue(payload.Payload.ContainsKey(Constants.TIMESTAMP));
}
```

### Context Testing
```csharp
[TestMethod]
public void testContextAttachment()
{
    var contexts = new List<IContext> {
        new DesktopContext()
            .SetOsType("Windows")
            .Build()
    };
    
    var event = new PageView()
        .SetPageUrl("url")
        .SetCustomContext(contexts)
        .Build();
    
    Assert.AreEqual(1, event.GetContexts().Count);
    Assert.IsInstanceOfType(event.GetContexts()[0], 
                           typeof(DesktopContext));
}
```

## Emitter Testing

### AsyncEmitter Testing
```csharp
[TestMethod]
public void testAsyncEmitterProcessesEvents()
{
    var storage = new LiteDBStorage(":memory:");
    var queue = new PersistentBlockingQueue(storage);
    var endpoint = new MockEndpoint(); // Test double
    
    var emitter = new AsyncEmitter(endpoint, queue);
    emitter.Start();
    
    emitter.Add(payload);
    
    // Wait for processing
    Thread.Sleep(100);
    
    Assert.AreEqual(1, endpoint.ReceivedPayloads.Count);
    
    emitter.Stop();
    emitter.Dispose();
}
```

### Emitter Failure Handling
```csharp
[TestMethod]
public void testEmitterRetriesOnFailure()
{
    var endpoint = new FailingEndpoint(failCount: 2);
    var emitter = new AsyncEmitter(endpoint, queue);
    
    emitter.Add(payload);
    emitter.Start();
    
    // Should succeed on third attempt
    Thread.Sleep(500);
    
    Assert.AreEqual(3, endpoint.AttemptCount);
    Assert.AreEqual(1, endpoint.SuccessCount);
}
```

## Storage Testing

### Storage Operations
```csharp
[TestMethod]
public void testStoragePutAndGet()
{
    var storage = new LiteDBStorage(":memory:");
    var record = new EventRecord { Data = "test" };
    
    var id = storage.Put(record);
    var retrieved = storage.Get();
    
    Assert.AreEqual(record.Data, retrieved.Data);
    Assert.AreEqual(id, retrieved.Id);
}

[TestMethod]
public void testStorageDelete()
{
    var storage = new LiteDBStorage(":memory:");
    var id = storage.Put(new EventRecord());
    
    Assert.IsTrue(storage.Delete(id));
    Assert.IsNull(storage.Get());
}
```

## Integration Testing

### End-to-End Tracking
```csharp
[TestMethod]
public void testCompleteTrackingFlow()
{
    var collector = new MockCollector();
    var endpoint = new SnowplowHttpCollectorEndpoint(
        collector.Host, HttpProtocol.HTTP);
    
    Tracker.Instance.Start(endpoint, ":memory:", 
                          HttpMethod.POST);
    
    var pageView = new PageView()
        .SetPageUrl("https://example.com")
        .Build();
    
    Tracker.Instance.Track(pageView);
    Tracker.Instance.Flush();
    
    Assert.AreEqual(1, collector.ReceivedEvents.Count);
    var received = collector.ReceivedEvents[0];
    Assert.AreEqual(Constants.EVENT_PAGE_VIEW, 
                   received["e"]);
}
```

## Mock/Test Double Patterns

### Mock Endpoint
```csharp
public class MockEndpoint : IEndpoint
{
    public List<Payload> ReceivedPayloads = new List<Payload>();
    
    public HttpRequestMessage GetRequest(Payload payload)
    {
        ReceivedPayloads.Add(payload);
        return new HttpRequestMessage();
    }
    
    public bool Send(Payload payload)
    {
        ReceivedPayloads.Add(payload);
        return true;
    }
}
```

### Mock Storage
```csharp
public class InMemoryTestStorage : IStorage
{
    private Queue<EventRecord> records = new Queue<EventRecord>();
    
    public string Put(EventRecord record)
    {
        records.Enqueue(record);
        return Guid.NewGuid().ToString();
    }
    
    public EventRecord Get()
    {
        return records.Count > 0 ? records.Dequeue() : null;
    }
}
```

## Performance Testing

### Load Test Pattern
```csharp
[TestMethod]
public void testHighVolumeEventTracking()
{
    var eventCount = 10000;
    var events = new List<IEvent>();
    
    for (int i = 0; i < eventCount; i++)
    {
        events.Add(new Structured()
            .SetCategory("load")
            .SetAction($"test-{i}")
            .Build());
    }
    
    var stopwatch = Stopwatch.StartNew();
    
    foreach (var e in events)
    {
        Tracker.Instance.Track(e);
    }
    
    stopwatch.Stop();
    
    Assert.IsTrue(stopwatch.ElapsedMilliseconds < 5000,
                 $"Tracking {eventCount} events took too long");
}
```

## Common Test Pitfalls

### 1. Not Cleaning Up State
```csharp
// ❌ Leaving tracker running between tests
[TestMethod]
public void test1()
{
    Tracker.Instance.Start(...);
    // No Stop() call
}

// ✅ Use TestCleanup
[TestCleanup]
public void Cleanup()
{
    if (Tracker.Instance.Started)
    {
        Tracker.Instance.Stop();
    }
}
```

### 2. Timing-Dependent Tests
```csharp
// ❌ Fixed sleep times
Thread.Sleep(100);
Assert.AreEqual(expected, actual); // May fail on slow systems

// ✅ Use retry logic or wait conditions
var maxWait = TimeSpan.FromSeconds(5);
var stopwatch = Stopwatch.StartNew();
while (stopwatch.Elapsed < maxWait)
{
    if (condition) break;
    Thread.Sleep(50);
}
```

### 3. Testing Implementation Details
```csharp
// ❌ Testing private methods/fields
var field = typeof(Tracker).GetField("_running", 
    BindingFlags.NonPublic | BindingFlags.Instance);
Assert.IsTrue((bool)field.GetValue(tracker));

// ✅ Test public API behavior
Assert.IsTrue(Tracker.Instance.Started);
```

## Test Data Builders

### Event Test Data
```csharp
public static class TestEvents
{
    public static PageView ValidPageView()
    {
        return new PageView()
            .SetPageUrl("https://test.com")
            .SetPageTitle("Test Page")
            .Build();
    }
    
    public static Structured ValidStructured()
    {
        return new Structured()
            .SetCategory("test")
            .SetAction("click")
            .Build();
    }
}
```

### Context Test Data
```csharp
public static class TestContexts
{
    public static List<IContext> StandardContexts()
    {
        return new List<IContext>
        {
            new DesktopContext()
                .SetOsType("Windows")
                .Build(),
            new SessionContext("user-123", "session-456")
                .Build()
        };
    }
}
```

## Assertion Patterns

### Payload Assertions
```csharp
// ✅ Comprehensive payload checks
Assert.IsNotNull(payload);
Assert.IsTrue(payload.Payload.ContainsKey(Constants.EVENT));
Assert.AreEqual(expectedType, payload.Payload[Constants.EVENT]);
Assert.IsTrue(Guid.TryParse(
    payload.Payload[Constants.EID].ToString(), out _));

// ❌ Weak assertions
Assert.IsNotNull(payload); // Only null check
```

### Collection Assertions
```csharp
// ✅ Check collection state properly
Assert.AreEqual(expectedCount, collection.Count);
Assert.IsTrue(collection.All(item => item.IsValid));
CollectionAssert.Contains(collection, expectedItem);

// ❌ Only checking count
Assert.AreEqual(3, items.Count); // What about content?
```

## Quick Reference - Test Patterns

### Essential Test Attributes
```csharp
[TestClass]     // Mark test class
[TestMethod]    // Mark test method
[TestInitialize] // Run before each test
[TestCleanup]   // Run after each test
[ExpectedException(typeof(Exception))] // Expected exception
[Timeout(5000)] // Test timeout in ms
```

### Common Assertions
```csharp
Assert.AreEqual(expected, actual);
Assert.IsTrue(condition);
Assert.IsNull(obj);
Assert.IsInstanceOfType(obj, typeof(Type));
Assert.ThrowsException<T>(() => code);
CollectionAssert.AreEqual(expected, actual);
```

### Test Organization Checklist
- [ ] One test class per production class
- [ ] Test methods follow naming convention
- [ ] Expected exceptions use attributes
- [ ] Test cleanup in TestCleanup method
- [ ] Mock objects implement interfaces
- [ ] No hardcoded delays in tests
- [ ] Test data builders for complex objects