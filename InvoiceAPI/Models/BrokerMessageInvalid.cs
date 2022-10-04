using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvoiceAPI.Models
{
   public class MessageInvalid
   {
      public string message { get; set; }
      public string error_details { get; set; }
      public string code { get; set; }

      public MessageInvalid()
      {

      }
   }
}
