using InvoiceAPI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace InvoiceAPI.Models
{
    public class ContenidoTimbrarDTO
    {
        public string User { get; set; }
        public string Password { get; set; }
        public string HttpMethod { get; set; }
        public string HttpUri { get; set; }
        public string CredId { get; set; }
        public string CredToken { get; set; }
        public string IssRFC { get; set; }
        public string RecemsEmail { get; set; }
        public string RecemsFormat { get; set; }
        public string RecemsTemplate { get; set; }
        public string DocRefId { get; set; }
        public string DocCerNum { get; set; }
        public string DocSection { get; set; }
        public string DocFormat { get; set; }
        public string DocTemplate { get; set; }
        public string DocType { get; set; }
        public string DocContent { get; set; }
        public string MediaTypeContent { get; set; }

        public ContenidoTimbrarDTO(int pacId,int compl)
        {
           // User = Singleton.Instance.UsuarioDiverza;
            //Password = Singleton.Instance.PasswordDiverza;
            HttpMethod = Singleton.Instance.HttpMethodDiverza;
           // CredId = Singleton.Instance.CredIdDiverza;
           // CredToken = Singleton.Instance.CredTokenDiverza;
            RecemsFormat = Singleton.Instance.RecemsFormatDiverza;
            RecemsTemplate = Singleton.Instance.RecemsTemplateDiverza;
            DocSection = Singleton.Instance.DocSectionDiverza;
            DocFormat = Singleton.Instance.DocFormatDiverza;
            DocTemplate = Singleton.Instance.DocTemplateDiverza;
           
            MediaTypeContent = Singleton.Instance.MediaContentType;

            if (compl == 1)
            {
                string source = Singleton.Instance.DocTypeDiverza;

                // Replace one substring with another with String.Replace.
                // Only exact matches are supported.
                DocType = source.Replace("+", "_complemento+");
            } else {
                DocType = Singleton.Instance.DocTypeDiverza;
            }
        }

        public override string ToString()
        {
            return "{\"credentials\": {\"id\":\""
                + CredId
                + "\",\"token\": \""
                + CredToken
                + "\" },\"issuer\": {\"rfc\": \""
                + IssRFC
                + "\"},\"receiver\": {\"emails\": [{\"email\": \""
                + RecemsEmail + "\",\"format\":\""
                + RecemsFormat + "\",\"template\": \""
                + RecemsTemplate + "\"}]},\"document\": {\"ref-id\": \""
                + DocRefId + "\",\"certificate-number\": \""
                + DocCerNum + "\",\"section\": \""
                + DocSection + "\",\"format\": \""
                + DocFormat + "\",\"template\": \""
                + DocTemplate + "\",\"type\": \""
                + DocType + "\",\"content\": \""
                + DocContent + "\"}}";
        }
    }
}
