using FlowBoard.Domain.Primitives;

namespace FlowBoard.Domain.ValueObjects;

public sealed class CardTitle
{
    private CardTitle(string value) => Value = value;
    public string Value { get; }
    public override string ToString() => Value;

    public static Result<CardTitle> Create(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Error.Validation("Card.Title.Empty", "Title must be provided");
        var trimmed = input.Trim();
        if (trimmed.Length > 200)
            return Error.Validation("Card.Title.TooLong", "Title must be 200 characters or fewer");
        return new CardTitle(trimmed);
    }
}
