using EuchreServer.GraphQL;
using GraphQL;
using GraphQL.AspNetCore3;
using GraphQL.MicrosoftDI;
using GraphQL.SystemTextJson;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddGraphQL(b => b
    .AddAutoSchema<Query>()
    .AddSystemTextJson()
);
var app = builder.Build();

app.UseGraphQL();
app.MapGet("/", () => "Hello World!");

app.Run();
