using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIM.Services.DTOs
{
    public class InvoiceDto
    {
        public int InvoiceId { get; set; }
        public int CustomerId { get; set; }

        public DateTime InvoiceDate { get; set; }

        public int Terms { get; set; }

        public DateTime DueDate { get; set; } 

        public decimal TotalAmount { get; set; } 

        public List<InvoiceLineDto> Lines { get; set; }
        public string? CustomerName { get; set; }
        public string? InvoiceNumber { get; set; }
    }
}
