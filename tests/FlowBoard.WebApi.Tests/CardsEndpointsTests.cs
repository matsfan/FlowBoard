using System.Net;
using System.Text;
using System.Text.Json;
using FlowBoard.Domain.ValueObjects;
using FlowBoard.Domain.Aggregates;
using FlowBoard.Domain.Abstractions;
using FlowBoard.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace FlowBoard.WebApi.Tests;

public class CardsEndpointsTests : IClassFixture<WebApiTestFactory>
{
    private readonly WebApiTestFactory _factory;
    private readonly HttpClient _client;

    public CardsEndpointsTests(WebApiTestFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private WebApiTestFactory Factory => _factory;

    private async Task<(Guid boardId, Guid columnId)> CreateTestBoardWithColumn()
    {
        using var scope = Factory.Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBoardRepository>();
        var clock = scope.ServiceProvider.GetRequiredService<IClock>();
        var userContext = scope.ServiceProvider.GetRequiredService<IUserContext>();
        
        // Use the same user ID that will be used by the API endpoints
        var userId = userContext.CurrentUserId;
        var boardResult = Board.Create("Test Board", userId, clock);
        Assert.True(boardResult.IsSuccess);
        var board = boardResult.Value!;
        
        var columnResult = board.AddColumn("Test Column", userId, null);
        Assert.True(columnResult.IsSuccess);
        
        await repository.AddAsync(board, CancellationToken.None);
        
        var column = board.Columns.First();
        return (board.Id.Value, column.Id.Value);
    }

    [Fact]
    public async Task CreateCard_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var (boardId, columnId) = await CreateTestBoardWithColumn();
        var request = new
        {
            Title = "Test Card",
            Description = "Test Description"
        };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/boards/{boardId}/columns/{columnId}/cards", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var responseContent = await response.Content.ReadAsStringAsync();
        var card = JsonSerializer.Deserialize<JsonElement>(responseContent);
        Assert.Equal("Test Card", card.GetProperty("title").GetString());
        Assert.Equal("Test Description", card.GetProperty("description").GetString());
    }

    [Fact]
    public async Task CreateCard_InvalidBoardId_ReturnsNotFound()
    {
        // Arrange
        var request = new
        {
            Title = "Test Card",
            Description = "Test Description"
        };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/boards/{Guid.NewGuid()}/columns/{Guid.NewGuid()}/cards", content);

        // Assert
    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCards_ValidColumn_ReturnsCards()
    {
        // Arrange
        var (boardId, columnId) = await CreateTestBoardWithColumn();
        
        // Create a card first
        var createRequest = new
        {
            Title = "Test Card",
            Description = "Test Description"
        };
        var createContent = new StringContent(JsonSerializer.Serialize(createRequest), Encoding.UTF8, "application/json");
        await _client.PostAsync($"/boards/{boardId}/columns/{columnId}/cards", createContent);

        // Act
        var response = await _client.GetAsync($"/boards/{boardId}/columns/{columnId}/cards");

        // Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var content = await response.Content.ReadAsStringAsync();
    var result = JsonSerializer.Deserialize<JsonElement>(content);
    Assert.True(result.TryGetProperty("cards", out var cardsArray));
    Assert.True(cardsArray.ValueKind == JsonValueKind.Array);
    Assert.True(cardsArray.GetArrayLength() >= 1);
    }

    [Fact]
    public async Task GetCard_ValidId_ReturnsCard()
    {
        // Arrange
        var (boardId, columnId) = await CreateTestBoardWithColumn();
        
        // Create a card first
        var createRequest = new
        {
            Title = "Test Card",
            Description = "Test Description"
        };
        var createContent = new StringContent(JsonSerializer.Serialize(createRequest), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync($"/boards/{boardId}/columns/{columnId}/cards", createContent);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdCard = JsonSerializer.Deserialize<JsonElement>(createResponseContent);
        var cardId = createdCard.GetProperty("id").GetString();

        // Act
        var response = await _client.GetAsync($"/boards/{boardId}/columns/{columnId}/cards/{cardId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var card = JsonSerializer.Deserialize<JsonElement>(content);
        Assert.Equal("Test Card", card.GetProperty("title").GetString());
    }

    [Fact]
    public async Task UpdateCard_ValidRequest_ReturnsUpdated()
    {
        // Arrange
        var (boardId, columnId) = await CreateTestBoardWithColumn();
        
        // Create a card first
        var createRequest = new
        {
            Title = "Test Card",
            Description = "Test Description"
        };
        var createContent = new StringContent(JsonSerializer.Serialize(createRequest), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync($"/boards/{boardId}/columns/{columnId}/cards", createContent);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdCard = JsonSerializer.Deserialize<JsonElement>(createResponseContent);
        var cardId = createdCard.GetProperty("id").GetString();

        // Update request
        var updateRequest = new
        {
            Title = "Updated Card",
            Description = "Updated Description",
            IsArchived = false
        };
        var updateContent = new StringContent(JsonSerializer.Serialize(updateRequest), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/boards/{boardId}/columns/{columnId}/cards/{cardId}", updateContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var card = JsonSerializer.Deserialize<JsonElement>(content);
        Assert.Equal("Updated Card", card.GetProperty("title").GetString());
        Assert.Equal("Updated Description", card.GetProperty("description").GetString());
    }

    [Fact]
    public async Task UpdateCard_ArchiveCard_ReturnsArchivedCard()
    {
        // Arrange
        var (boardId, columnId) = await CreateTestBoardWithColumn();
        
        // Create a card first
        var createRequest = new
        {
            Title = "Test Card",
            Description = "Test Description"
        };
        var createContent = new StringContent(JsonSerializer.Serialize(createRequest), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync($"/boards/{boardId}/columns/{columnId}/cards", createContent);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdCard = JsonSerializer.Deserialize<JsonElement>(createResponseContent);
        var cardId = createdCard.GetProperty("id").GetString();

        // Archive request
        var archiveRequest = new
        {
            Title = "Test Card",
            Description = "Test Description",
            IsArchived = true
        };
        var archiveContent = new StringContent(JsonSerializer.Serialize(archiveRequest), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PutAsync($"/boards/{boardId}/columns/{columnId}/cards/{cardId}", archiveContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        var card = JsonSerializer.Deserialize<JsonElement>(content);
        Assert.True(card.GetProperty("isArchived").GetBoolean());
    }

    [Fact]
    public async Task DeleteCard_ValidId_ReturnsNoContent()
    {
        // Arrange
        var (boardId, columnId) = await CreateTestBoardWithColumn();
        
        // Create a card first
        var createRequest = new
        {
            Title = "Test Card",
            Description = "Test Description"
        };
        var createContent = new StringContent(JsonSerializer.Serialize(createRequest), Encoding.UTF8, "application/json");
        var createResponse = await _client.PostAsync($"/boards/{boardId}/columns/{columnId}/cards", createContent);
        var createResponseContent = await createResponse.Content.ReadAsStringAsync();
        var createdCard = JsonSerializer.Deserialize<JsonElement>(createResponseContent);
        var cardId = createdCard.GetProperty("id").GetString();

        // Act
        var response = await _client.DeleteAsync($"/boards/{boardId}/columns/{columnId}/cards/{cardId}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteCard_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var (boardId, columnId) = await CreateTestBoardWithColumn();

        // Act
        var response = await _client.DeleteAsync($"/boards/{boardId}/columns/{columnId}/cards/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateCard_EmptyTitle_ReturnsBadRequest()
    {
        // Arrange
        var (boardId, columnId) = await CreateTestBoardWithColumn();
        var request = new
        {
            Title = "",
            Description = "Test Description"
        };
        var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync($"/boards/{boardId}/columns/{columnId}/cards", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}