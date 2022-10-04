using InvoiceAPI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace InvoiceAPI.Models
{
    public class PreparaPACDiverzaDTO
    {
        public int IdPAC { get; set; }
        public string Descripcion { get; set; }
        public int Proceso { get; set; }
        public int Satisfactorias { get; set; }
        public int Erroneas { get; set; }
        public decimal Porcentaje { get; set; }
        public int Espera { get; set; }
        public decimal Promedio { get; set; }
        public int Distribucion { get; set; }
        public int MaxError { get; set; }
        public int Habilitado { get; set; }
        public int Peticiones { get; set; }
        public string HttpUri { get; set; }

        public PreparaPACDiverzaDTO(int indexPacId)
        {
            IdPAC = indexPacId;
            Descripcion = Singleton.Instance.DescripcionDiverza;
            Distribucion = Singleton.Instance.DistribucionDiverza;
            Porcentaje = Singleton.Instance.PorcentajeDiverza;
            Promedio = Singleton.Instance.PromedioDiverza;
            Peticiones = 0;
            HttpUri = Singleton.Instance.HttpUriDiverza;
        }
    }
}
