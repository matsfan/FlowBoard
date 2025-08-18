using NetArchTest.Rules;
using System.Reflection;
using Xunit;
using FlowBoard.Infrastructure.Boards; // marker
using FlowBoard.Web; // marker

namespace FlowBoard.Architecture.Tests;

public class ArchitectureRules
{
    private static readonly Assembly Domain = typeof(FlowBoard.Domain.Board).Assembly;
    private static readonly Assembly Application = typeof(FlowBoard.Application.DependencyInjection).Assembly;
    private static readonly Assembly Infrastructure = typeof(InfrastructureAssemblyMarker).Assembly;
    private static readonly Assembly Web = typeof(WebAssemblyMarker).Assembly; // Marker type

    [Fact]
    public void Domain_Should_Not_Depend_On_Outer_Layers()
    {
        var result = Types.InAssembly(Domain)
            .ShouldNot()
            .HaveDependencyOnAny(
                Application.GetName().Name!,
                Infrastructure.GetName().Name!,
                Web.GetName().Name!
            )
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Application_Should_Only_Depend_On_Domain()
    {
        var result = Types.InAssembly(Application)
            .ShouldNot()
            .HaveDependencyOnAny(
                Infrastructure.GetName().Name!,
                Web.GetName().Name!
            )
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Infrastructure_Should_Not_Depend_On_Web()
    {
        var result = Types.InAssembly(Infrastructure)
            .ShouldNot()
            .HaveDependencyOnAny(Web.GetName().Name!)
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    [Fact]
    public void Web_Should_Not_Depend_On_Infrastructure_Directly_For_Domain_Types()
    {
        // Example rule placeholder: ensure Web layer does not reference infrastructure implementation namespace
        var result = Types.InAssembly(Web)
            .That().ResideInNamespace("FlowBoard.Web")
            .ShouldNot().HaveDependencyOn("FlowBoard.Infrastructure.Boards")
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    private static string Describe(TestResult result)
    {
        if (result.IsSuccessful) return "Success";
        return "Failed types: " + string.Join(", ", result.FailingTypeNames);
    }
}
