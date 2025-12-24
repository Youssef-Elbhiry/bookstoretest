using BookStore.DataAccess.Data;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Repository
{
    internal class ShopingCartRepository : Repository<ShopingCart>, IShopingCartRepository
    {
        private ApplicationDbContext _db;
        public ShopingCartRepository(ApplicationDbContext db):base(db) 
        {
            _db = db;
        }
        public void Update(ShopingCart shopingcart)
        {
           _db.ShopingCarts.Update(shopingcart);
        }
    }
}
