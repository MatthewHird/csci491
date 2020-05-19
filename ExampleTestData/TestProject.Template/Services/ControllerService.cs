using Microsoft.AspNetCore.Mvc;

namespace TestProject.Services
{
    public class ControllerService : IControllerService
    {
        public string GetRootName<T>() where T : Controller
        {
            string typeName = typeof(T).Name;
            return typeof(T).Name.EndsWith(nameof(Controller)) ? typeName.Substring(0, typeName.Length - nameof(Controller).Length) : typeName;
        }
    }
}
