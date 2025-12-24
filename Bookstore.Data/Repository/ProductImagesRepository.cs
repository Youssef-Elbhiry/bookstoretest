using BookStore.DataAccess.Data;
using BookStore.DataAccess.Repository.IRepository;
using BookStore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.DataAccess.Repository
{
    public class ProductImagesRepository : Repository<ProductImages>,IProductImagesRepository
    {
        private readonly ApplicationDbContext _db;

        public ProductImagesRepository(ApplicationDbContext db):base(db)
        {
            _db = db;
        }
        public void Update(ProductImages productImages)
        {
           _db.ProductImages.Update(productImages);
        }
    }
}
