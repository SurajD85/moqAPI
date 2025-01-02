using Moq.DB.Context;

namespace Moq.DB.Repository
{
    public interface ICandidateRepository
    {
        Task<Candidate> GetCandidateByEmailAsync(string email);
        Task AddCandidateAsync(Candidate candidate);
        Task<Candidate> UpdateCandidateAsync(Candidate candidate);
    }
}
