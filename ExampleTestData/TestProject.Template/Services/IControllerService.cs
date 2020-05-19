using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TestProject.Services
{
    public interface IControllerService
    {
        public string GetRootName<T>() where T : Controller;
    }
}
