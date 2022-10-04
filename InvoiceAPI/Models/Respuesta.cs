using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvoiceAPI.Models
{
	public class Respuesta
	{
        public string archivoTimbrado { get; set; }
		public string message { get; set; }
		public string details { get; set; }
		public string code { get; set; }
		public bool hasError
		{
			get
			{
				if (!code.Contains("200") && !code.Contains("201") && !code.Contains("202"))
					return true;
				else
					return false;
			}
		}
	}
}
