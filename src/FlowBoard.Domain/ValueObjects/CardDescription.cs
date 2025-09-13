using FlowBoard.Domain.Primitives;

namespace FlowBoard.Domain.ValueObjects;

public sealed class CardDescription
{
    private CardDescription(string value) => Value = value;
    public string Value { get; }
    public override string ToString() => Value;

    public static Result<CardDescription> Create(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return new CardDescription(string.Empty); // treat empty as empty string (optional field)
        var trimmed = input.Trim();
        if (trimmed.Length > 5000)
            return Error.Validation("Card.Description.TooLong", "Description must be 5000 characters or fewer");
        return new CardDescription(trimmed);
    }
}
