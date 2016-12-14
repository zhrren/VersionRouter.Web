using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mark.VersionRouter;
using System.IO;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using System.Text;
using Newtonsoft.Json;

namespace VersionRouter.Web.Services
{
    public class VersionSettingsService : IVersionSettingsService
    {
        private readonly IHostingEnvironment _env;
        private string _versionsPath;
        private Dictionary<string, List<Package>> _packages;

        public VersionSettingsService(IHostingEnvironment env)
        {
            _env = env;
        }

        public List<Package> GetPackages(string name)
        {
            if (_packages == null)
            {
                var settingsPath = Path.Combine(_env.ContentRootPath, "settings");
                if (!Directory.Exists(settingsPath))
                    Directory.CreateDirectory(settingsPath);

                _versionsPath = Path.Combine(settingsPath, "versions");
                if (!Directory.Exists(_versionsPath))
                    Directory.CreateDirectory(_versionsPath);

                IFileProvider fileProvider = new PhysicalFileProvider(_versionsPath);
                ChangeToken.OnChange(() => fileProvider.Watch("*.json"), () =>
                    Reload(fileProvider));

                _packages = new Dictionary<string, List<Package>>();
                Reload(fileProvider);
            }

            List<Package> result;
            if (!_packages.TryGetValue(name, out result))
                result = new List<Package>();
            return result;
        }

        private void Reload(IFileProvider fileProvider)
        {
            var files = Directory.GetFiles(_versionsPath);
            foreach (var file in files)
            {
                var filename = Path.GetFileName(file);
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);

                Stream stream = fileProvider.GetFileInfo(Path.GetFileName(file)).CreateReadStream();
                {
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);

                    var content = Encoding.UTF8.GetString(buffer);
                    var packages = JsonConvert.DeserializeObject<List<Package>>(content);
                    if (packages == null) packages = new List<Package>();
                    _packages.Add(fileNameWithoutExtension, packages);
                }
            }
        }
    }
}
