using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using TopSpeed.Application.ApplicationConstants;
using TopSpeed.Application.Contracts.Priestience;
using TopSpeed.Domain.ApplicationEnums;
using TopSpeed.Domain.Models;
using TopSpeed.Infrastructure.Common;

namespace TopSpeed.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =CustomRole.MasterAdmin + "," + CustomRole.Admin)]
    public class BrandController : Controller
    {
        //DB Accesss
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILogger<BrandController> _logger;
        public BrandController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, ILogger<BrandController> logger)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                List<Brand> brands = await _unitOfWork.Brand.GetAllAsync();

                _logger.LogInformation("Brand List featched from database successfully");

                return View(brands);
            }
            catch (Exception ex)
            {
                _logger.LogError("Something Went Wrong");
                 return View();
            }
        }
        [HttpGet]

        public IActionResult Create()
        {
           

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Brand brand)
        {

            string webRootPath = _webHostEnvironment.WebRootPath;

            var file = HttpContext.Request.Form.Files;
            if (file.Count > 0)
            {
                string newFileName = Guid.NewGuid().ToString();
                var upload = Path.Combine(webRootPath, @"images\brand");
                var extension = Path.GetExtension(file[0].FileName);
                using (var fileStream = new FileStream(Path.Combine(upload, newFileName + extension), FileMode.Create))
                {
                    file[0].CopyTo(fileStream);
                }

                brand.BrandLogo = @"\images\brand\" + newFileName + extension;
            }


            if (ModelState.IsValid)
            {
                await _unitOfWork.Brand.Create(brand);
                await _unitOfWork.SaveAsync();

                TempData["success"] = CommonMessage.RecordCreated;

                return RedirectToAction(nameof(Index));

            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            Brand brand = await _unitOfWork.Brand.GetByIdAsync(id);

            return View(brand);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            Brand brand = await _unitOfWork.Brand.GetByIdAsync(id);

            return View(brand);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Brand brand)
        {
            string webRootPath = _webHostEnvironment.WebRootPath;

            var file = HttpContext.Request.Form.Files;
            if (file.Count > 0)
            {
                string newFileName = Guid.NewGuid().ToString();
                var upload = Path.Combine(webRootPath, @"images\brand");
                var extension = Path.GetExtension(file[0].FileName);

                //delete old img
                var objFromDb = await _unitOfWork.Brand.GetByIdAsync(brand.Id);

                if (objFromDb.BrandLogo != null)
                {
                    var oldImagePath = Path.Combine(webRootPath, objFromDb.BrandLogo.Trim('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }


                using (var fileStream = new FileStream(Path.Combine(upload, newFileName + extension), FileMode.Create))
                {
                    file[0].CopyTo(fileStream);
                }

                brand.BrandLogo = @"\images\brand\" + newFileName + extension;
            }

            if (ModelState.IsValid)
            {
                await _unitOfWork.Brand.Update(brand);
                await _unitOfWork.SaveAsync();
                TempData["Warning"] = CommonMessage.RecordUpdated; ;


                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            Brand brand = await _unitOfWork.Brand.GetByIdAsync(id);

            return View(brand);
        }

        [HttpPost]

        public async Task<IActionResult> Delete(Brand brand)
        {
            string webRootPath = _webHostEnvironment.WebRootPath;

            if (!string.IsNullOrEmpty(brand.BrandLogo))
            {
                //delete old img
                var objFromDb = await _unitOfWork.Brand.GetByIdAsync(brand.Id);

                if (objFromDb.BrandLogo != null)
                {
                    var oldImagePath = Path.Combine(webRootPath, objFromDb.BrandLogo.Trim('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

            }
            await _unitOfWork.Brand.Delete(brand);
            await _unitOfWork.SaveAsync();

            TempData["Delete"] = CommonMessage.RecordDeleted; ;


            return RedirectToAction(nameof(Index));
        }
    }
}
