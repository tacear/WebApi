using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvoiceAPI.Models
{
	public class ComprobantesDTO
	{
		public string uuid { get; set; }
		public string username { get; set; }
		public string key_id { get; set; }
		public string version { get; set; }
		public string rfc { get; set; }
		public string serie { get; set; }
		public string folio { get; set; }
		public DateTime fecha { get; set; }
		public string noSerieCert { get; set; }
		public string selloDigest { get; set; }
		public DateTime fechaEmision { get; set; }
		public string receptor_rfc { get; set; }
		public string receptor_nombre { get; set; }
		public string infaduanera { get; set; }
		public int currency_id { get; set; }
		public DateTime dateExchangeRate { get; set; }
		public decimal ExchangeRate { get; set; }
		public bool cancelado { get; set; }
		public DateTime fecha_Cancelacion { get; set; }
		public string tipo_comprobante { get; set; }
		public decimal total { get; set; }
		public decimal total_impuestos_t { get; set; }
		public DateTime fechaTimbrado { get; set; }
		public byte[] XML_Comprobante { get; set; }
		public string RfcProvCertif { get; set; }
	}
}
