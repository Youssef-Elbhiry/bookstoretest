using BookStore.DataAccess.Repository;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Utilites;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace Ecomerce_mvc.ViewComponents
{
    public class ShopingCartViewComponent : ViewComponent
    {
        private readonly IUnitOfWork _unitofwork;

        public ShopingCartViewComponent(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork;
        }


        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsidentity = (ClaimsIdentity)User.Identity!;
            var claim = claimsidentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                if(HttpContext.Session.GetInt32(SD.CartSession) == null)
                {
                    HttpContext.Session.SetInt32(SD.CartSession,
               _unitofwork.shopingCart.GetAll(s => s.ApplicationUserId == claim.Value).Count());
                }
                return View(HttpContext.Session.GetInt32(SD.CartSession));
            }
            else
            {
                HttpContext.Session.Clear();
                return View(0);
            }
        }
    }
}
