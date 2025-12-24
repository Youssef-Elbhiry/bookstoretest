using BookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository :IRepository<OrderHeader>
    {
        void Update(OrderHeader orderHeader);
        public void UpdateStatus(int id, string Orderstatus, string? PaymentStatus = null);
        public void UpdateStripePaymentId(int id, string SessionId, string PaymentIntentId);
        

    }
}
