using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Repository.Models
{
    public class Invoice : BaseEntity
    {
        public int InvoiceId { get; set; }
        public string? InvoiceNumber { get; set; }
        public int CustomerId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public int Terms { get; set; }
        public decimal TotalAmount { get; set; }
        public List<InvoiceLine> Lines { get; set; }

        public string? CustomerName { get; set; }
    }
   
}
