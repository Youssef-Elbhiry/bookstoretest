using BookStore.DataAccess.Data;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Repository
{
    internal class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(OrderHeader orderHeader)
        {
            _db.OrderHeader.Update(orderHeader);
        }

        public void UpdateStripePaymentId(int id, string SessionId, string PaymentIntentId)
        {
            var orderheader = _db.OrderHeader.Find(id);
            if (orderheader != null && !string.IsNullOrEmpty(SessionId))
            {

                orderheader.PaymentIntentId = PaymentIntentId;
                orderheader.SessionId = SessionId;
            }
        }
        public void UpdateStatus(int id, string Orderstatus, string? PaymentStatus = null)
        {
            var orderheader = _db.OrderHeader.Find(id);
            if (orderheader != null && !string.IsNullOrEmpty(Orderstatus))
            {
                orderheader.OrderStatus = Orderstatus;
                if (!string.IsNullOrEmpty(PaymentStatus))
                {
                    orderheader.PaymentStatus = PaymentStatus;
                }
            }
        }
    }
}
