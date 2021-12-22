using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using MVC_proj.DAL;
using MVC_proj.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MVC_proj.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SliderImageController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SliderImageController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        public async Task<IActionResult> Index()
        {
            var sliderImages = await _context.SliderImages.ToListAsync();
            return View(sliderImages);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SliderImage sliderImage)
        {
            if(!ModelState.IsValid)
            {
                return View();
            }

            if (!sliderImage.File.ContentType.Contains("image"))
            {
                ModelState.AddModelError(nameof(sliderImage.File), "File is unsupproted");
                return View();
            }

            if (sliderImage.File.Length > 1024 * 1000)
            {
                ModelState.AddModelError(nameof(SliderImage.File), "File size cannot be greater than 1 mb");
                return View();
            }

            string fileName = Guid.NewGuid() + sliderImage.File.FileName;
            string wwwRootPath = _env.WebRootPath;

            var path = Path.Combine(wwwRootPath,"img", fileName);
            FileStream stream = new FileStream(path, FileMode.Create);
            await sliderImage.File.CopyToAsync(stream);
            stream.Close();

            sliderImage.Image = fileName;

            await _context.SliderImages.AddAsync(sliderImage);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
