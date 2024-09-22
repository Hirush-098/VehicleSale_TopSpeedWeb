using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using TopSpeed.Application.ApplicationConstants;
using TopSpeed.Application.Contracts.Priestience;
using TopSpeed.Application.Services.Interface;
using TopSpeed.Domain.ApplicationEnums;
using TopSpeed.Domain.Models;
using TopSpeed.Domain.ViewModel;
using TopSpeed.Infrastructure.Common;

namespace TopSpeed.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = CustomRole.MasterAdmin + "," + CustomRole.Admin)]

    public class PostController : Controller
    {
        //DB Accesss
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUserNameService _userName;
        
        public PostController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment, IUserNameService userName)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _userName = userName;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<Post> posts = await _unitOfWork.Post.GetAllPost();
            return View(posts);
        }
        [HttpGet]

        public IActionResult Create()
        {
            IEnumerable<SelectListItem> brandList = _unitOfWork.Brand.Query().Select(x => new SelectListItem
            {
                Text = x.Name.ToUpper(),
                Value = x.Id.ToString(),
            });

            IEnumerable<SelectListItem> vehicleTypeList = _unitOfWork.VehicleType.Query().Select(x => new SelectListItem
            {
                Text = x.Name.ToUpper(),
                Value = x.Id.ToString(),
            });

            IEnumerable<SelectListItem> engineAndFuelType = Enum.GetValues(typeof(EngineAndFuelType))
                .Cast<EngineAndFuelType>()
                .Select(x => new SelectListItem
                {
                    Text = x.ToString().ToUpper(),
                    Value = ((int)x).ToString(),
                });
            IEnumerable<SelectListItem> transmission = Enum.GetValues(typeof(Transmission))
               .Cast<Transmission>()
               .Select(x => new SelectListItem
               {
                   Text = x.ToString().ToUpper(),
                   Value = ((int)x).ToString(),
               });

            PostVm postVm = new PostVm
            {
                Post = new Post(),
                BrandList = brandList,
                VehicleTypeList = vehicleTypeList,
                EngineAndFuelTypeList = engineAndFuelType,
                TransmissionList = transmission
            };

            return View(postVm);
        }

        [HttpPost]
        public async Task<IActionResult> Create(PostVm postVm)
        {

            string webRootPath = _webHostEnvironment.WebRootPath;

            var file = HttpContext.Request.Form.Files;
            if (file.Count > 0)
            {
                string newFileName = Guid.NewGuid().ToString();
                var upload = Path.Combine(webRootPath, @"images\post");
                var extension = Path.GetExtension(file[0].FileName);
                using (var fileStream = new FileStream(Path.Combine(upload, newFileName + extension), FileMode.Create))
                {
                    file[0].CopyTo(fileStream);
                }

                postVm.Post.VehicleImage = @"\images\post\" + newFileName + extension;
            }


            if (ModelState.IsValid)
            {
                await _unitOfWork.Post.Create(postVm.Post);
                await _unitOfWork.SaveAsync();

                TempData["success"] = CommonMessage.RecordCreated;

                return RedirectToAction(nameof(Index));

            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            Post post = await _unitOfWork.Post.GetPostById(id);
            post.CreatedBy = await _userName.GetUserName(post.CreatedBy);
            post.ModifiedBy = await _userName.GetUserName(post.ModifiedBy);
            return View(post);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            Post post = await _unitOfWork.Post.GetPostById(id);

            IEnumerable<SelectListItem> brandList = _unitOfWork.Brand.Query().Select(x => new SelectListItem
            {
                Text = x.Name.ToUpper(),
                Value = x.Id.ToString(),
            });

            IEnumerable<SelectListItem> vehicleTypeList = _unitOfWork.VehicleType.Query().Select(x => new SelectListItem
            {
                Text = x.Name.ToUpper(),
                Value = x.Id.ToString(),
            });

            IEnumerable<SelectListItem> engineAndFuelType = Enum.GetValues(typeof(EngineAndFuelType))
                .Cast<EngineAndFuelType>()
                .Select(x => new SelectListItem
                {
                    Text = x.ToString().ToUpper(),
                    Value = ((int)x).ToString(),
                });
            IEnumerable<SelectListItem> transmission = Enum.GetValues(typeof(Transmission))
               .Cast<Transmission>()
               .Select(x => new SelectListItem
               {
                   Text = x.ToString().ToUpper(),
                   Value = ((int)x).ToString(),
               });

            PostVm postVm = new PostVm
            {
                Post = post,
                BrandList = brandList,
                VehicleTypeList = vehicleTypeList,
                EngineAndFuelTypeList = engineAndFuelType,
                TransmissionList = transmission
            };

            return View(postVm);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PostVm postVm)
        {
            string webRootPath = _webHostEnvironment.WebRootPath;

            var file = HttpContext.Request.Form.Files;
            if (file.Count > 0)
            {
                string newFileName = Guid.NewGuid().ToString();
                var upload = Path.Combine(webRootPath, @"images\post");
                var extension = Path.GetExtension(file[0].FileName);

                //delete old img
                var objFromDb = await _unitOfWork.Post.GetByIdAsync(postVm.Post.Id);

                if (objFromDb.VehicleImage != null)
                {
                    var oldImagePath = Path.Combine(webRootPath, objFromDb.VehicleImage.Trim('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }


                using (var fileStream = new FileStream(Path.Combine(upload, newFileName + extension), FileMode.Create))
                {
                    file[0].CopyTo(fileStream);
                }

                postVm.Post.VehicleImage = @"\images\post\" + newFileName + extension;
            }

            if (ModelState.IsValid)
            {
                await _unitOfWork.Post.Update(postVm.Post);
                await _unitOfWork.SaveAsync();
                TempData["Warning"] = CommonMessage.RecordUpdated; ;


                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            Post post = await _unitOfWork.Post.GetByIdAsync(id);
            IEnumerable<SelectListItem> brandList = _unitOfWork.Brand.Query().Select(x => new SelectListItem
            {
                Text = x.Name.ToUpper(),
                Value = x.Id.ToString(),
            });

            IEnumerable<SelectListItem> vehicleTypeList = _unitOfWork.VehicleType.Query().Select(x => new SelectListItem
            {
                Text = x.Name.ToUpper(),
                Value = x.Id.ToString(),
            });

            IEnumerable<SelectListItem> engineAndFuelType = Enum.GetValues(typeof(EngineAndFuelType))
                .Cast<EngineAndFuelType>()
                .Select(x => new SelectListItem
                {
                    Text = x.ToString().ToUpper(),
                    Value = ((int)x).ToString(),
                });
            IEnumerable<SelectListItem> transmission = Enum.GetValues(typeof(Transmission))
               .Cast<Transmission>()
               .Select(x => new SelectListItem
               {
                   Text = x.ToString().ToUpper(),
                   Value = ((int)x).ToString(),
               });

            PostVm postVm = new PostVm
            {
                Post = post,
                BrandList = brandList,
                VehicleTypeList = vehicleTypeList,
                EngineAndFuelTypeList = engineAndFuelType,
                TransmissionList = transmission
            };

            return View(postVm);
        }

        [HttpPost]

        public async Task<IActionResult> Delete(PostVm postVm)
        {
            string webRootPath = _webHostEnvironment.WebRootPath;

            if (!string.IsNullOrEmpty(postVm.Post.VehicleImage))
            {
                //delete old img
                var objFromDb = await _unitOfWork.Post.GetByIdAsync(postVm.Post.Id);

                if (objFromDb.VehicleImage != null)
                {
                    var oldImagePath = Path.Combine(webRootPath, objFromDb.VehicleImage.Trim('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

            }
            await _unitOfWork.Post.Delete(postVm.Post);
            await _unitOfWork.SaveAsync();

            TempData["Delete"] = CommonMessage.RecordDeleted; ;


            return RedirectToAction(nameof(Index));
        }
    }
}
