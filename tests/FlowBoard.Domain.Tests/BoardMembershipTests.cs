using FlowBoard.Domain.Aggregates;
using FlowBoard.Domain.Abstractions;
using FlowBoard.Domain.ValueObjects;

namespace FlowBoard.Domain.Tests;

public class BoardMembershipTests
{
    private sealed class TestClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow { get; } = now;
    }

    private static readonly UserId TestOwner = new(Guid.Parse("550e8400-e29b-41d4-a716-446655440000"));
    private static readonly UserId TestMember = new(Guid.Parse("550e8400-e29b-41d4-a716-446655440001"));

    [Fact]
    public void Create_Board_Should_Have_Initial_Owner()
    {
        // Arrange
        var clock = new TestClock(DateTimeOffset.UtcNow);

        // Act
        var result = Board.Create("Test Board", TestOwner, clock);

        // Assert
        Assert.True(result.IsSuccess);
        var board = result.Value!;
        Assert.Single(board.Members);
        var member = board.Members.First();
        Assert.Equal(TestOwner, member.Id);
        Assert.Equal(BoardRole.Owner, member.Role);
        Assert.True(board.IsOwner(TestOwner));
    }

    [Fact]
    public void AddMember_Should_Add_New_Member()
    {
        // Arrange
        var clock = new TestClock(DateTimeOffset.UtcNow);
        var board = Board.Create("Test Board", TestOwner, clock).Value!;

        // Act
        var result = board.AddMember(TestMember, BoardRole.Member, clock);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, board.Members.Count);
        Assert.True(board.HasMember(TestMember));
        Assert.False(board.IsOwner(TestMember));
    }

    [Fact]
    public void AddMember_Should_Prevent_Duplicate_Members()
    {
        // Arrange
        var clock = new TestClock(DateTimeOffset.UtcNow);
        var board = Board.Create("Test Board", TestOwner, clock).Value!;
        board.AddMember(TestMember, BoardRole.Member, clock);

        // Act
        var result = board.AddMember(TestMember, BoardRole.Owner, clock);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Board.Member.AlreadyExists");
    }

    [Fact]
    public void RemoveMember_Should_Prevent_Removing_Last_Owner()
    {
        // Arrange
        var clock = new TestClock(DateTimeOffset.UtcNow);
        var board = Board.Create("Test Board", TestOwner, clock).Value!;

        // Act
        var result = board.RemoveMember(TestOwner);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Board.Member.LastOwner");
    }

    [Fact]
    public void ChangeRole_Should_Prevent_Removing_Last_Owner()
    {
        // Arrange
        var clock = new TestClock(DateTimeOffset.UtcNow);
        var board = Board.Create("Test Board", TestOwner, clock).Value!;

        // Act
        var result = board.ChangeRole(TestOwner, BoardRole.Member);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Board.Member.LastOwner");
    }

    [Fact]
    public void Rename_Should_Require_Owner_Permission()
    {
        // Arrange
        var clock = new TestClock(DateTimeOffset.UtcNow);
        var board = Board.Create("Test Board", TestOwner, clock).Value!;
        board.AddMember(TestMember, BoardRole.Member, clock);

        // Act
        var result = board.Rename("New Name", TestMember);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Board.Permission.OwnerRequired");
    }

    [Fact]
    public void AddColumn_Should_Require_Member_Permission()
    {
        // Arrange
        var clock = new TestClock(DateTimeOffset.UtcNow);
        var board = Board.Create("Test Board", TestOwner, clock).Value!;
        var nonMember = new UserId(Guid.NewGuid());

        // Act
        var result = board.AddColumn("Test Column", nonMember);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Contains(result.Errors, e => e.Code == "Board.Permission.MemberRequired");
    }
}