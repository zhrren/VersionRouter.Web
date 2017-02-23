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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">客户端标识</param>
        /// <param name="platform">客户端平台名称</param>
        /// <param name="vesion">客户端版本</param>
        /// <param name="uid">用户标识</param>
        /// <param name="basever">软件包版本</param>
        /// <param name="redirect">是否自动跳转</param>
        /// <returns>
        /// 1、空字符
        /// 2、跳转到新Url，当basever="" && redirect=true
        /// 3、Url
        /// 4、{url: "", payload: ""}，当payload=true
        /// </returns>
        public IActionResult Index(string name = "", string platform = "", string vesion = "1.0.0",
            string uid = "", string basever = "", bool redirect = false, bool payload = false)
        {
            var packages = _versionSettingsService.GetPackages(name);
            var groups = _versionSettingsService.GetGroups();
            var router = new Router(packages, groups);
            var item = router.Match(platform, vesion, uid);

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
                        return CreateContent(item, redirect, payload);
                }

                return CreateContent(null, redirect, payload);
            }
            else
                return CreateContent(item, redirect, payload);
        }

        private IActionResult CreateContent(Entry item, bool redirect, bool payload)
        {
            if (item == null)
                return Content("");

            if (redirect)
            {
                return Redirect(item.Url);
            }
            else
            {

                if (payload)
                {
                    var result = new
                    {
                        url = item.Url,
                        payload = item.Payload
                    };
                    return Json(result);
                }
                else
                {
                    return Content(item.Url);
                }
            }
        }

        public IActionResult Error()
        {
            return Content("Error");
        }
    }
}
