using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCPilotProject.DataAccess.Repository.IRepository;
using MVCPilotProject.Models;
using MVCPilotProject.Models.ViewModels;
using MVCPilotProject.Utility;
using Stripe;
using Stripe.Checkout;
using Stripe.Climate;
using System.Security.Claims;

namespace MVCPilotProjectWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        [BindProperty]
        public OrderVM OrderVm { get; set; }

        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int orderId)
        {
            OrderVm = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(o=>o.Id ==orderId, includeParameter:"ApplicationUser"),
                OrderDetails = _unitOfWork.OrderDetail.GetAll(u=>u.OrderHeaderId == orderId, includeParameter:"Product" )
            };

            return View(OrderVm);
        }

        [HttpPost]
        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        public IActionResult UpdateOrderDetails()
        {
            var orderHeaderFromView = _unitOfWork.OrderHeader.Get(o => o.Id == OrderVm.OrderHeader.Id);

            orderHeaderFromView.Name = OrderVm.OrderHeader.Name;
            orderHeaderFromView.PhoneNumber = OrderVm.OrderHeader.PhoneNumber;
            orderHeaderFromView.StreetAddress = OrderVm.OrderHeader.StreetAddress;
            orderHeaderFromView.City = OrderVm.OrderHeader.City;
            orderHeaderFromView.State = OrderVm.OrderHeader.State;
            orderHeaderFromView.PostalCode= OrderVm.OrderHeader.PostalCode;

            if (!string.IsNullOrEmpty(OrderVm.OrderHeader.Carrier))
            {
                orderHeaderFromView.Carrier = OrderVm.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVm.OrderHeader.TrackingNumber))
            {
                orderHeaderFromView.TrackingNumber = OrderVm.OrderHeader.TrackingNumber;
            }

            _unitOfWork.OrderHeader.Update(orderHeaderFromView);
            _unitOfWork.Save();

            TempData["Success"] = "Order details updated successfully";

            return RedirectToAction(nameof(Details), new {orderId = orderHeaderFromView.Id});
        }

        [ActionName("Details")]
        [HttpPost]
        public IActionResult DetailsPayNow()
        {
            OrderVm.OrderHeader = _unitOfWork.OrderHeader
                .Get(u => u.Id == OrderVm.OrderHeader.Id, includeParameter: "ApplicationUser");
            OrderVm.OrderDetails = _unitOfWork.OrderDetail
                .GetAll(u => u.OrderHeaderId == OrderVm.OrderHeader.Id, includeParameter: "Product");

            var domain = "http://localhost:5193/"; //Request.Scheme + "://" + Request.Host.Value + "/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={OrderVm.OrderHeader.Id}",
                CancelUrl = domain + $"admin/order/details?orderId={OrderVm.OrderHeader.Id}",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in OrderVm.OrderDetails)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.Price * 100), // $20.50 => 2050
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product.Title
                        }
                    },
                    Quantity = item.Count
                };
                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);

            _unitOfWork.OrderHeader.UpdateStripePaymentId(OrderVm.OrderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);

            return new StatusCodeResult(303);
        }

        public IActionResult PaymentConfirmation(int orderHeaderId)
        {
            OrderHeader orderHeader = _unitOfWork.OrderHeader.Get(o => o.Id == orderHeaderId);
            if (orderHeader.PaymentStatus != SD.PaymentStatusDelayedPayment)
            {
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeader.UpdateStripePaymentId(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeader.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }

            return View(orderHeaderId);
        }

        [HttpPost]
        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(OrderVm.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();

            TempData["Success"] = "Order details updated successfully";

            return RedirectToAction(nameof(Details), new { orderId = OrderVm.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        public IActionResult ShipOrder()
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVm.OrderHeader.Id);

            orderHeader.TrackingNumber = OrderVm.OrderHeader.TrackingNumber;
            orderHeader.Carrier = OrderVm.OrderHeader.Carrier;
            orderHeader.OrderStatus = SD.StatusShipped;
            orderHeader.ShippingDate = DateTime.Now;

            if(orderHeader.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderHeader.PaymentDueDate = DateTime.Now.AddDays(30);
            }

            _unitOfWork.OrderHeader.Update(orderHeader);
            _unitOfWork.Save();

            TempData["Success"] = "Order shipped successfully";

            return RedirectToAction(nameof(Details), new { orderId = OrderVm.OrderHeader.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.RoleAdmin + "," + SD.RoleEmployee)]
        public IActionResult CancelOrder()
        {
            var orderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVm.OrderHeader.Id);

            if(orderHeader.PaymentStatus == SD.PaymentStatusApproved)
            {
                var option = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeader.PaymentIntentId
                };

                var service = new RefundService();
                Refund refund = service.Create(option);

                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeader.Id, SD.StatusCancelled, SD.StatusCancelled);
            }

            _unitOfWork.Save();

            TempData["Success"] = "Order cancelled successfully";

            return RedirectToAction(nameof(Details), new { orderId = OrderVm.OrderHeader.Id });
        }

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            List<OrderHeader> orders = new List<OrderHeader>();

            if(User.IsInRole(SD.RoleAdmin) || User.IsInRole(SD.RoleEmployee))
            {
                orders = _unitOfWork.OrderHeader.GetAll(includeParameter: "ApplicationUser").ToList();
            }
            else
            {
                var claimsUserIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsUserIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                orders = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == userId, includeParameter: "ApplicationUser").ToList();
            }

                switch (status)
                {
                    case "pending":
                        orders = orders.Where(o => o.PaymentStatus == SD.PaymentStatusDelayedPayment).ToList();
                        break;
                    case "inprocess":
                        orders = orders.Where(o => o.OrderStatus == SD.StatusInProcess).ToList();
                        break;
                    case "completed":
                        orders = orders.Where(o => o.OrderStatus == SD.StatusShipped).ToList();
                        break;
                    case "approved":
                        orders = orders.Where(o => o.OrderStatus == SD.StatusApproved).ToList();
                        break;
                    default:
                        break;

                }

            return Json(new {data = orders});
        }
    }
}
