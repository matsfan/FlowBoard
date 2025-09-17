using FastEndpoints;

namespace FlowBoard.WebApi.Endpoints.Columns;

public sealed class ColumnsGroup : Group
{
	public ColumnsGroup()
	{
		Configure("/", ep =>
		{
			ep.Description(d => d.WithGroupName("Columns"));
		});
	}
}
