namespace Endpoints;

public static class OpenAiEndpoints
{
    public static IEndpointRouteBuilder MapOpenAiEndpoints(this IEndpointRouteBuilder app, string apiKey)
    {
        var group = app.MapGroup("/api").RequireAuthorization("AdminPolicy").WithOpenApi();

        group.MapPost("/tailor-resume", async (HttpContext context, HttpClient client) =>
        {
            // Setup request to Chat GPT
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");

            // Handle auth header
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            // Add JSON body
            // TODO: Make it hit the correct prompt
            // TODO: Ensure payload includes input file & job description. Send in payload
            var body = new { model = "gpt-5", input = "say hi" };
            request.Content = JsonContent.Create(body);

            // Send off request
            var response = await client.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            return Results.Content(content, "application/json");
        }).RequireAuthorization("AdminPolicy");


        return app;
    }
}

