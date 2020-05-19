using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestProject.Services;

namespace TestProject.Areas.MyArea.Services
{
    public class MismatchService : IMismatchService
    {
        private readonly IEmailService _emailService;

        public MismatchService(
            IEmailService emailService)
        {
            _emailService = emailService;
        }

        public string Method1()
        {
            return "Hello";
        }

        public int Method2(int myInt)
        {
            return 0;
        }

        public int Method2(int myInt, string myString)
        {
            return 0;
        }

        public void Method3()
        {
            return;
        }

        private void PrivateMethod()
        {
            return;
        }
    }
}
