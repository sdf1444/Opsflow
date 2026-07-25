using Xunit;

namespace OpsFlow.Tests.Fixtures;

[CollectionDefinition("SharedTestFactory")]
public class SharedTestCollection : ICollectionFixture<OpsFlow.Tests.RequestAuditEndpointTests.TestFactory>
{
}
