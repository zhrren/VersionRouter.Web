using Microsoft.AspNetCore.Mvc;
using VersionRouter.Web.Core;
using NLog;
using Mark.VersionRouter;
using System;
using Mark.ApiResult;

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

        public IActionResult Index(string name = "", string platform = "", string vesion = "1.0.0", string uid = "", string basever = "", bool redirect = false)
        {
            var packages = _versionSettingsService.GetPackages(name);
            var groups = _versionSettingsService.GetGroups();
            var router = new Router(packages, groups);
            var item = router.Match(platform, vesion, uid);

            var url = item == null ? "" : item.Url;

            if (!string.IsNullOrWhiteSpace(basever))
            {
                Version baseVersion;
                if (Version.TryParse(basever, out baseVersion))
                {
                    var fullVersion = new Version(
                         baseVersion.Major >= 0 ? baseVersion.Major : 0,
                         baseVersion.Minor >= 0 ? baseVersion.Minor : 0,
                         baseVersion.Build >= 0 ? baseVersion.Build : 0,
                         baseVersion.Revision >= 0 ? baseVersion.Revision : 0);

                    if (fullVersion < item.PackageVersion)
                        return Content(item.Url);
                }

                return Content("");
            }
            else if (redirect && !string.IsNullOrWhiteSpace(url))
                return Redirect(item.Url);
            else
                return Content(url);
        }

        public IActionResult Error()
        {
            return Content("Error");
        }
    }
}
