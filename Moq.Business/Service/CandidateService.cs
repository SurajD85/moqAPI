using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq.DB.Context;
using Moq.DB.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moq.Business.Service
{
    public class CandidateService : ICandidateService
    {
        private readonly ICandidateRepository _repository;
        private readonly IMemoryCache _cache;
        private readonly ILogger<CandidateService> _logger;

        public CandidateService(ICandidateRepository repository, IMemoryCache cache, ILogger<CandidateService> logger)
        {
            _repository = repository;
            _cache = cache;
            _logger = logger;
        }

        public async Task AddOrUpdateCandidateAsync(Candidate candidate)
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
                UpdateCache(existingCandidate);
            }
            else
            {
                // Add new candidate to the repository (DB)
                await _repository.AddCandidateAsync(candidate);

                // Cache the new candidate
                AddOrUpdateCache(candidate);
            }
        }

        public async Task<Candidate> GetCandidateByEmailCacheAsync(string email)
        {
            if (_cache.TryGetValue(email, out Candidate cachedCandidate))
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

        // Helper method to update the cache after any change
        private void UpdateCache(Candidate candidate)
        {
            _cache.Set(candidate.Email, candidate, TimeSpan.FromMinutes(30));
            _logger.LogInformation("Cache updated for candidate email: {Email}", candidate.Email);
        }

        // Helper method to add or update candidate in the cache
        private void AddOrUpdateCache(Candidate candidate)
        {
            _cache.Set(candidate.Email, candidate, TimeSpan.FromMinutes(30));
            _logger.LogInformation("Candidate cached for email: {Email}", candidate.Email);
        }
    }
}
