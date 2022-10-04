using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvoiceAPI.Models
{
   public class MessageOk
   {
      //public lMessageOk message_response { get; set; }
      public string code { get; set; }

      public MessageOk()
      {
        // message_response = new CancelMessageOk();
      }
   }
}
