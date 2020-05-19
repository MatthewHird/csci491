using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartBreadcrumbs.Nodes;
using TestProject.Controllers.Fish;
using TestProject.Data;

namespace TestProject.Services.Breadcrumbs
{
    public class FishBreadcrumbService : IFishBreadcrumbService
    {
        private readonly MyDbContext _context;
        private readonly IControllerService _controllerService;

        public FishBreadcrumbService (
            MyDbContext context,
            IControllerService controllerService)
        {
            _context = context;
            _controllerService = controllerService;
        }

        public MvcBreadcrumbNode BlueFishDetailsBreadcrumb(int? id)
        {
            int? parentId = _context.BlueFish
                .Where(x => x.Id == id)
                .Select(x => x.ParentId)
                .FirstOrDefault();

            return new MvcBreadcrumbNode(nameof(BlueFishController.Details), _controllerService.GetRootName<BlueFishController>(), "<BlueFishController.Details>")
            {
                Parent = RedFishDetailsBreadcrumb(parentId),
                RouteValues = new { id }
            };
        }

        public void UnrelatedMethod(string words)
        {
            Console.Out.WriteLine(words);
        }

        public MvcBreadcrumbNode RedFishIndexBreadcrumb()
        {
            return new MvcBreadcrumbNode(nameof(RedFishController.Index), _controllerService.GetRootName<RedFishController>(), "<RedFishController.Index>")
            {
                Parent = null
            };
        }


        public MvcBreadcrumbNode RedFishDetailsBreadcrumb(int? id)
        {
            return new MvcBreadcrumbNode(nameof(RedFishController.Details), _controllerService.GetRootName<RedFishController>(), "<RedFishController.Details>")
            {
                Parent = null,
                RouteValues = new { id }
            };
        }

        public MvcBreadcrumbNode RedFishEditBreadcrumb(int? id)
        {
            return new MvcBreadcrumbNode(nameof(RedFishController.Edit), _controllerService.GetRootName<RedFishController>(), "<RedFishController.Edit>")
            {
                Parent = null,
                RouteValues = new { id }
            };
        }

        
    }
}
