using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using BookStore.Utilites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Reflection.Metadata;

namespace Ecomerce_mvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitofwork , IWebHostEnvironment webHostEnvironment)
        {
            _unitofwork = unitofwork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            var products = _unitofwork.Product.GetAll(includeprops:"category,,");
            return View(products);
        }

        [HttpGet]
        public IActionResult Upsert(int? id)
        { 
             
            ProductViewModel productviewmodel = new ProductViewModel()
            {
                product = new Product(),
                Categorylist = _unitofwork.Category.GetAll().Select(c => new SelectListItem
                {
                    Text = c.Name,
                    Value = c.Id.ToString()
                })
            };
            if (id == null || id == 0) { 
            
            
            }
            else
            {
                productviewmodel.product = _unitofwork.Product.Get(p=>p.Id == id,includeprops: "ProductImages");
            }
                return View(productviewmodel);
        }

        [HttpPost]
        public IActionResult Upsert(ProductViewModel productvm,List<IFormFile> files)
        {
            if (ModelState.IsValid) {
                if (productvm.product.Id == 0)
                {
                    _unitofwork.Product.Add(productvm.product);
                }
                else
                {
                    _unitofwork.Product.Update(productvm.product);
                }

                _unitofwork.Save();

                if (files is not null) {
                    string wwwroot = _webHostEnvironment.WebRootPath;
                    string productpath = wwwroot + "/Images/Product" + "/product-"+productvm.product.Id;
                  
                    if (!Directory.Exists(productpath))
                        Directory.CreateDirectory(productpath);

                    foreach (IFormFile file in files)
                    {
                        String filename = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);


                        using (var filestream = new FileStream(Path.Combine(productpath, filename), FileMode.Create))
                        {
                            file.CopyTo(filestream);
                        }
                        var productimage = new ProductImages()
                        {
                            ImageUrl = "/Images/Product" + "/product-" + productvm.product.Id + "/" + filename,
                            ProductId = productvm.product.Id
                        };

                       if (productvm.product.ProductImages is null)
                        {
                            productvm.product.ProductImages = new List<ProductImages>();
                        }
                        productvm.product.ProductImages.Add(productimage);
                        
                       
                    }
                    _unitofwork.Product.Update(productvm.product);
                    _unitofwork.Save();
                }
                TempData["success"] = "Product created/updated successfully";
              
                return RedirectToAction("Index");
            
            }
            productvm.Categorylist = _unitofwork.Category.GetAll().Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            });
            return View(productvm);
        }

        public IActionResult DeleteImg(int id)
        {
            var productimage = _unitofwork.ProductImages.Get(pi => pi.Id == id);
           
            if (productimage != null)
            {
                int productid = productimage.ProductId;
                string imagepath = Path.Combine(_webHostEnvironment.WebRootPath, productimage.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(imagepath))
                    System.IO.File.Delete(imagepath);

                _unitofwork.ProductImages.Delete(productimage);
                _unitofwork.Save();
                TempData["success"] = "Image deleted successfuly";
                return RedirectToAction(nameof(Upsert), new { id = productid });

            }
            return NotFound();
        }
        #region api calls
        [HttpGet]
        public IActionResult GetAll()
        {
            var products = _unitofwork.Product.GetAll(includeprops:"category,,");
          return Json(new { data = products });
        }
        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var product = _unitofwork.Product.Get(p => p.Id == id, includeprops: "ProductImages");
            if (product is null)
            {
                return Json(new { success = false, message = " product not found" });
            }
            foreach (var productimage in product.ProductImages)
            {
                string ProductImgPath = Path.Combine(_webHostEnvironment.WebRootPath, productimage.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(ProductImgPath))
                {
                    System.IO.File.Delete(ProductImgPath);
                }

                _unitofwork.ProductImages.Delete(productimage);
            }
            string directorypath = "Images/Product/product-" + product.Id;
            string fullpath = Path.Combine(_webHostEnvironment.WebRootPath, directorypath);
            if (Directory.Exists(fullpath))
                {
                Directory.Delete(fullpath);
            }
            _unitofwork.Product.Delete(product);
            _unitofwork.Save();
            return Json(new { success = true, message = " Product Deleted" });
        }
        #endregion
    }
}
