using FastEndpoints;

namespace FlowBoard.Web.Endpoints.Boards;

public sealed class BoardsGroup : Group
{
    public BoardsGroup()
    {
        Configure("/", ep =>
        {
            ep.Description(x => x.WithGroupName("Boards"));
        });
    }
}
