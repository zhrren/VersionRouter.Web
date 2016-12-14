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
        private string _groupPath;
        private Dictionary<string, List<Package>> _packages;
        private List<Group> _groups;

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
                    ReloadPackage(fileProvider));

                _packages = new Dictionary<string, List<Package>>();
                ReloadPackage(fileProvider);
            }

            List<Package> result;
            if (!_packages.TryGetValue(name, out result))
                result = new List<Package>();
            return result;
        }

        private void ReloadPackage(IFileProvider fileProvider)
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

        public List<Group> GetGroups()
        {
            if (_groups == null)
            {
                var settingsPath = Path.Combine(_env.ContentRootPath, "settings");
                if (!Directory.Exists(settingsPath))
                    Directory.CreateDirectory(settingsPath);

                _versionsPath = Path.Combine(settingsPath, "groups");
                if (!Directory.Exists(_versionsPath))
                    Directory.CreateDirectory(_versionsPath);

                IFileProvider fileProvider = new PhysicalFileProvider(_versionsPath);
                ChangeToken.OnChange(() => fileProvider.Watch("*.txt"), () =>
                    ReloadGroup(fileProvider));

                _groups = new List<Group>();
                ReloadGroup(fileProvider);
            }
            
            return _groups;
        }

        private void ReloadGroup(IFileProvider fileProvider)
        {
            _groups.Clear();

            var files = Directory.GetFiles(_groupPath);
            foreach (var file in files)
            {
                var filename = Path.GetFileName(file);
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(file);

                Stream stream = fileProvider.GetFileInfo(Path.GetFileName(file)).CreateReadStream();
                {
                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);

                    var content = Encoding.UTF8.GetString(buffer);
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        var group = new Group(fileNameWithoutExtension, content.Split(','));
                        _groups.Add(group);
                    }
                }
            }
        }
    }
}
