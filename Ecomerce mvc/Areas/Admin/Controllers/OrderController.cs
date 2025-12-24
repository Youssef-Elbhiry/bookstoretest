using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using BookStore.Models.ViewModels;
using BookStore.Utilites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace Ecomerce_mvc.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitofwork;
        [BindProperty]
        public Orderviewmodel orderviewmodel {  get; set; }
        public OrderController(IUnitOfWork unitofwork)
        {
            _unitofwork = unitofwork; 
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Detail(int id)
        {
            orderviewmodel = new Orderviewmodel()
            { 
               orderheader = _unitofwork.OrderHeader.Get(h=>h.Id == id,includeprops:"User"),
               orderDetails =_unitofwork.OrderDetail.GetAll(od=>od.OrderHeaderId ==id,includeprops: "Product")
            };
     
        
            return View(orderviewmodel);
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," +SD.Role_Employee)]
        public IActionResult UpdateOrderDetails()
        {
            var orderheaderfromdb = _unitofwork.OrderHeader.Get(o => o.Id == orderviewmodel.orderheader.Id);

            orderheaderfromdb.Name = orderviewmodel.orderheader.Name;
            orderheaderfromdb.StreetAddress = orderviewmodel.orderheader.StreetAddress;
            orderheaderfromdb.City = orderviewmodel.orderheader.City;
            orderheaderfromdb.PhoneNumber = orderviewmodel.orderheader.PhoneNumber;
            orderheaderfromdb.State = orderviewmodel.orderheader.State;
            orderheaderfromdb.PostalCode = orderviewmodel.orderheader.PostalCode;
            orderheaderfromdb.Carrier = orderviewmodel.orderheader.Carrier;
            orderheaderfromdb.TrakingNumber = orderviewmodel.orderheader.TrakingNumber;

            _unitofwork.Save();

            return RedirectToAction(nameof(Detail), new { id = orderviewmodel.orderheader.Id });


        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult StartProcessing()
        {
            _unitofwork.OrderHeader.UpdateStatus(orderviewmodel.orderheader.Id,SD.StatusInProcess);
            _unitofwork.Save();

            TempData["success"] = "Order Status Updated Successfuly";

            return RedirectToAction(nameof(Detail), new { id = orderviewmodel.orderheader.Id });

        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderheaderfromdb = _unitofwork.OrderHeader.Get(o=>o.Id ==orderviewmodel.orderheader.Id);
            orderheaderfromdb.Carrier = orderviewmodel.orderheader.Carrier;
            orderheaderfromdb.TrakingNumber = orderviewmodel.orderheader.TrakingNumber;
            orderheaderfromdb.OrderStatus = SD.StatusShiped;
            orderheaderfromdb.ShipingDate = DateTime.Now;
            if(orderviewmodel.orderheader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderheaderfromdb.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }
            _unitofwork.Save();

            TempData["success"] = "Order Shiped Successfuly";

            return RedirectToAction(nameof(Detail), new { id = orderviewmodel.orderheader.Id });

        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        public IActionResult CancelOrder()
        { 
            var orderheader = _unitofwork.OrderHeader.Get(o => o.Id == orderviewmodel.orderheader.Id);

            if(orderheader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions()
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderheader.PaymentIntentId
                };

                var service = new RefundService();

                Refund refund = service.Create(options);

                _unitofwork.OrderHeader.UpdateStatus(orderheader.Id, SD.StatusCancelld, SD.StatusRefunded);

            }
            else
            {
                _unitofwork.OrderHeader.UpdateStatus(orderheader.Id, SD.StatusCancelld, SD.StatusCancelld);
            }
            _unitofwork.Save();
            TempData["success"] = "Order Canseld Successfuly";
            return RedirectToAction(nameof(Detail), new { id = orderviewmodel.orderheader.Id });

        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Company )]
        public IActionResult PayDelayedPayment()
        {
            string domain = "https://localhost:7032";
            var options = new SessionCreateOptions()
            {
                SuccessUrl = domain + $"/Admin/Order/OrderConfirmationPage/{orderviewmodel.orderheader.Id}" ,
                CancelUrl = domain + $"/Admin/Order/Detail/{orderviewmodel.orderheader.Id}",
                Mode = "payment",
                LineItems = new List<SessionLineItemOptions>()
            };

            IEnumerable<OrderDetail> orderDetails = _unitofwork.OrderDetail.GetAll(d => d.OrderHeaderId == orderviewmodel.orderheader.Id,includeprops:"Product");
            foreach(var item in orderDetails)
            {
                var sessionlineitem = new SessionLineItemOptions()
                {
                    PriceData = new SessionLineItemPriceDataOptions()
                    {
                        UnitAmount = (long)(item.TotalPrice * 100),
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

            _unitofwork.OrderHeader.UpdateStripePaymentId(orderviewmodel.orderheader.Id, session.Id, session.PaymentIntentId);
            _unitofwork.Save();

            Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303);
            
        
        }

        public IActionResult OrderConfirmationPage(int id)
        {
            var orderheader = _unitofwork.OrderHeader.Get(o=>o.Id == id);
            if(orderheader is not null)
            {
                
                var service = new SessionService();
                var session = service.Get(orderheader.SessionId);
                if(session.PaymentStatus.ToLower() == "paid")
                {
                    orderheader.PaymentIntentId = session.PaymentIntentId;
                    orderheader.PaymentDate = DateTime.Now;
                    orderheader.PaymentStatus = SD.PaymentStatusApproved;
                }

             
            }
            return View("OrderConfirmationPage", orderheader.Id);
        }

        #region Api Call
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaders;
           
            if(User.IsInRole(SD.Role_Admin)|| User.IsInRole(SD.Role_Employee))
            {
                orderHeaders = _unitofwork.OrderHeader.GetAll(includeprops: "User");
       
            }
            else
            {
                var claimsidentity = (ClaimsIdentity)User.Identity!;
                var userid = claimsidentity.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                orderHeaders = _unitofwork.OrderHeader.GetAll( o=>o.ApplicationUserId == userid,includeprops: "User");
            }

                switch (status)
                {
                    case "inprocess":
                        orderHeaders = orderHeaders.Where(o => o.OrderStatus == SD.StatusInProcess);
                        break;
                    case "pending":
                        orderHeaders = orderHeaders.Where(o => o.OrderStatus == SD.PaymentStatusDelayedPayment);
                        break;
                    case "approved":
                        orderHeaders = orderHeaders.Where(o => o.OrderStatus == SD.StatusApproved);
                        break;
                    case "completed":
                        orderHeaders = orderHeaders.Where(o => o.OrderStatus == SD.StatusShiped);
                        break;



                }
            return Json(new {data =  orderHeaders});
        }
        #endregion
    }
}
