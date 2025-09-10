using Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

// Add policy 'Admin'. Give to Liam and I for full access to all endpoints
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
    {
        policy.RequireClaim("scope", "Admin");
    });
});

builder.Services.AddAuthentication().AddJwtBearer();

// Load Environment variables
var openAiKey = builder.Configuration["OpenAI:ApiKey"];
if (openAiKey == null)
{
    throw new Exception("Could not load 'Open AI' API key.");
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapOpenAiEndpoints(openAiKey);

app.Run();
