using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace InvoiceAPI.Models
{
    public class InvoiceAPIContext : DbContext
    {
        public InvoiceAPIContext (DbContextOptions<InvoiceAPIContext> options)
            : base(options)
        {
        }

        public DbSet<InvoiceAPI.Models.Invoice> Invoice { get; set; }
    }
}
