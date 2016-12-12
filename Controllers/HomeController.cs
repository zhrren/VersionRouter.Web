using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VersionRouter.Web.Base;
using VersionRouter.Web.Services;
using NLog;

namespace WebApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger _log = LogManager.GetCurrentClassLogger();
        private readonly IVersionSettingsService _formatOptions;

        public HomeController(IVersionSettingsService formatOptions)
        {
            _formatOptions = formatOptions;
        }

        public IActionResult Index()
        {
            _log.Info("Index");

            var list = _formatOptions.GetPackages("dd");
            return Content("ContentResult"+ _formatOptions.GetHashCode());
        }

        public IActionResult Error()
        {
            return Content("Error");
        }
    }
}
