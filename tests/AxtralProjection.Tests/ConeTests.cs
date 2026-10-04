using AxtralProjection.Core;
using Xunit;
namespace AxtralProjection.Tests;
public class ConeTests
{
    [Fact] public void IncludesTreeAtThirtyMetersAhead() => Assert.True(Cone.Contains(0, 0, 30, 0, 1, 30, 30));
}
