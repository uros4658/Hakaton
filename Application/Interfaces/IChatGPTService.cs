using Application.Models;

namespace Application.Interfaces
{
    public interface IChatGptService
    {
        public Task AddQandA(QandA qanda);
        public Task<string> SendMessageAsync(Answer answer);
        public Task<QandA> GetQandAById(int id);
    }
}
