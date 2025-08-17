var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.FlowBoard_Web>("flowboard-web");

builder.Build().Run();
