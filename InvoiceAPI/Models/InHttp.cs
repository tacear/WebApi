using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvoiceAPI.Models
{
    public class InHttp
    {
        public string user { get; set; }
        public string password { get; set; }
        public string httpmethod { get; set; }
        public string httpuri { get; set; }

        public string credid { get; set; }
        public string credtoken { get; set; }

        public string issrfc { get; set; }

        public string recemsemail { get; set; }
        public string recemsformat { get; set; }
        public string recemstemplate { get; set; }

        public string docrefid { get; set; }
        public string doccernum { get; set; }
        public string docsection { get; set; }
        public string docformat { get; set; }
        public string doctemplate { get; set; }
        public string doctype { get; set; }
        public string doccontent { get; set; }
    }
}
