using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvoiceAPI.Models
{
    public class PAC
    {
        public int Id_PAC { get; set; }
        public string Tx_Descripcion { get; set; }
        public int Nu_Proceso { get; set; }
        public int Nu_Satisfactorias { get; set; }
        public int Nu_Erroneas { get; set; }
        public decimal Dc_Porcentaje { get; set; }
        public int Nu_Espera { get; set; }
        public decimal Dc_Promedio { get; set; }
        public int Nu_Distribucion { get; set; }
        public int Nu_MaxError { get; set; }
        public int Nu_Habilitado { get; set; }
        public int Nu_Peticiones { get; set; }
        public string Tx_HttpUri { get; set; }
    }
}
