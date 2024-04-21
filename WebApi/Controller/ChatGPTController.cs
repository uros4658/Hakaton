using Application.Interfaces;
using Application.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class ChatGptController : ControllerBase
{
    private readonly IChatGptService _chatGptService;

    public ChatGptController(IChatGptService chatGptService)
    {
        _chatGptService = chatGptService;
    }

    [HttpPost("AddQuestionAndAnswerToDB")]
    public async Task<IActionResult> AddQandA(QandA qanda)
    {
        await _chatGptService.AddQandA(qanda);
        return Ok();
    }

    [HttpPost("SendAnswersAndGetPoints")]
    public async Task<IActionResult> SendMessageAsync([FromBody] Answer answer)
    {
        try
        {
            string result = await _chatGptService.SendMessageAsync(answer);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

}
