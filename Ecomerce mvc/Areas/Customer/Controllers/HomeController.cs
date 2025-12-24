using System.Diagnostics;
using System.Security.Claims;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Utilites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecomerce_mvc.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger,IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> products = _unitOfWork.Product.GetAll(includeprops: "category,ProductImages");
   
            return View(products);
        }
        [HttpGet]
        public IActionResult Detail(int? id)
        {
            if(id is not null)
            {
                var shopingcart = new ShopingCart()
                {
                    Product = _unitOfWork.Product.Get(p => p.Id == id, includeprops: "category,ProductImages"),
                    Count = 1,
                    ProductId = (int)id
                };
              
                return View(shopingcart);
            }
            return NotFound();

        }
        [Authorize]
        [HttpPost]
        public IActionResult Detail(ShopingCart shopingCart)
        {
            var claimsidentity = (ClaimsIdentity)User.Identity!;
            var userid = claimsidentity.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            shopingCart.ApplicationUserId = userid;
            var cartfromdb = _unitOfWork.shopingCart.Get(s=>s.ProductId == shopingCart.ProductId && s.ApplicationUserId == shopingCart.ApplicationUserId);
            if(cartfromdb is null)
            {
                _unitOfWork.shopingCart.Add(shopingCart);
            }
            else
            {
                cartfromdb.Count = shopingCart.Count;
            }
            _unitOfWork.Save();

            HttpContext.Session.SetInt32(SD.CartSession, 
                _unitOfWork.shopingCart.GetAll(s => s.ApplicationUserId == userid).Count());
            return RedirectToAction(nameof(Index));

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
