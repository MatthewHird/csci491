using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SmartBreadcrumbs.Nodes;

namespace TestProject.Services.Breadcrumbs
{
    public interface IFishBreadcrumbService
    {
        public MvcBreadcrumbNode RedFishDetailsBreadcrumb(int? id);

        public MvcBreadcrumbNode BlueFishDetailsBreadcrumb(int? id);
    }
}
