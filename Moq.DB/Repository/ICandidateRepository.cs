using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq.DB.Context;
using Microsoft.EntityFrameworkCore;

namespace Moq.DB.Repository
{
    public interface ICandidateRepository
    {
        Task<Candidate> GetCandidateByEmailAsync(string email);  // This will now work with Candidate class
        Task AddCandidateAsync(Candidate candidate);
        Task<Candidate> UpdateCandidateAsync(Candidate candidate);
    }
}
