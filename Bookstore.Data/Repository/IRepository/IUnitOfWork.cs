using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        public ICategoryRepository Category { get;}
        public IProductRepository Product { get; }
        public ICompanyRepository Company { get; }

        public IShopingCartRepository shopingCart { get; }
        public IOrderHeaderRepository OrderHeader { get; }
        public IOrderDetailRepository OrderDetail { get; }
        public  IApplicationUserRepository ApplicationUser { get; }

        public IProductImagesRepository ProductImages { get; }

        void Save();
    }
}
