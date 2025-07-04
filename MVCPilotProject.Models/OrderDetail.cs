﻿using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MVCPilotProject.Models
{
    public class OrderDetail
    {
        public int Id { get; set; }

        [Required]

        public int OrderHeaderId { get; set; }

        [ForeignKey("OrderHeaderId")]
        [ValidateNever]
        public OrderHeader OrderHeader { get; set; }

        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        [ValidateNever]
        public Product Product { get; set; }

        public int Count { get; set; }

        public double Price { get; set; }

    }
}
