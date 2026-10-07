using System.Linq;
using AxtralProjection.Core;
using Xunit;
namespace AxtralProjection.Tests;
public class SelectionTests
{
    [Fact] public void ListsTrimCloneSuffixAndDeduplicate() => Assert.Equal(new[]{"Oak", "Pine"}, TargetSelection.Parse(" Oak(Clone), ,Pine,Oak ").OrderBy(x=>x));
    [Fact] public void ExclusionWinsOverInclusion() => Assert.False(TargetSelection.Allowed("Oak(Clone)", true, false, false, TargetSelection.Parse("Oak"), TargetSelection.Parse("Oak")));
    [Fact] public void StumpCannotBeIncludedWhenDisabled() => Assert.False(TargetSelection.Allowed("OakStub", false, true, false, TargetSelection.Parse("OakStub"), TargetSelection.Parse("")));
    [Fact] public void AdditionalPrefabIsAllowed() => Assert.True(TargetSelection.Allowed("Custom(Clone)", false, false, false, TargetSelection.Parse("Custom"), TargetSelection.Parse("")));
    [Fact] public void StumpsCanBeEnabled() => Assert.True(TargetSelection.Allowed("OakStub", false, true, true, TargetSelection.Parse(""), TargetSelection.Parse("")));
    [Fact] public void SelectionSortsAndCapsByForwardDistance() => Assert.Equal(new[]{1, 2}, TargetSelection.Nearest(new[]{5, 2, 1, 3}, x=>x, 2));
}
