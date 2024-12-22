using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq.DB.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moq.DB.Repository
{
    public class CandidateRepository : ICandidateRepository
    {
        private readonly AppDbContext _context;

        public CandidateRepository(AppDbContext context, ILogger<CandidateRepository> logger)
        {
            _context = context;
        }

        public async Task AddCandidateAsync(Candidate candidate)
        {
            try
            {
                _context.Candidates.Add(candidate);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Candidate> GetCandidateByEmailAsync(string email)
        {
            try
            {
                return await _context.Candidates.FindAsync(email);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Candidate> UpdateCandidateAsync(Candidate candidate)
        {
            try
            {
                _context.Entry(candidate).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return candidate;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }

}
