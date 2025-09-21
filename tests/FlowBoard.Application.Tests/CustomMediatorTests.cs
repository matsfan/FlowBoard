using Microsoft.Extensions.DependencyInjection;
using Xunit;
using FlowBoard.Application.Abstractions;
using FlowBoard.Application.Services;
using FlowBoard.Application.UseCases.Boards.Create;
using FlowBoard.Application.UseCases.Boards.List;
using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.Primitives;
using FlowBoard.Domain.Aggregates;
using FlowBoard.Domain.ValueObjects;
using NSubstitute;

namespace FlowBoard.Application.Tests;

public class CustomMediatorTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly IBoardRepository _mockRepository;
    private readonly IClock _mockClock;
    
    public CustomMediatorTests()
    {
        var services = new ServiceCollection();
        
        // Add our custom mediator and handlers
        services.AddApplicationServices();
        
        // Add mocked dependencies
        _mockRepository = Substitute.For<IBoardRepository>();
        _mockClock = Substitute.For<IClock>();
        var mockUserContext = Substitute.For<IUserContext>();
        
        services.AddSingleton(_mockRepository);
        services.AddSingleton(_mockClock);
        services.AddSingleton(mockUserContext);
        
        _serviceProvider = services.BuildServiceProvider();
        
        // Setup clock and user context
        _mockClock.UtcNow.Returns(DateTimeOffset.UnixEpoch);
        mockUserContext.CurrentUserId.Returns(new UserId(Guid.Parse("550e8400-e29b-41d4-a716-446655440000")));
    }
    
    [Fact]
    public async Task CustomMediator_Can_Create_Board()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var command = new CreateBoardCommand("Test Board");
        
        _mockRepository.ExistsByNameAsync("Test Board", Arg.Any<CancellationToken>()).Returns(false);
        
        // Act
        var result = await mediator.Send(command);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("Test Board", result.Value!.Name);
        await _mockRepository.Received(1).AddAsync(Arg.Any<Board>(), Arg.Any<CancellationToken>());
    }
    
    [Fact]
    public async Task CustomMediator_Can_Handle_Failed_Commands()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var command = new CreateBoardCommand("Duplicate Board");
        
        _mockRepository.ExistsByNameAsync("Duplicate Board", Arg.Any<CancellationToken>()).Returns(true);
        
        // Act
        var result = await mediator.Send(command);
        
        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Board.Name.Duplicate");
    }
    
    [Fact]
    public async Task CustomMediator_Can_List_Boards()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var query = new ListBoardsQuery();
        
        var testBoards = new List<Board>();
        _mockRepository.ListAsync(Arg.Any<CancellationToken>()).Returns(testBoards);
        
        // Act
        var result = await mediator.Send(query);
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value!);
        await _mockRepository.Received(1).ListAsync(Arg.Any<CancellationToken>());
    }
    
    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }
}