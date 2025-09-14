namespace Endpoints;

using System.Text.Json;
using System.Text.Json.Serialization;

public static class OpenAiEndpoints
{
    // Incoming request body
    public record TailorResumeRequestBody(string FileData, string JobDescription, bool UseGPT5 = false);

    // Outgoing request body to Open AI
    public record OpenAiRequest(string Model, Prompt Prompt, List<InputItem> Input);
    public record Prompt(string Id);
    public record InputItem(string Role, List<ContentItem> Content);

    public record ContentItem
    {
        [JsonPropertyName("type")]
        public string Type { get; init; } = default!;

        [JsonPropertyName("filename")]
        public string? Filename { get; init; }

        [JsonPropertyName("file_data")]
        public string? FileData { get; init; }

        [JsonPropertyName("text")]
        public string? Text { get; init; }
    }


    public static IEndpointRouteBuilder MapOpenAiEndpoints(this IEndpointRouteBuilder app, string apiKey)
    {
        var group = app.MapGroup("/api").RequireAuthorization("AdminPolicy").WithOpenApi();

        group.MapPost("/tailor-resume", async (TailorResumeRequestBody requestBody, HttpClient client) =>
        {
            const int ENDPOINT_TIMEOUT = 300; // 5 minutes
            const string PROMPT_ID = "pmpt_68c08c329b548190a785bcc83306e45e0d83ea34646f2cf2";
            string gptVersion = requestBody.UseGPT5 ? "gpt-5" : "gpt-5-nano";

            client.Timeout = TimeSpan.FromSeconds(ENDPOINT_TIMEOUT);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/responses");

            // Set auth header using Open AI API key
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            // Setup JSON body for request to open ai
            Prompt prompt = new Prompt(PROMPT_ID);
            List<ContentItem> contents = new List<ContentItem>();

            ContentItem pdfContent = new ContentItem { Type = "input_file", Filename = "resume.pdf", FileData = requestBody.FileData };
            ContentItem jobDescContent = new ContentItem { Type = "input_text", Text = requestBody.JobDescription };

            contents.Add(pdfContent);
            contents.Add(jobDescContent);

            List<InputItem> inputItems = new List<InputItem>();
            InputItem inputItem = new InputItem("user", contents);
            inputItems.Add(inputItem);

            OpenAiRequest openAiRequestBody = new OpenAiRequest(gptVersion, prompt, inputItems);
            JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;

            request.Content = JsonContent.Create(openAiRequestBody, options: options);

            var response = await client.SendAsync(request); // Send off request
            var content = await response.Content.ReadAsStringAsync();

            return Results.Content(content, "application/json");
        }).RequireAuthorization("AdminPolicy");

        return app;
    }
}

