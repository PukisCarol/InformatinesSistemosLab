namespace InformacinesSistemos.Services
{
    public interface IAIService
    {
        Task<string> GetAnswerAsync(string question);
    }
}
