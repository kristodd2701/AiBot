using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

class Program
{
    private static readonly HttpClient client = new HttpClient();
    private static readonly string apiKey = "************************************************************";
    private const string apiUrl = "https://api.groq.com/v1/chat/completions";
    private static List<dynamic> messageHistory = new List<dynamic>();
    private static string selectedModel = "llama3-8b-8192";
    private static string systemPrompt = "You are a helpful AI assistant.";
    private static readonly string historyFile = "chat_history.txt";

    static async Task<string> GetAIResponse(string userInput)
    {
        messageHistory.Add(new { role = "user", content = userInput });

        var requestData = new
        {
            model = selectedModel,
            messages = messageHistory.Prepend(new { role = "system", content = systemPrompt })
        };

        var jsonRequest = JsonConvert.SerializeObject(requestData);
        var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        try
        {
            var response = await client.PostAsync(apiUrl, content);
            if (!response.IsSuccessStatusCode)
            {
                return $"Error: {response.StatusCode} - {response.ReasonPhrase}";
            }

            var responseString = await response.Content.ReadAsStringAsync();
            dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);
            string aiResponse = jsonResponse.choices[0].message.content.ToString();

            messageHistory.Add(new { role = "assistant", content = aiResponse });
            SaveToHistory($"You: {userInput}\nAI: {aiResponse}\n");
            return aiResponse;
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    static void SaveToHistory(string conversation)
    {
        File.AppendAllText(historyFile, conversation);
    }

    static async Task Main()
    {
        Console.WriteLine("Welcome to your AI chatbot! Type 'exit' to quit.");

        Console.Write("Choose AI Model (default: llama3-8b-8192): ");
        string modelInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(modelInput)) selectedModel = modelInput;

        Console.Write("Set Chatbot Personality (default: Helpful AI): ");
        string promptInput = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(promptInput)) systemPrompt = promptInput;

        while (true)
        {
            Console.Write("\nYou: ");
            string userInput = Console.ReadLine();
            if (userInput.ToLower() == "exit" || userInput.ToLower() == "quit")
            {
                Console.WriteLine("\nGoodbye! 👋");
                break;
            }
            string response = await GetAIResponse(userInput);
            Console.WriteLine("AI: " + response);
        }
    }
}

