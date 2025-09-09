using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication().AddJwtBearer();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Example endpoint that requires valid JWT token
app.MapGet("/permissions", (ClaimsPrincipal user) =>
{
    var permissions = user.FindAll("permissions").Select(c => c.Value).ToHashSet();
    return string.Join(",", permissions);
}).RequireAuthorization();

app.Run();
