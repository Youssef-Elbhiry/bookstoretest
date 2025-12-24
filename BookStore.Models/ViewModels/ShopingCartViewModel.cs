using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Models.ViewModels
{
    public class ShopingCartViewModel
    {
        [ValidateNever]
      public  IEnumerable<ShopingCart> shopingCartList {  get; set; }
        public OrderHeader orderHeader { get; set; }
     
    }
}
