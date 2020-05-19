using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestProject.Data;
using TestProject.Services;

namespace TestProject.Areas.MyArea.Services
{
    public class TypeParamService<T> : ITypeParamService<T> where T : new()
    {
        private readonly MyDbContext _context;
        private readonly IEmailService _emailService;

        public TypeParamService(
            MyDbContext context,
            IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public string Method1(T thingy)
        {
            return thingy.ToString();
        }

        public string Method1(T thingy, string stringy)
        {
            return thingy.ToString() + stringy;
        }

        public string Method1(T thingy, int blingy)
        {
            return thingy.ToString() + blingy.ToString();
        }

    }
}
