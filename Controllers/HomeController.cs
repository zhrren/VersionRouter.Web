using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VersionRouter.Web.Base;
using VersionRouter.Web.Services;
using NLog;
using Mark.VersionRouter;

namespace VersionRouter.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();
        private readonly IVersionSettingsService _versionSettingsService;

        public HomeController(IVersionSettingsService versionSettingsService)
        {
            _versionSettingsService = versionSettingsService;
        }

        public IActionResult Index(string name, string platform="", string vesion="", string uid="")
        {
            var packages = _versionSettingsService.GetPackages(name);
            var groups = _versionSettingsService.GetGroups();
            var router = new Router(packages, groups);
            var item = router.Match(platform, vesion, uid);
            return Redirect(item.Url);
        }

        public IActionResult Error()
        {
            return Content("Error");
        }
    }
}
