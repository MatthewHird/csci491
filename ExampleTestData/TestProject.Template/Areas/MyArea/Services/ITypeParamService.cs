using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestProject.Areas.MyArea.Services
{
    interface ITypeParamService<T> where T: new()
    {
        string Method1(T thingy);

        string Method1(T thingy, string stringy);
    }
}
