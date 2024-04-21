using Application.Models;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Text;
using System.Data;
using Application.Interfaces;
using Google.Protobuf;
using Microsoft.Extensions.Hosting.Internal;

public class ChatGptService : IChatGptService
{
    private readonly string _connectionString;
    private readonly HttpClient client;
    private readonly string apikey;

    public ChatGptService(string connectionString)
    {
        _connectionString = connectionString;
        client = new HttpClient();
        apikey = "Bearer 99vcvcqb4o4d";
    }

    public async Task AddQandA(QandA qanda)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            string query = "INSERT INTO QandA (Question, Answer, Option) VALUES (@Question, @Answer, @Option)";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Question", qanda.Question);
                command.Parameters.AddWithValue("@Answer", qanda.Answer);
                command.Parameters.AddWithValue("@Option", qanda.Option);

                await command.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task<string> SendMessageAsync(Answer answer)
    {
        // Fetch the QandA from the database using the QandAID from the answer
        QandA qanda = await GetQandAById(answer.QandAID);

        if (qanda == null)
        {
            throw new Exception("QandA not found in the database");
        }

        string modifiedPrompt = "In this prompt I will send you some questions, answers and their points your job is to rate it \n" + qanda.Question;

        if (qanda.Option == "text")
        {
            modifiedPrompt = "More harsh on the low end and The minimum rating can be zero";
        }
        modifiedPrompt += " Here are its right answers: " + qanda.Answer;

        // Add the answer string to the content
        modifiedPrompt += " Here are the answers from the student " + answer.Answers;

        modifiedPrompt += "Make sure your grading is correct check it mulitple times. " +
            "Reply like this Score: Score, then some questions the " +
            "student should work on based" +
            "on what he got wrong not the same thing just the same topic. That should be formed like this Questions: the list of" +
            " questions, then the material he can study " +
            "to improve his knowledge in a form like this Material: List of materials " ;

        var messages = new List<object>
        {
            new { role = "system", content = "" },
            new { role = "user", content = modifiedPrompt }
        };

        var requestData = new { model = "gpt-3.5-turbo", messages = messages };

        var content = new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json");
        client.DefaultRequestHeaders.Clear();

        // Add your API key for the proxy server
        client.DefaultRequestHeaders.Add("Authorization", apikey);

        // Use the proxy server's URL
        var response = await client.PostAsync("https://openai-proxy.sellestial.com/api/chat/completions", content);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(result);
            string res = data.choices[0].message.content.ToString();

            var jsonReturn = JSONReturn.FromString(res);
            var json = JsonConvert.SerializeObject(jsonReturn);

            return json;
        }

        else
        {
            throw new Exception($"Request to OpenAI API failed with status code {response.StatusCode}");
        }
    }


    public async Task<QandA> GetQandAById(int id)
    {
        using (var connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            string query = "SELECT * FROM QandA WHERE ID = @ID";

            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@ID", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new QandA
                        {
                            ID = reader.GetInt32("ID"),
                            Question = reader.GetString("Question"),
                            Answer = reader.GetString("Answer"),
                            Option = reader.GetString("Option")
                        };
                    }
                }
            }
        }

        return null;
    }

}
