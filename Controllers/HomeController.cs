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

        public IActionResult Index(string name = "", string platform = "", string vesion = "1.0.0", string uid = "", bool redirect = false)
        {
            var packages = _versionSettingsService.GetPackages(name);
            var groups = _versionSettingsService.GetGroups();
            var router = new Router(packages, groups);
            var item = router.Match(platform, vesion, uid);

            var url = item == null ? "" : item.Url;
            if (redirect && !string.IsNullOrWhiteSpace(url))
                return Redirect(item.Url);
            else
                return Content(url);
        }

        public IActionResult Check(string name = "", string platform = "", string vesion = "1.0.0", string ver = "1.0.0", string uid = "")
        {
            var packages = _versionSettingsService.GetPackages(name);
            var groups = _versionSettingsService.GetGroups();
            var router = new Router(packages, groups);
            var item = router.Match(platform, vesion, uid);

            if (item != null)
            {
                Version runVersion;
                if (Version.TryParse(ver, out runVersion))
                {
                    var fullVersion = new Version(
                         runVersion.Major >= 0 ? runVersion.Major : 0,
                         runVersion.Minor >= 0 ? runVersion.Minor : 0,
                         runVersion.Build >= 0 ? runVersion.Build : 0,
                         runVersion.Revision >= 0 ? runVersion.Revision : 0);

                    if (fullVersion < item.PackageVersion)
                        return Content(item.Url);
                }
            }
            return Content("");
        }

        public IActionResult Error()
        {
            return Content("Error");
        }
    }
}
