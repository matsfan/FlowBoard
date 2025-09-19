using FastEndpoints;

namespace FlowBoard.WebApi.Endpoints.Boards;

public sealed class BoardsGroup : Group
{
    public BoardsGroup()
    {
        Configure("boards", ep =>
        {
            ep.Description(x => x.WithGroupName("Boards"));
        });
    }
}
