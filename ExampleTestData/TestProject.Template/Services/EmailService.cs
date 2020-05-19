using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestProject.Data;

namespace TestProject.Services
{
    public class EmailService : IEmailService
    {
        private readonly MyDbContext _context;

        public EmailService(MyDbContext context)
        {
            _context = context;
        }
    }
}
