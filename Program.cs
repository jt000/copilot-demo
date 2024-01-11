using System.Text;
using Azure;
using Azure.AI.OpenAI;
using copilot_demo;

var openAiUrl = Environment.GetEnvironmentVariable("AZURE_OPENAI_URL");
var openAiKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");

if (string.IsNullOrWhiteSpace(openAiUrl) || string.IsNullOrWhiteSpace(openAiKey))
{
    Console.WriteLine("ERROR: Missing Open AI Url and\\or Key. Open the file at 'Properties\\launchSettings.json' to set.");
    return;
}

OpenAIClient client = new OpenAIClient(new Uri(openAiUrl), new AzureKeyCredential(openAiKey));

var currentSvg = Encoding.UTF8.GetString(Prompts.StartingSvg);
File.WriteAllText("out.svg", currentSvg);

string userInput;

while (true)
{
    Console.Write("Enter Instruction ('Q' to quit, 'R' to reset): ");
    userInput = Console.ReadLine();

    if (userInput.ToLowerInvariant() == "q")
        return;

    if (userInput.ToLowerInvariant() == "r")
    {
        currentSvg = Encoding.UTF8.GetString(Prompts.StartingSvg);
        File.WriteAllText("out.svg", currentSvg);
        continue;
    }

    var prompt = string.Format(Prompts.SystemPrompt, currentSvg);
    var options = new ChatCompletionsOptions()
    {
        Temperature = (float)0.2,
        MaxTokens = 350,
        NucleusSamplingFactor = (float)0.95,
        FrequencyPenalty = 0,
        PresencePenalty = 0,
        Messages = {
            new ChatMessage(ChatRole.System, prompt),
            new ChatMessage(ChatRole.User, userInput)
        }
    };

    Response<ChatCompletions> responseWithoutStream = await client.GetChatCompletionsAsync("gpt35_logexp", options);
    ChatCompletions response = responseWithoutStream.Value;

    var newSvg = response.Choices[0].Message.Content.Trim();
    if (newSvg.StartsWith("<svg") && newSvg.EndsWith("/svg>"))
    {
        File.WriteAllText("out.svg", newSvg);
        currentSvg = newSvg;
    }
    else
    {
        Console.WriteLine($">> {newSvg}");
    }
}
