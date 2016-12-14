using Mark.VersionRouter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VersionRouter.Web.Services
{
    public interface IVersionSettingsService
    {
        List<Package> GetPackages(string name);
        List<Group> GetGroups();
    }
}
