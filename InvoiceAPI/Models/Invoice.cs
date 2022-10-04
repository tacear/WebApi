using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace InvoiceAPI.Models
{
	public class Invoice
	{
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		[Key, Column(Order = 0)]
		public int Id { get; set; }
		public String xmlfile { get; set; }
		public String numCert { get; set; }
		public String user { get; set; }
		public String password { get; set; }
		public String opc1 { get; set; }
		public String opc2 { get; set; }

		
	}
}
