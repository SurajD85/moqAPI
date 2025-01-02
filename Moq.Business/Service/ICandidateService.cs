using Moq.DB.Context;

namespace Moq.Business.Service
{
    public interface ICandidateService
    {
        Task AddOrUpdateCandidateAsync(Candidate candidate);
    }
}
