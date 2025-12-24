using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Models
{
    public class ShopingCart
    {
        public int Id { get; set; }

        public string ApplicationUserId { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(ApplicationUserId))]
        public ApplicationUser User { get; set; }
        [Range(1,1000, ErrorMessage ="Quantity Must be between 1 and 1000")]
        public int Count { get; set; }

        public int ProductId { get; set; }

        [ValidateNever]
        [ForeignKey(nameof(ProductId))]
        public Product Product { get; set; }

        [NotMapped]
        public double Price { get; set; }


    }
}
