using Microsoft.Extensions.Logging;
using Moq.DB.Context;
using Moq.DB.Repository;

namespace Moq.Business.Service
{
    public class CandidateService : ICandidateService
    {
        private readonly ICandidateRepository _repository;
        private readonly ICacheService _cache;
        private readonly ILogger<CandidateService> _logger;

        public CandidateService(ICandidateRepository repository, ICacheService cache, ILogger<CandidateService> logger)
        {
            _repository = repository;
            _cache = cache;
            _logger = logger;
        }

        public async Task AddOrUpdateCandidateAsync(Candidate candidate)
        {
            try
            {
                // First, try to get the candidate from the cache
                var existingCandidate = await GetCandidateByEmailCacheAsync(candidate.Email);

                if (existingCandidate != null)
                {
                    // Update existing candidate properties
                    existingCandidate.FirstName = candidate.FirstName;
                    existingCandidate.LastName = candidate.LastName;
                    existingCandidate.PhoneNumber = candidate.PhoneNumber;
                    existingCandidate.CallTimeInterval = candidate.CallTimeInterval;
                    existingCandidate.LinkedInUrl = candidate.LinkedInUrl;
                    existingCandidate.GitHubUrl = candidate.GitHubUrl;
                    existingCandidate.Comment = candidate.Comment;

                    // Update the candidate in the repository (DB)
                    await _repository.UpdateCandidateAsync(existingCandidate);

                    // Ensure cache is updated after update
                    AddOrUpdateCache(existingCandidate);
                }
                else
                {
                    // Add new candidate to the repository (DB)
                    await _repository.AddCandidateAsync(candidate);

                    // Cache the new candidate
                    AddOrUpdateCache(candidate);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding or updating a candidate with email {Email}", candidate.Email);
                throw new ServiceException("An error occurred while adding or updating the candidate.", ex);
            }
        }

        public async Task<Candidate> GetCandidateByEmailCacheAsync(string email)
        {
            try
            {
                if (_cache.TryGetValue(email, out Candidate? cachedCandidate))
                {
                    _logger.LogInformation("Cache hit for candidate email: {Email}", email);
                    return cachedCandidate;
                }

                _logger.LogInformation("Cache miss for candidate email: {Email}", email);
                var candidate = await _repository.GetCandidateByEmailAsync(email);

                if (candidate != null)
                {
                    // Cache the candidate for 30 minutes if found in the database
                    AddOrUpdateCache(candidate);
                }

                return candidate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving a candidate with email {Email}", email);
                throw new ServiceException("An error occurred while retrieving the candidate.", ex);
            }
        }

        // Helper method to add or update candidate in the cache
        private void AddOrUpdateCache(Candidate candidate)
        {
            if (candidate == null)
            {
                throw new ArgumentNullException(nameof(candidate), "Cannot add or update cache with a null candidate.");
            }

            _cache.Set(candidate.Email, candidate, TimeSpan.FromMinutes(30));
            _logger.LogInformation("Candidate cached for email: {Email}", candidate.Email);
        }
    }
    public class ServiceException : Exception
    {
        public ServiceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
