using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Utilites
{
    public static class SD
    {
        public const string Role_Customer = "Customer";
        public const string Role_Company= "Company";
        public const string Role_Admin = "Admin";
        public const string Role_Employee = "Employee";

        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusInProcess = "Processing";
        public const string StatusShiped = "Shiped";
        public const string StatusCancelld = "Cancelld";
        public const string StatusRefunded = "Refunded";

        public const string CartSession = "ShopingCart";

        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusRejected = "Rejected";
        public const string PaymentStatusDelayedPayment = "ApprovedForDelayedPayment";
    }
}
