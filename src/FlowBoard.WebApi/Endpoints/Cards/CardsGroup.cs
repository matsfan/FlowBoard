using FastEndpoints;

namespace FlowBoard.WebApi.Endpoints.Cards;

public sealed class CardsGroup : Group
{
    public CardsGroup()
    {
        Configure("/", ep =>
        {
            ep.Description(d => d.WithGroupName("Cards"));
        });
    }
}
