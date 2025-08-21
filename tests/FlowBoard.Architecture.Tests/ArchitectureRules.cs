using NetArchTest.Rules;
using System.Reflection;
using Xunit;
using FlowBoard.Infrastructure.Services; // marker
using FlowBoard.WebApi; // marker

namespace FlowBoard.Architecture.Tests;

public class ArchitectureRules
{
    private static readonly Assembly Domain = typeof(FlowBoard.Domain.Aggregates.Board).Assembly;
    private static readonly Assembly Application = typeof(FlowBoard.Application.Services.ServiceRegistration).Assembly;
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
            .That().ResideInNamespace("FlowBoard.WebApi")
            .ShouldNot().HaveDependencyOn("FlowBoard.Infrastructure.Boards") // legacy namespace kept in rule for safety
            .GetResult();

        Assert.True(result.IsSuccessful, Describe(result));
    }

    private static string Describe(TestResult result)
    {
        if (result.IsSuccessful) return "Success";
        return "Failed types: " + string.Join(", ", result.FailingTypeNames);
    }
}
