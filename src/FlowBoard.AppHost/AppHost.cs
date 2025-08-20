var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.FlowBoard_WebApi>("flowboard-webapi");

builder.Build().Run();
