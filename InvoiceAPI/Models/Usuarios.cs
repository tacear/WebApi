using System;


namespace InvoiceAPI.Models
{

    public class Usuarios
    {
        public String contrasena { get; set; }
        public String usuario { get; set; }
        public DateTime fecha_expiracion { get; set; }
        public String estatus { get; set; }

      
    }
}
