using FastEndpoints;

namespace FlowBoard.WebApi.Endpoints.Cards;

public sealed class CardsGroup : Group
{
    public CardsGroup()
    {
        Configure("cards", ep =>
        {
            ep.Description(d => d.WithGroupName("Cards"));
        });
    }
}
