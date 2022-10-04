using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvoiceAPI.Models
{
	public class Message
	{
		public string message { get; set; }
		public string details { get; set; }
		public string code { get; set; }
		public string file64 { get; set; }
		public bool hasError
		{
			get
			{
				if (code == null || code == "" || (!code.Contains("200") && !code.Contains("201") && !code.Contains("202") && !code.Contains("success")))
					return true;
				else
					return false;
			}
		}
	}

}
