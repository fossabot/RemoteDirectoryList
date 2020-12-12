﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RemoteDirectoryList.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RemoteDirectoryList.Controllers
{
    public class FilesController : Controller
    {
        private readonly ILogger<FilesController> _logger;
        private readonly AppSettings _appSettings;

        public FilesController(ILogger<FilesController> logger, IOptions<AppSettings> appSettings)
        {
            _logger = logger;
            _appSettings = appSettings.Value;
        }

        public IActionResult Directory(string path = "")
        {
            path = path.TrimStart('/');

            var model = new DirectoryViewModel
            {
                DirectoryPath = Path.Combine(_appSettings.RootDirectoryPath, path),
                ParentDirectoryPath = _appSettings.RootDirectoryPath,
                Files = new List<EntryViewModel>()
            };

            var directory = new DirectoryInfo(model.DirectoryPath);
            // The directory is not a root drive, it has a parent directory 
            if (directory.Parent != null)
            {
                model.ParentDirectoryPath = directory.Parent.FullName.Replace(_appSettings.RootDirectoryPath, "");
            }

            if (directory == null)
            {
                return NotFound();
            }

            // List all directories
            directory.GetDirectories().ToList().ForEach(file =>
            {
                var fileModel = new EntryViewModel
                {
                    LastModifyTime = file.LastWriteTimeUtc,
                    Name = file.Name,
                    Size = new FileSize { Byte = 0 },
                    IsDirectory = true
                };

                model.Files.Add(fileModel);
            });

            // List all files
            directory.GetFiles().ToList().ForEach(file =>
            {
                var fileModel = new EntryViewModel
                {
                    LastModifyTime = file.LastWriteTimeUtc,
                    Name = file.Name,
                    Size = new FileSize { Byte = file.Length },
                    IsDirectory = false
                };

                model.Files.Add(fileModel);
            });

            return base.View(model);
        }

        public IActionResult Download(string path)
        {
            path = path.TrimStart('/');
            var absolutePath = Path.Combine(_appSettings.RootDirectoryPath, path);

            if (System.IO.File.Exists(absolutePath))
            {
                var stream = System.IO.File.OpenRead(absolutePath);
                return File(stream, "octlet/stream", absolutePath.Split('\\').Last());
            }

            return NotFound();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
