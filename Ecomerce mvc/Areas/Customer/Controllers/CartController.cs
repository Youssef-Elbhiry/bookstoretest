using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using BookStore.Utilites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace Ecomerce_mvc.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public ShopingCartViewModel cartViewModel { get; set; }

        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            cartViewModel = new ShopingCartViewModel();
          

        }

        public IActionResult Index()
        {
            var claimsidentity = (ClaimsIdentity)User.Identity!;
            var userid = claimsidentity.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            cartViewModel = new ShopingCartViewModel()
            {
                shopingCartList = _unitOfWork.shopingCart.GetAll(s => s.ApplicationUserId == userid, includeprops: "Product"),
                orderHeader = new OrderHeader()
            };


            if (cartViewModel.shopingCartList.Count() == 0)
            {
                return Content("Your cart is empty");
            }
            var productimages = _unitOfWork.ProductImages.GetAll();

            foreach (var shopingcart in cartViewModel.shopingCartList)
            {
                shopingcart.Price = GetPriceByQuantity(shopingcart);
                shopingcart.Product.ProductImages = productimages.Where(pi => pi.ProductId == shopingcart.ProductId).ToList();

                cartViewModel.orderHeader.OrderTotal += shopingcart.Price * shopingcart.Count;
            }
            return View(cartViewModel);
        }
        public double GetPriceByQuantity(ShopingCart shopingcart)
        {
            if (shopingcart.Count < 50)
            {
                return shopingcart.Product.Price;
            }
            else if (shopingcart.Count <= 100)
            {
                return shopingcart.Product.Price50;
            }
            else
            {
                return shopingcart.Product.Price100;
            }
        }

        public IActionResult PlusOne(int id)
        {
            var shopingcart = _unitOfWork.shopingCart.Get(s => s.Id == id);

            if (shopingcart is not null)
            {
                shopingcart.Count++;
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return NotFound();
        }
        public IActionResult MinusOne(int id)
        {
            var shopingcart = _unitOfWork.shopingCart.Get(s => s.Id == id);

            if (shopingcart is not null)
            {
                shopingcart.Count--;
                if (shopingcart.Count == 0)
                {
                    HttpContext.Session.SetInt32(SD.CartSession,
                    _unitOfWork.shopingCart.GetAll(s => s.ApplicationUserId == shopingcart.ApplicationUserId).Count() - 1);
                    _unitOfWork.shopingCart.Delete(shopingcart);
                }
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return NotFound();
        }
        public IActionResult Delete(int id)
        {
            var shopingcart = _unitOfWork.shopingCart.Get(s => s.Id == id);

            if (shopingcart is not null)
            {
                HttpContext.Session.SetInt32(SD.CartSession,
                _unitOfWork.shopingCart.GetAll(s => s.ApplicationUserId == shopingcart.ApplicationUserId).Count()-1);
                _unitOfWork.shopingCart.Delete(shopingcart);
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return NotFound();
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            var userid = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            cartViewModel = new ShopingCartViewModel()
            {
                shopingCartList = _unitOfWork.shopingCart.GetAll(s => s.ApplicationUserId == userid, includeprops: "Product"),
                orderHeader = new OrderHeader()
            };

            cartViewModel.orderHeader.User = _unitOfWork.ApplicationUser.Get(u => u.Id == userid);
            cartViewModel.orderHeader.Name = cartViewModel.orderHeader.User.Name;
            cartViewModel.orderHeader.City = cartViewModel.orderHeader.User.City!;
            cartViewModel.orderHeader.StreetAddress = cartViewModel.orderHeader.User.StreetAddress!;
            cartViewModel.orderHeader.PostalCode = cartViewModel.orderHeader.User.PostalCode!;
            cartViewModel.orderHeader.State = cartViewModel.orderHeader.User.State!;
            cartViewModel.orderHeader.PhoneNumber = cartViewModel.orderHeader.User.PhoneNumber!;
            cartViewModel.orderHeader.ApplicationUserId = userid;
            var productimages = _unitOfWork.ProductImages.GetAll();
            foreach (var shopingcart in cartViewModel.shopingCartList)
            {
                shopingcart.Price = GetPriceByQuantity(shopingcart);
                shopingcart.Product.ProductImages = productimages.Where(pi => pi.ProductId == shopingcart.ProductId).ToList();
                cartViewModel.orderHeader.OrderTotal += shopingcart.Price * shopingcart.Count;
            }

            return View(cartViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public IActionResult SummaryPost()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity!;
            var userid = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            cartViewModel.shopingCartList = _unitOfWork.shopingCart.GetAll(s => s.ApplicationUserId == userid, includeprops: "Product");

            foreach (var shopingcart in cartViewModel.shopingCartList)
            {
                shopingcart.Price = GetPriceByQuantity(shopingcart);
                cartViewModel.orderHeader.OrderTotal += shopingcart.Price * shopingcart.Count;
            }

            if (ModelState.IsValid)
            {

                cartViewModel.orderHeader.ApplicationUserId = userid;
                cartViewModel.orderHeader.OrderDate = DateTime.Now;

                ApplicationUser appuser = _unitOfWork.ApplicationUser.Get(u => u.Id == userid);

                if (appuser.CompanyId.GetValueOrDefault() == 0)
                {
                    //customer
                    cartViewModel.orderHeader.OrderStatus = SD.StatusPending;
                    cartViewModel.orderHeader.PaymentStatus = SD.PaymentStatusPending;
                }
                else
                {
                    // company user
                    cartViewModel.orderHeader.OrderStatus = SD.StatusApproved;
                    cartViewModel.orderHeader.PaymentStatus = SD.PaymentStatusDelayedPayment;
                }

                _unitOfWork.OrderHeader.Add(cartViewModel.orderHeader);
                _unitOfWork.Save();

                foreach (var shopingcart in cartViewModel.shopingCartList)
                {
                    var orderdetail = new OrderDetail()
                    {
                        OrderHeaderId = cartViewModel.orderHeader.Id,
                        ProductId = shopingcart.ProductId,
                        Count = shopingcart.Count,
                        TotalPrice = shopingcart.Price
                    };
                    _unitOfWork.OrderDetail.Add(orderdetail);
                    _unitOfWork.Save();
                }

                if (appuser.CompanyId.GetValueOrDefault() == 0)
                {
                    //stripe
                    string domain = $"{Request.Scheme}://{Request.Host}";
                    var options = new Stripe.Checkout.SessionCreateOptions
                    {
                        SuccessUrl = domain + $"/Customer/Cart/ConfirmationPage/{cartViewModel.orderHeader.Id}",
                        CancelUrl = domain + "/Customer/Cart/Index",
                        LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                        Mode = "payment",
                    };
                    foreach (var item in cartViewModel.shopingCartList)
                    {
                        var sessionlineitem = new SessionLineItemOptions()
                        {
                            PriceData = new SessionLineItemPriceDataOptions()
                            {
                                UnitAmount = (long)(item.Price * 100),
                                Currency = "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions()
                                {
                                    Name = item.Product.Title
                                }
                            },
                            Quantity = item.Count
                        };

                        options.LineItems.Add(sessionlineitem);
                    }
                    var service = new Stripe.Checkout.SessionService();
                    Stripe.Checkout.Session session = service.Create(options);

                    _unitOfWork.OrderHeader.UpdateStripePaymentId(cartViewModel.orderHeader.Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.Save();


                    Response.Headers.Add("Location", session.Url);
                    return new StatusCodeResult(303);




                }
                return RedirectToAction(nameof(ConfirmationPage), new { id = cartViewModel.orderHeader.Id } );
            }
            return RedirectToAction("Summary", cartViewModel);

        }

        public IActionResult ConfirmationPage(int id)
        {
            var orderheader = _unitOfWork.OrderHeader.Get(o => o.Id == id);
            if(orderheader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderheader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
           

            var shopingcartlist = _unitOfWork.shopingCart.GetAll(s=>s.ApplicationUserId == orderheader.ApplicationUserId);
            _unitOfWork.shopingCart.DeleteRange(shopingcartlist);
            _unitOfWork.Save();
            return View(id);
        }
    }

}
