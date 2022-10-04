using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InvoiceAPI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using InvoiceAPI.Data;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections;
using InvoiceAPI.Helpers;
using System.Security.Cryptography;
using SW.Services.Stamp;
using SW.Services.Authentication;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace InvoiceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class InvoicesController : ControllerBase
    {
        private readonly InvoiceAPIContext _context;
        private String docRefId = "";
        const int IndexSeguriData = 0;
        const int IndexDiverza = 1;
        public Certificados certAct = null;
        private readonly ILoggerManager _logger;
        private int complementoDiverza = 0; //inicializado en comprobantes excepto pagos


        public InvoicesController(InvoiceAPIContext context, ILoggerManager logger)
        {
            _context = context;
            _logger = logger;
          
        }

        // GET: api/Invoices
        [HttpGet]
        public IActionResult GetInvoice()
        {
            // return new JsonResult(_context.Invoice);
            var rng = new Random();
            var result = Enumerable.Range(1, 5).Select(index => new Invoice
            {
                Id = 1,
                numCert = "10",
                xmlfile = "FILE"

            })
            .ToArray();

            return new JsonResult(_context.Invoice);
        }

        // GET: api/Invoices/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInvoice([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var invoice = await _context.Invoice.FindAsync(id);

            if (invoice == null)
            {
                return NotFound();
            }

            return Ok(invoice);
        }

        // PUT: api/Invoices/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutInvoice([FromRoute] int id, [FromBody] Invoice invoice)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != invoice.Id)
            {
                return BadRequest();
            }

            _context.Entry(invoice).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        /**
        * @param: Json {	
	    *                "xmlfile":"",
        *                "numCert":"",
        *                "user": "",
        *                "password":"",
        *                "opc1":"",
        *                "opc2":""
        *              }
        * @author: MCLFH
        * @version: 07/2022
        **/
        // POST: api/Invoices
        [HttpPost]
        public async Task<IActionResult> PostInvoice([FromBody] Invoice invoice)
        {
            /*
             * MC - Valida es estatus. 
             */
            if (!ModelState.IsValid)
            {
                _logger.LogInformation("Estado no válido.");
                return BadRequest(ModelState);
            }


            /*
             * MC - Valida que el usuario exista en el arreglo de usuarios cargados en cache.
             * Usuarios guardados en: Singleton.Instance.usuariosCache
             */
            bool acceso = false;

            _logger.LogInformation("Usuario que solicita el proceso: " + invoice.user);
            Debug.WriteLine("Usuario que solicita el proceso: " + invoice.user);

            foreach (Usuarios aUser in Singleton.Instance.usuariosCache)
            {
                if (aUser.usuario.Equals(invoice.user))
                {
                    acceso = true;
                }
            }
            if (!acceso)
            {
                var err = new Message
                {
                    code = "1100",
                    details = "Error",
                    message = "El usuario que contiene el Json no se encuentra registrado en la Base de Datos."
                };
                _logger.LogError("Error: " + "Acceso no autorizado.");
                _logger.LogError("Error - código: " + err.code);
                _logger.LogError("Error - detalle: " + err.details);
                _logger.LogError("Error - message: " + err.message);
                Debug.WriteLine("Usuario no autorizado: " + invoice.user);

            }
            /*
             * MC - Valida longitud del certificado que viene en el Json.
             */
            if (invoice.numCert.Length != 20)
            {
                var err = new Message
                {
                    code = "100",
                    details = "Error",
                    message = "El número de certificado debe tener una longitud de 20 caracteres."             
                };

                _logger.LogError("Error: " + "Número de certificado incorrecto.");
                _logger.LogError("Error - código: " + err.code);
                _logger.LogError("Error - detalle: " + err.details);
                _logger.LogError("Error - message: " + err.message);
                Debug.WriteLine("Número de certificado incorrecto: " + err.message);
                return new JsonResult(err);
            }
            /*
             * MC - Valida el RFC con una expresión regular.
             */
            String cadena = "^([A-Za-z\\x26]{3,4}([0-9]{2})(0[1-9]|1[0-2])(0[1-9]|1[0-9]|2[0-9]|3[0-1])[A-Za-z|\\d]{3})$";
            Regex rgx = new Regex(cadena);

            if (!rgx.IsMatch(invoice.opc1))
            {
                var err = new Message
                {
                    code = "100",
                    details = "Error",
                    message = "El RFC es incorrecto."
                };
                _logger.LogError("Error: " + "RFC incorrecto.");
                _logger.LogError("Error - código: " + err.code);
                _logger.LogError("Error - detalle: " + err.details);
                _logger.LogError("Error - message: " + err.message);
                Debug.WriteLine("RFC incorrecto. " + err.message);
                return new JsonResult(err);
            }

            /*	if (Singleton.Instance.dbString.Equals("connSQL"))
                {

                    var err = broker.ExisteComprobanteSQL(invoice.opc1,invoice.numCert );
                    Debug.WriteLine("El comprobante existe en la base de datos. " + err);
                    if (err.code.Equals("1000")) {
                        err.file64 = Base64Encode(err.file64);
                        return new JsonResult(err);
                    }


                }
                else
                {
                    //resultInsert = new BrokerDAL().InsertComprobanteOracle(comprobante);
                }*/
            /*
            * MC - Si el PAC es diverza y tiene complemento en el parámetro del JSON 
            * y lo guarda en la variable complementoDiverza.
            */
            Debug.WriteLine("¿El archivo es un complemento? " + invoice.opc2);
            if (!invoice.opc2.Equals(""))
            {
                complementoDiverza = Int32.Parse(invoice.opc2);
            }
            /*
            * MC - Decodifica el archivo que viene en el JSON.
            */
            string xml = Base64Decode(invoice.xmlfile);

            /*
            * MC - Valida si el config tiene habilitada la opción de limpiar el XML.
            * Si la opción está activa manda el xml al método de limpiaXML.
            */
            if (Singleton.Instance.LimpiaXML.Equals("1"))
            {
                xml = EscapeXMLValue(limpiaXML(xml));
            }

            /*
            * MC - Va al Proceso de Digestión en dónde sella el archivo.
            * Le coloca el certificado, sello y firma.
            */
            var result = ProcesoDigestion(xml, invoice.numCert);

            string xmlSellado = Base64Decode(result.message);

            bool FileOver = false;
            if (Singleton.Instance.HacerFileOver.Equals(0))
            {
               // Log.Debug("Failover : false");
                FileOver = false;
            }
            else if (Singleton.Instance.HacerFileOver.Equals(1))
            {
                //Log.Debug("Failover : true");
                FileOver = true;
            }
            else
            {
               // Log.Debug("Failover : false");
                FileOver = false;
            }
   

            Debug.WriteLine(xml);
           
            byte[] originalFile = Encoding.UTF8.GetBytes(xml);//Convert.FromBase64String(xml);

            var msg = SearchData(xml);

           // Debug.WriteLine("¿El comprobante existe en la base de datos? " + msg.details);
            if (msg.code.Equals("1000"))
            {
                //	msg.file64 = Base64Encode(msg.file64);
                /*_logger.LogAdvertencia("Warn: " + "El comprobante ya fue timbrado y se encuentra en la base de datos. Folio repetido.");
                _logger.LogAdvertencia("Warn - código: " + msg.code);
                _logger.LogAdvertencia("Warn - detalle: " + msg.details);
                _logger.LogAdvertencia("Warn - message: " + msg.message);*/
                msg.file64 = Base64Encode(msg.file64);
                return new JsonResult(msg);
            }

            if (result.hasError)
            {
                _logger.LogError("Error - código: " + result.code);
                _logger.LogError("Error - detalle: " + result.details);
                _logger.LogError("Error - message: " + result.message);
                return new JsonResult(result);
            }

            // originalFile = Convert.FromBase64String(result.message);

           // Debug.WriteLine(" RESULT: "+ (Base64Decode(result.message)));
           // Debug.WriteLine(" ------------------------------------ ");
           // Debug.WriteLine(" XML: " + xml);
            originalFile = Convert.FromBase64String(result.message);

            if (Singleton.Instance.PacPrinc == 0)
            { //DIVERZA
                _logger.LogInformation("Prepara el envío para timbrar con Diverza. ");
                result = PrepareEnvioTimbrarDiverza(docRefId, originalFile, invoice.user);

                Message messageExtra = new Message();

                result.code = " === Código Diverza === " + result.code;
                result.details = " === Detalles Diverza === " + (result.details);
                result.message = EscapeXMLValue(result.message);

               //Se timbra con Diverza y no tiene error

                if (!result.hasError)
                {
                   result.file64 = Base64Encode(result.message);
                    _logger.LogInformation("Info - Se realizó el timbrado con Diverza. ");
                    _logger.LogInformation("Info - código: " + result.code);
                    _logger.LogInformation("Info - detalle: " + result.details);
                    _logger.LogInformation("Info - message: " + result.message);
                   
                } else if (result.hasError && FileOver) {

                    //Si tiene error pero tiene fileover
                    _logger.LogInformation("Prepara el envío para timbrar con LUNA. ");
                    //Prepara el envio a Luna
                    messageExtra = EnvioTimbrarLuna(xmlSellado, invoice.user);

                    if (messageExtra.hasError)
                    {           
                        //Si luna tiene error suma los 2 mensajes de error
                        result.code += " *** Código Luna *** " + messageExtra.code;
                        result.details += " *** Detalles Luna *** " + (messageExtra.details);
                        result.message += messageExtra.message;

                        _logger.LogError("Error - No se pudo realizar el timbrado con Luna. ");
                        _logger.LogError("Error - código: " + messageExtra.code);
                        _logger.LogError("Error - detalle: " + messageExtra.details);
                        _logger.LogError("Error - message: " + messageExtra.message);

                    }
                    else
                    {
                            //Si luna timbra entonces sólo regresa la info de luna 
                        result.code = " *** Código Luna *** " + messageExtra.code;
                        result.details = " *** Detalles Luna *** " + messageExtra.details;
                        result.message = EscapeXMLValue(messageExtra.message);
                        result.file64 = messageExtra.file64;

                        _logger.LogInformation("Info - Se realizó el timbrado con Luna. ");
                        _logger.LogInformation("Info - código: " + messageExtra.code);
                        _logger.LogInformation("Info - detalle: " + messageExtra.details);
                        _logger.LogInformation("Info - message: " + messageExtra.message);

                        
                    }
                }
            }

             if (Singleton.Instance.PacPrinc == 1)
            { //LUNA
                _logger.LogInformation("Prepara el envío para timbrar con Luna. ");
                result = EnvioTimbrarLuna(xmlSellado, invoice.user);

                Message messageExtra = new Message();

                result.code = " === Código Luna === " + result.code;
                result.details = " === Detalles Luna === " + (result.details);
                result.message = result.message;            

                if (!result.hasError)
                {
                   // if (result.file64 != null)
                  //  {
                     //   result.file64 = Base64Encode(result.file64);
                        result.file64 = Base64Encode(result.message);
                   // }
                    _logger.LogInformation("Info - Se realizó el timbrado con LUNA. ");
                    _logger.LogInformation("Info - código: " + result.code);
                    _logger.LogInformation("Info - detalle: " + result.details);
                    _logger.LogInformation("Info - message: " + result.message);

                }else if (result.hasError && FileOver) {
                    _logger.LogInformation("Prepara el envío para timbrar con Diverza. ");
                    //Prepara el envio a Diverza
                    messageExtra = PrepareEnvioTimbrarDiverza(docRefId, originalFile, invoice.user);


                    if (messageExtra.hasError)
                    {
                        result.code += " *** Código Diverza *** " + messageExtra.code;
                        result.details += " *** Detalles Diverza *** " + messageExtra.details;
                        result.message +=  messageExtra.message;

                        _logger.LogError("Error - No se pudo realizar el timbrado con Diverza. ");
                        _logger.LogError("Error - código: " + messageExtra.code);
                        _logger.LogError("Error - detalle: " + messageExtra.details);
                        _logger.LogError("Error - message: " + messageExtra.message);

                    }
                    else
                    {

                        result.code = " *** Código Diverza *** " + messageExtra.code;
                        result.details = " *** Detalles Diverza *** " + messageExtra.details;
                        result.message =  messageExtra.message;
                       // result.file64 = Base64Encode(messageExtra.file64);
                        result.file64 = Base64Encode(result.message);

                        _logger.LogInformation("Info - Se realizó el timbrado con Diverza. ");
                        _logger.LogInformation("Info - código: " + messageExtra.code);
                        _logger.LogInformation("Info - detalle: " + messageExtra.details);
                        _logger.LogInformation("Info - message: " + messageExtra.message);


                    }
                }

            }



            Debug.WriteLine("DETALLE ---->" + new JsonResult(result).Value);
            Debug.WriteLine("DETALLE ---->" + new JsonResult(result).ToString());



            return new JsonResult(result);

        }

        
            private Message PrepareEnvioTimbrarDiverza(string docRefId, byte[] encodedDataAsBytes, String usuario)
        {
            try
            {
                //string contenidoOriginal = Encoding.UTF8.GetString(encodedDataAsBytes);
                //XmlDocument doc = GetEntryXmlDoc(encodedDataAsBytes);
                XmlDocument doc = GetEntryXmlDoc(encodedDataAsBytes);
                //doc = limpiaXMLCaracteres(doc);
               // doc.LoadXml(contenidoOriginal);
               // XmlDocument doc = new XmlDocument();
               // doc.LoadXml(xmlSellado);
                string contenido = JsonConvert.SerializeXmlNode(doc);
                //contenido = contenido.Replace("'", "&apos;");
                var settings = new JsonSerializerSettings();
                settings.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;
                settings.Culture = new System.Globalization.CultureInfo("es-MX");
                var conte = JsonConvert.DeserializeObject(contenido, settings);
                JObject result = JObject.Parse(contenido);
                PreparaPACDiverzaDTO Pac = new PreparaPACDiverzaDTO(IndexDiverza);
                ContenidoTimbrarDTO contenidoResult = PreparaContenido(result, Pac, docRefId);
                //contenidoResult.DocContent = Convert.ToBase64String(Encoding.UTF8.GetBytes(doc.InnerXml));
                String xmlInner = doc.InnerXml;
                xmlInner = EscapeXMLValue(xmlInner);
                contenidoResult.DocContent = Base64Encode(xmlInner);
                Message brokerMessage = new Message();

                _logger.LogInformation("Info - Lo que se enviará al timbrado con Diverza. Usuario: " + usuario + ". Contenido: " + contenidoResult);

                Parallel.Invoke(() =>
                {
                    brokerMessage = EnvioTimbrarDiverza(contenidoResult, usuario);
                });

                return brokerMessage;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error - Ocurrió un error al intentar timbrar con Diverza. ");
                _logger.LogError("Error - Mensaje: " + ex.Message);
                _logger.LogError("Error - StackTrace: " + ex.StackTrace);
                return ObtenerRespuestaInvalida("Ocurrió un error al intentar timbrar el archivo con el Pac DIVERZA.", ex.Message);
            }
        }

        public static string EscapeXMLValue(string xmlString)
        {

            if (xmlString == null)
                throw new ArgumentNullException("xmlString");

            //xmlString = xmlString.Replace("&amp;", "&");
            //xmlString = xmlString.Replace("&apos;", "'");


            //ATC
           // xmlString = xmlString.Replace("&", "&amp;");
           // xmlString = xmlString.Replace("'", "&apos;");
            

            return xmlString;
        }
        private String limpiaXML(String xmlFile)
        {

            if (Singleton.Instance.LimpiaXML.Equals("1"))
            {
                Debug.WriteLine("LIMPIA EL XML DE CARACTERES ESPECIALES.");
               
                byte[] bytes = Encoding.UTF8.GetBytes(xmlFile);

                XmlDocument xmlDoc = GetEntryXmlDoc(bytes);

                string contenido = xmlFile;

                XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
                namespaceManager.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/4");

                XmlNode node = xmlDoc.SelectSingleNode("/cfdi:Comprobante/cfdi:Receptor", namespaceManager);

                String str = node.Attributes["Nombre"].Value;
                String str_replace = Singleton.Instance.Reemplazo;
                Debug.WriteLine("Original: \"{0}\"", str);


                string[] replcace = str_replace.Split(",");

               

                foreach (var pair in replcace)
                {
                    int position = pair.IndexOf("|");
                    if (position < 0)
                        continue;
                    Debug.WriteLine("Key: {0}, Value: '{1}'",
                                   pair.Substring(0, position),
                                   pair.Substring(position + 1));
                    
                        str = str.Replace(pair.Substring(0, position), pair.Substring(position + 1));
                    

                }
                Debug.WriteLine("Modificado:      \"{0}\"", str.ToUpper());

                node.Attributes["Nombre"].Value = str.ToUpper();



                MemoryStream xmlStream = new MemoryStream();
                xmlDoc.Save(xmlStream);
                xmlStream.Flush();
                xmlStream.Position = 0;
                return xmlDoc.InnerXml;
            }
            else
            {
                Debug.WriteLine("NO LIMPIA EL XML DE CARACTERES ESPECIALES.");
                return xmlFile;
            }

        }

        private ContenidoTimbrarDTO PreparaContenido(JObject contenido, PreparaPACDiverzaDTO pac, string docRefId)
        {
            try
            {
                ContenidoTimbrarDTO contenidoTimbrar = new ContenidoTimbrarDTO(pac.IdPAC, complementoDiverza);
                contenidoTimbrar.IssRFC = contenido["cfdi:Comprobante"]["cfdi:Emisor"]["@Rfc"].ToString();
                contenidoTimbrar.DocCerNum = contenido["cfdi:Comprobante"]["@NoCertificado"].ToString();
                contenidoTimbrar.RecemsEmail = "micorreo@hotmail.com"; // De donde lo saco????
                contenidoTimbrar.DocRefId = (contenidoTimbrar.IssRFC + contenido["cfdi:Comprobante"]["@Serie"].ToString() + contenido["cfdi:Comprobante"]["@Folio"].ToString()).Replace("-", ""); //docRefId;
                contenidoTimbrar.HttpUri = pac.HttpUri;
                //contenidoTimbrar.DocContent = contenido.ToString();
                contenidoTimbrar.CredId = certAct.id_diverza;
                contenidoTimbrar.CredToken = certAct.token_diverza;


                return contenidoTimbrar;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error - Ocurrió un error al preparar el contenido. ");
                _logger.LogError("Error - Mensaje: " + ex.Message);
                _logger.LogError("Error - StackTrace: " + ex.StackTrace);
                throw ex;
            }
        }

        private Message EnvioTimbrarDiverza(ContenidoTimbrarDTO contenidoTimbrar, String usuario)
        {

            /*Debug.WriteLine("EnvioTimbrarDiverza ----->");
			Debug.WriteLine("EnvioTimbrarDiverza -----> CONTENIDO");
			Debug.WriteLine(contenidoTimbrar.ToString());*/
            _logger.LogInformation("Info - Envío a timbrar con Diverza. ");

            try
            {
                var result = new HttpResponseMessage();
                Message messageReturn = new Message();
                string messageDetail = string.Empty;
                string messagePrincipal = string.Empty;

                using (HttpClient client = new HttpClient())
                {
                    string contenido = contenidoTimbrar.ToString();
                    var request = new StringContent(contenido, System.Text.Encoding.Default, "application/json");
                    request.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");


                    _logger.LogInformation("Info - Request enviado: " + request.ToString());


                    var response = client.PostAsync(
                        new Uri(Singleton.Instance.HttpUriDiverza), request);

                    Debug.WriteLine("EnvioTimbrarDiverza -----> response");
                    Debug.WriteLine(response.ToString());

                    _logger.LogInformation("Info - Response recibido: " + response.Result);

                    result = response.Result;
                    var sResult = result.Content.ReadAsStringAsync();
                    JObject resultJson = JObject.Parse(sResult.Result.ToString());

                    Debug.WriteLine("EnvioTimbrarDiverza -----> result");
                    Debug.WriteLine(resultJson.ToString());

                    if ((int)result.StatusCode == 200 || (int)result.StatusCode == 201 || (int)result.StatusCode == 202)
                    {
                        var sResultJason = resultJson["content"].ToString();
                        byte[] bResult = Convert.FromBase64String(sResultJason);
                        messagePrincipal = ASCIIEncoding.UTF8.GetString(bResult);
                        messageDetail = result.RequestMessage.ToString();
                        messageReturn.file64 = Base64Encode(messagePrincipal);
                        _logger.LogInformation("Info - Mensaje de respuesta del timbrado Diverza: " + messagePrincipal);
                        _logger.LogInformation("Info - Detalle de respuesta del timbrado Diverza: " + messageDetail);

                    }
                    else
                    {
                        messagePrincipal = resultJson["message"].ToString() + resultJson["error_details"] == null ? string.Empty : resultJson["error_details"].ToString();
                        messageDetail = result.RequestMessage.ToString();

                        _logger.LogInformation("Info - Error de respuesta del timbrado Diverza: " + messagePrincipal);
                        _logger.LogInformation("Info - Detalle de error de respuesta del timbrado Diverza: " + messageDetail);

                    }

                    messageReturn = ObtenerMensaje(((int)result.StatusCode).ToString(), messageDetail, messagePrincipal);
                    //messageReturn.message = messagePrincipal;
                    

                    if (messageReturn.hasError == false)
                    {
                        //byte[] xmlByteArray = Encoding.Default.GetBytes(messageReturn.message);
                        var insert = new List<Message>();
                        insert.Add(messageReturn);
                        if (Singleton.Instance.GuardarDB.Equals("1"))
                        {
                            InsertData(insert, usuario);
                            _logger.LogInformation("Info - No hubo error en el timbrado y se inserta la información en la base de datos. ");
                        }
                        else {
                            _logger.LogInformation("Info - Modo de no guardado en la base de datos. ");
                            Debug.WriteLine("Info - Modo de no guardado en la base de datos. ");

                        }
                    }


                    Debug.WriteLine("EnvioTimbrarDiverza ---->" + resultJson.ToString());

                    return messageReturn;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error - Ocurrio un error al intentar timbrar el archivo con Diverza. " + ex.Message);
                _logger.LogError("Error - Ocurrio un error al intentar timbrar el archivo con Diverza. " + ex.StackTrace);
                Debug.WriteLine("Error - Ocurrio un error al intentar timbrar el archivo con Diverza. " + ex.Message);
                Debug.WriteLine("Error - Ocurrio un error al intentar timbrar el archivo con Diverza. " + ex.StackTrace);
                return ObtenerRespuestaInvalida("Ocurrio un error al intentar timbrar el archivo con el Pack DIVERZA.", ex.Message);
            }
        }

        private static void Autenticacion(string _url, string _user, string _password)
        {
            Debug.WriteLine("Autenticación");
            Authentication swAutentication = new Authentication(_url, _user, _password);
            AuthResponse authResponse = swAutentication.GetToken();
            if (authResponse.status == "success")
            {
                Debug.WriteLine("Se ha autenticado de manera correcta");
                Debug.WriteLine("Token:" + authResponse.data.token + "\n");
            }
            else
            {
                Debug.WriteLine("Error en la autenticación");
                Debug.WriteLine(authResponse.message + " : " + authResponse.messageDetail + "\n");
            }


        }

        private static StampResponseV3 LunaTimbrarV3(string xmlInfo)
        {
            Debug.WriteLine("Timbrado V3");
            string _url = "http://services.test.sw.com.mx";
            string _user = "T2lYQ0t4L0RHVkR4dHZ5Nkk1VHNEakZ3Y0J4Nk9GODZuRyt4cE1wVm5tbXB3YVZxTHdOdHAwVXY2NTdJb1hkREtXTzE3dk9pMmdMdkFDR2xFWFVPUXpTUm9mTG1ySXdZbFNja3FRa0RlYURqbzdzdlI2UUx1WGJiKzViUWY2dnZGbFloUDJ6RjhFTGF4M1BySnJ4cHF0YjUvbmRyWWpjTkVLN3ppd3RxL0dJPQ.T2lYQ0t4L0RHVkR4dHZ5Nkk1VHNEakZ3Y0J4Nk9GODZuRyt4cE1wVm5tbFlVcU92YUJTZWlHU3pER1kySnlXRTF4alNUS0ZWcUlVS0NhelhqaXdnWTRncklVSWVvZlFZMWNyUjVxYUFxMWFxcStUL1IzdGpHRTJqdS9Zakw2UGRHeDFiU3IrKzZ2cDAzbWE0WGFIMmlnTHdiNWlhVUdrL3ZHd3JNakVSSVk3ZXVqNFlLWFBpbDEvdkNaY2VxRG94djVGVUQ4UTU2Ni9CTjVMbUh0NmtBUUovWmNtM3BqRGFXNnVBNUNEOG1TaEJoVGVucmROTW9kMXJCemNKYVRtRHRlL3JDL1Ryc09PbUx4SElYdmJyNlAyTy9QVlFwL1hNY3kxc0RFMis5b3BTeXpQNnRQbldKVitaY1NrK0c1aXNCb29tcW5hOGxkUFNLdW8ydzFDYUd4eW5hRGVTWmd0TnoyL1hGSjMrWDk0K0ZCbElCeUhCVUgra2xrTlExamdCVHhZNmI1Snppc2pDM1lHUkNtU3FKQ1VQVFNDWEg5SHF5QWhvaGdLMk1SbEdtRTZpbkVmdzFJeUhjWUhUMGFiMnNPUk9Dc2xFN082TVJxZDNLU3ozNTRVdmRtU3hmaCt5L2tMZkZES2dYRC9BeXRUZ1RWRXhOdjUybHJ5OGxTTGNsbGZKUHBGZG8yTHp6eis1azczSnRnPT0.XkqLpej89YB2OelcdqcyLHKwqZpvkEeoP8yCZngxF1I";
            string _password = "T2lYQ0t4L0RHVkR4dHZ5Nkk1VHNEakZ3Y0J4Nk9GODZuRyt4cE1wVm5tbXB3YVZxTHdOdHAwVXY2NTdJb1hkREtXTzE3dk9pMmdMdkFDR2xFWFVPUXpTUm9mTG1ySXdZbFNja3FRa0RlYURqbzdzdlI2UUx1WGJiKzViUWY2dnZGbFloUDJ6RjhFTGF4M1BySnJ4cHF0YjUvbmRyWWpjTkVLN3ppd3RxL0dJPQ.T2lYQ0t4L0RHVkR4dHZ5Nkk1VHNEakZ3Y0J4Nk9GODZuRyt4cE1wVm5tbFlVcU92YUJTZWlHU3pER1kySnlXRTF4alNUS0ZWcUlVS0NhelhqaXdnWTRncklVSWVvZlFZMWNyUjVxYUFxMWFxcStUL1IzdGpHRTJqdS9Zakw2UGRHeDFiU3IrKzZ2cDAzbWE0WGFIMmlnTHdiNWlhVUdrL3ZHd3JNakVSSVk3ZXVqNFlLWFBpbDEvdkNaY2VxRG94djVGVUQ4UTU2Ni9CTjVMbUh0NmtBUUovWmNtM3BqRGFXNnVBNUNEOG1TaEJoVGVucmROTW9kMXJCemNKYVRtRHRlL3JDL1Ryc09PbUx4SElYdmJyNlAyTy9QVlFwL1hNY3kxc0RFMis5b3BTeXpQNnRQbldKVitaY1NrK0c1aXNCb29tcW5hOGxkUFNLdW8ydzFDYUd4eW5hRGVTWmd0TnoyL1hGSjMrWDk0K0ZCbElCeUhCVUgra2xrTlExamdCVHhZNmI1Snppc2pDM1lHUkNtU3FKQ1VQVFNDWEg5SHF5QWhvaGdLMk1SbEdtRTZpbkVmdzFJeUhjWUhUMGFiMnNPUk9Dc2xFN082TVJxZDNLU3ozNTRVdmRtU3hmaCt5L2tMZkZES2dYRC9BeXRUZ1RWRXhOdjUybHJ5OGxTTGNsbGZKUHBGZG8yTHp6eis1azczSnRnPT0.XkqLpej89YB2OelcdqcyLHKwqZpvkEeoP8yCZngxF1I"; 
            //Autenticacion(_url, _user, _password);


            Stamp stamp = new Stamp(_url, _user, _password);
            string xml = (xmlInfo);
           
            StampResponseV3 stampResult = stamp.TimbrarV3(xml);
           
            if (stampResult.status == "success")
            {
                Debug.WriteLine("Respuesta del Timbrado\n\n");
                Debug.WriteLine("CFDI:" + stampResult.data.cfdi + "\n");
               
            }
            else
            {
                Debug.WriteLine("Error al timbrar\n\n");
                Debug.WriteLine(stampResult.message + " : " + stampResult.messageDetail + "\n");
            }
            return stampResult;
        }

        private static StampResponseV4 LunaTimbrarV4(string xmlInfo)
        {
            Debug.WriteLine("Timbrado V4");
            
            Stamp stamp = new Stamp(Singleton.Instance.UrlLuna, Singleton.Instance.TokenLuna);// (_url, _user, _password);
            StampResponseV4 stampResult;

           // Stamp stamp = new Stamp(_url, _user, _password);
            string xml = EscapeXMLValue(xmlInfo);
             stampResult = stamp.TimbrarV4(xml);
            
            if (stampResult.status == "success")
            {
                Debug.WriteLine("Respuesta del Timbrado\n\n");
                Debug.WriteLine("CFDI:" + stampResult.data.cfdi + "\n");
                Debug.WriteLine("Cadena Original SAT:" + stampResult.data.cadenaOriginalSAT + "\n");
                Debug.WriteLine("Fecha de Timbrado:" + stampResult.data.fechaTimbrado + "\n");
                Debug.WriteLine("Número de Certificado CFDI:" + stampResult.data.noCertificadoCFDI + "\n");
                Debug.WriteLine("Número de Certificado SAT:" + stampResult.data.noCertificadoSAT + "\n");
                Debug.WriteLine("qrCode:" + stampResult.data.qrCode + "\n");
                Debug.WriteLine("Sello CFDI:" + stampResult.data.selloCFDI + "\n");
                Debug.WriteLine("Sello SAT:" + stampResult.data.selloSAT + "\n");
                Debug.WriteLine("UUID:" + stampResult.data.uuid + "\n");
            }
            else
            {
                Debug.WriteLine("Error al timbrar\n\n");
                Debug.WriteLine(stampResult.message + " : " + stampResult.messageDetail + "\n");
            }
            return stampResult;
        }
        private Message EnvioTimbrarLuna(string xml, String usuario)
        {
            try
            {
                _logger.LogInformation("Info - Envío a timbrar el documento con Luna. ");
                Message messageReturn = new Message();


            
               
                 StampResponseV4 response= LunaTimbrarV4(xml);


                string controlErr = null;
                if (response.status.Equals("Success") || response.status.Equals("success"))
                {
                    messageReturn = ObtenerMensaje(response.status, response.messageDetail, EscapeXMLValue( response.data.cfdi));
                    messageReturn.file64 = Base64Encode(response.data.cfdi);
                    //controlErr = response.data.cfdi;
                    var insert = new List<Message>();
                    insert.Add(messageReturn);
                    if (Singleton.Instance.GuardarDB.Equals("1"))
                    {
                        InsertData(insert, usuario);
                    }
                    else
                    {
                        _logger.LogInformation("Info - Modo de no guardado en la base de datos. ");
                        Debug.WriteLine("Info - Modo de no guardado en la base de datos. ");

                    }
                    //messageReturn.message = response.data.cfdi;
                   // messageReturn.file64 = Base64Encode (response.data.cfdi);
                    //brokerMessage = messageReturn;

                    _logger.LogInformation("Info - No hubo error en el timbrado y se inserta la información en la base de datos. ");
                }
                else
                {
                     messageReturn = ObtenerMensaje(response.status, response.messageDetail,response.message);
                    controlErr = "Error Mensaje :  " + response.message + " Error Detalle: " + response.messageDetail;
                }

                    
               
             // brokerMessag
                

            //    _logger.LogInformation("Info - Solicitud enviada. Request: " + stamp.ToString());

             //   Debug.WriteLine("EnvioTimbrarLuna ->" + stamp.ToString());
                Debug.WriteLine("EnvioTimbrarLuna status -> "   + response.status);

                _logger.LogInformation("Info - Respuesta recibida. Request: " + response.messageDetail);
                
               /* if (messageReturn.hasError == false)
                {
                    //byte[] xmlByteArray = Encoding.Default.GetBytes(messageReturn.message);
                    

                }
                */

                //InsertComprobante(contenidoOriginal);
                //Debug.WriteLine("EnvioTimbrarSeguridata message---->" + messageReturn.message);

                return messageReturn;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error- Ocurrio un error al intentar timbrar el archivo con LUNA. " + ex.Message);
                return ObtenerRespuestaInvalida("Ocurrio un error al intentar timbrar el archivo con el Pac SEGURIDATA.", ex.Message);
            }
        }

        private static Message ObtenerRespuestaInvalida(string resultado, string messageSeg)
        {
            Message responseInvalid = new Message();
            responseInvalid.code = "1000";
            responseInvalid.message = messageSeg;

            return responseInvalid;
        }
        
        private void InsertData(List<Message> response, String usuario)
        {
            List<ComprobantesDTO> comprobantes = new List<ComprobantesDTO>();
            try
            {
                _logger.LogInformation("Info - Inserta la información en la base de datos. ");

                Parallel.ForEach(response, resp =>
                {
                    if (!resp.hasError)
                    {
                        string xml = resp.message.Substring(resp.message.IndexOf("<"));
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(xml);
                        string contenido = JsonConvert.SerializeXmlNode(doc);
                        var settings = new JsonSerializerSettings();
                        settings.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;
                        settings.Culture = new System.Globalization.CultureInfo("es-MX");
                        var conte = JsonConvert.DeserializeObject(contenido, settings);
                        JObject result = JObject.Parse(contenido);

                        ComprobantesDTO comprobante = new ComprobantesDTO();
                        comprobante.fecha = DateTime.Parse(result["cfdi:Comprobante"]["@Fecha"].ToString());
                        //comprobante.fechaEmision = DateTime.Parse(result["cfdi:Comprobante"]["cfdi:Complemento"]["tfd:TimbreFiscalDigital"]["@FechaTimbrado"].ToString());
                        comprobante.fechaEmision = DateTime.Now;
                        comprobante.fechaTimbrado = DateTime.Parse(result["cfdi:Comprobante"]["cfdi:Complemento"]["tfd:TimbreFiscalDigital"]["@FechaTimbrado"].ToString());
                        comprobante.folio = result["cfdi:Comprobante"]["@Folio"].ToString();
                        comprobante.key_id = result["cfdi:Comprobante"]["@NoCertificado"].ToString();
                        comprobante.noSerieCert = result["cfdi:Comprobante"]["@NoCertificado"].ToString();
                        comprobante.receptor_rfc = result["cfdi:Comprobante"]["cfdi:Receptor"]["@Rfc"].ToString();
                        comprobante.rfc = result["cfdi:Comprobante"]["cfdi:Emisor"]["@Rfc"].ToString();
                        comprobante.RfcProvCertif = result["cfdi:Comprobante"]["cfdi:Complemento"]["tfd:TimbreFiscalDigital"]["@RfcProvCertif"].ToString();
                        comprobante.serie = result["cfdi:Comprobante"]["@Serie"].ToString();
                        comprobante.total = decimal.Parse(result["cfdi:Comprobante"]["@Total"].ToString());
                        comprobante.tipo_comprobante = (result["cfdi:Comprobante"]["@TipoDeComprobante"].ToString());
                        comprobante.username = usuario;
                        comprobante.uuid = result["cfdi:Comprobante"]["cfdi:Complemento"]["tfd:TimbreFiscalDigital"]["@UUID"].ToString();
                        comprobante.version = result["cfdi:Comprobante"]["@Version"].ToString();
                        comprobante.receptor_nombre = result["cfdi:Comprobante"]["cfdi:Receptor"]["@Nombre"].ToString();
                        comprobante.XML_Comprobante = Encoding.UTF8.GetBytes(xml);
                        comprobante.selloDigest = "sello";
                        //comprobantes.Add(comprobante);

                        //Dependiendo de la configuración de la base de datos inserta en Oracle o en SQL

                        string resultInsert = null;
                        if (Singleton.Instance.dbString.Equals("1"))
                        {
                            resultInsert = Singleton.Instance.broker.InsertComprobanteOracle(comprobante);
                        }
                        else
                        {
                            resultInsert = Singleton.Instance.broker.InsertComprobanteSQL(comprobante);

                        }



                        if (!resultInsert.Equals("OK"))
                        {

                            _logger.LogError("---- EXCEPTION ----");
                                _logger.LogError("Error - Ocurrió un error al insertar un registro de comprobante. Message: " + resultInsert);

                            //MessageBox.Show($"Ocurrio un error al insertar un registro de comprobante. Message: {resultInsert}");
                        }
                    }
                });

                //if (comprobantes.Count > 0)
                //    BrokerDAL.BulkInsertComprobante(comprobantes);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error - Ocurrió un error al insertar un registro de comprobante. Message: " + ex.Message);
                //MessageBox.Show($"Ocurrio un error al insertar un registro de comprobante. Message: {ex.Message}");
            }
        }

        private Message SearchData(String xml)
        {
            List<ComprobantesDTO> comprobantes = new List<ComprobantesDTO>();
            Message resultSearch = null;
            try
            {
                _logger.LogInformation("Info - Búsqueda del comprobante en la base de datos.");
                //string xml = xml.IndexOf("<"));
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                string contenido = JsonConvert.SerializeXmlNode(doc);
                var settings = new JsonSerializerSettings();
                settings.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;
                settings.Culture = new System.Globalization.CultureInfo("es-MX");
                var conte = JsonConvert.DeserializeObject(contenido, settings);
                JObject result = JObject.Parse(contenido);

                ComprobantesDTO comprobante = new ComprobantesDTO();
                comprobante.folio = result["cfdi:Comprobante"]["@Folio"].ToString();
                comprobante.key_id = result["cfdi:Comprobante"]["@NoCertificado"].ToString();
                comprobante.noSerieCert = result["cfdi:Comprobante"]["@NoCertificado"].ToString();
                comprobante.receptor_rfc = result["cfdi:Comprobante"]["cfdi:Receptor"]["@Rfc"].ToString();
                comprobante.rfc = result["cfdi:Comprobante"]["cfdi:Emisor"]["@Rfc"].ToString();
                comprobante.serie = result["cfdi:Comprobante"]["@Serie"].ToString();
                comprobante.XML_Comprobante = Encoding.UTF8.GetBytes(xml);
                comprobante.selloDigest = "sello";
                //comprobantes.Add(comprobante);

                _logger.LogInformation("Info - Datos del comprobante. Folio: " + comprobante.folio + ", Número de serie del certificado: " + comprobante.noSerieCert + ", rfc: " + comprobante.rfc + ", Serie: " + comprobante.serie);

                _logger.LogInformation("Antes de pasar valor de Oracle o SQL:  " + Singleton.Instance.dbString);

                if (Singleton.Instance.dbString.Equals("1"))
                {
                    resultSearch = Singleton.Instance.broker.ExisteComprobanteOracle(comprobante);
                    
                }
                else
                {
                    
                    resultSearch = Singleton.Instance.broker.ExisteComprobanteSQL(comprobante);
                   
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error - Ocurrió un error en la búsqueda del comprobante en la base de datos.");
                _logger.LogError("Error - Mensaje: " + ex.Message);
                _logger.LogError("Error - StackTrace: " + ex.StackTrace);

                //MessageBox.Show($"Ocurrio un error al insertar un registro de comprobante. Message: {ex.Message}");
                // poner exce

                Debug.WriteLine("Error - Ocurrió un error en la búsqueda del comprobante en la base de datos.");
                Debug.WriteLine("Error - Mensaje: " + ex.Message);
                Debug.WriteLine("Error - StackTrace: " + ex.StackTrace);
            }
            return resultSearch;
        }

        private Message ObtenerMensaje(string statusCode, string detailMessage, string messagePrincipal)
        {
            return new Message()
            {
                code = statusCode,
                details = (detailMessage),
                message = messagePrincipal
            };
        }

        public XmlDocument GetEntryXmlDoc(byte[] Bytes)
        {
            Debug.WriteLine("--------------------------------------");
            Debug.WriteLine(Encoding.Default.GetString(Bytes ));
            XmlDocument xmlDoc = new XmlDocument();
            using (MemoryStream ms = new MemoryStream(Bytes))
            {
                xmlDoc.Load(ms);
            }          
            //EscapeXMLValue(xmlDoc);
            return xmlDoc;
        }

        private XmlDocument limpiaXMLCaracteres(XmlDocument xmlDoc)
        {
            const string atributeReceptor = "/cfdi:Comprobante/cfdi:Receptor";


            XmlNamespaceManager namespaceManager = new XmlNamespaceManager(xmlDoc.NameTable);
            namespaceManager.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/4");

            XmlNode node = xmlDoc.SelectSingleNode(atributeReceptor, namespaceManager);

            String str = node.Attributes["Nombre"].Value;

            string data = str;
            Debug.WriteLine("--------------------> " + data);
           // data = data.Replace("'", "&apos;");
          //  data = data.Replace("&amp;apos;", "&apos;");
            Debug.WriteLine("--------------------> " + data);
            node.Attributes["Nombre"].Value = data.ToUpper();

            
            /*MemoryStream xmlStream = new MemoryStream();

            xmlDoc.Save(xmlStream);
            xmlStream.Flush();
            xmlStream.Position = 0;*/

            return xmlDoc;
        }

        static Message GetValuesToVerifyComprobante(JObject result)
        {
            try
            {
                ComprobantesDTO comprobante = new ComprobantesDTO();
                comprobante.folio = result["cfdi:Comprobante"]["@Folio"].ToString();
                comprobante.rfc = result["cfdi:Comprobante"]["cfdi:Emisor"]["@Rfc"].ToString();
                comprobante.serie = result["cfdi:Comprobante"]["@Serie"].ToString();

                return new BrokerDAL().ExisteComprobanteOracle(comprobante);
            }
            catch (Exception ex)
            {
                return new Message() { code = "1000", details = "Ocurrió un error al verificar el comprobante. Método  GetValuesToVerifyComprobante()", message = ex.Message };
            }
        }

        private static string GetEncodeUTF8(string stringEncode)
        {
            Encoding encodingText = Encoding.UTF8;
            byte[] encodingBytes = Encoding.UTF8.GetBytes(stringEncode);
            stringEncode = Encoding.UTF8.GetString(encodingBytes);
            return stringEncode;
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        /*    public static string hashSHA1Decryption(string value)
            {

                byte[] data = Encoding.Unicode.GetBytes(value);
                SHA1 sha = new SHA1CryptoServiceProvider();
                byte[] hash = sha.ComputeHash(data);
                DSASignatureFormatter DSAFormatter = new DSASignatureFormatter(DSA);
                DSAFormatter.SetHashAlgorithm("SHA1"); 

                signature = DSAFormatter.CreateSignature(hash);

                string digiSignature = "";
                foreach (byte b in signature)
                { digiSignature += b; }
                return stringBuilder.ToString();
            }*/

        /**
        * Método que sella el archivo y les pone el certificado, sello y firma.
        * @return devuelve un objeto de tipo Message con message,details,code,file64,hasError.
        **/
        private Message ProcesoDigestion(String xmlFile, String numCertificado)
        {
            try
            {
                _logger.LogInformation("Info - Proceso de digestión el archivo con el número de certificado: " + numCertificado);
                Debug.WriteLine("---------------- XML DE ENTRADA ----------------");
                Debug.WriteLine(xmlFile);
                
                // byte[] bytes = GetBytes(xmlFile);
                //  Debug.WriteLine(BitConverter.ToString(bytes));
                //  Debug.WriteLine(Encoding.Default.GetString(bytes))
                //XmlDocument xmlDoc = GetEntryXmlDoc(bytes);

                XmlDocument xmlDoc = new XmlDocument();

                // String xmlSCE = xmlFile;
                /*.Replace("&amp;", "&")
                                    .Replace("&quot;", "\"")
                                    .Replace("&lt;", "<")
                                    .Replace("&gt;", ">")
                                    .Replace("&apos;", "'")*/

                // Debug.WriteLine("---------------- xmlSCE ----------------");
                //  Debug.WriteLine(xmlSCE);

                xmlDoc.LoadXml(xmlFile);

                /*  
                *  REVISAR SI NECESITAMOS ESTO PORQUE DE IGUAL FORMA ESTA EL SEARCH
                *  if (Singleton.Instance.ConConsulta == true)
                 {
                     //contenido = JsonConvert.SerializeXmlNode(xmlDoc);
                     var settings = new JsonSerializerSettings();
                     settings.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;
                     //var conte = JsonConvert.DeserializeObject(contenido, settings);
                     //JObject result = JObject.Parse(contenido);
                     var conte = JsonConvert.DeserializeObject(xmlFile, settings);
                     JObject result = JObject.Parse(xmlFile);

                     Message brokerMessageVerify = GetValuesToVerifyComprobante(result);

                     return brokerMessageVerify;
                 }*/

    
                const string algoritm = "SHA256WithRSA";
                const string attributeSello = "Sello";
                const string attributeCertificado = "Certificado";
                const string atributeNoCertificado = "NoCertificado";

             /*   //XslCompiledTransform _Transformador = new XslCompiledTransform();
                AsymmetricKeyParameter decryptedKey;
                MemoryStream memoryStream = new MemoryStream();
                TextWriter textWriter = new StreamWriter(memoryStream);
                StringWriter stringWriter = new System.IO.StringWriter();
                PemWriter pemWriter = new PemWriter(stringWriter);
                ISigner signer;
                byte[] Plaintext;
                StringWriter CadenaOriginal = new StringWriter();*/

                byte[] Xslt = Convert.FromBase64String(Singleton.Instance.XSLTFile);
			    int TamañoXslT = Xslt.Length;
			    MemoryStream Mem = new MemoryStream(Xslt, 0, TamañoXslT);
			    XslCompiledTransform Trans = new XslCompiledTransform(true);
			    XsltSettings Set = new XsltSettings(true, true);
			    var Sad = new XmlUrlResolver();
			    Trans.Load(new XmlTextReader(Mem), Set, Sad);

                xmlDoc.DocumentElement.SetAttribute(atributeNoCertificado, numCertificado);
              
                Debug.WriteLine("Cache " + Singleton.Instance.certificadosCache.Count);

                certAct = new Certificados();
                certAct.numCert = numCertificado;

                foreach (Certificados aCert in Singleton.Instance.certificadosCache)
                {
                    Debug.WriteLine(numCertificado);
                    if (aCert.numCert.Equals(numCertificado) && aCert.estatus == 1)
                    {
                        certAct.certificado = aCert.certificado;
                        //certAct.contrasena = hashSHA1Decryption(aCert.contrasena);
                        //string EncryptedString = 
                        //Simple3Des des = new Simple3Des() EncryptData("12121212Qw.");
                        //string DecryptedString = DecryptText(EncryptedString);
                        certAct.contrasena = (aCert.contrasena);
                        certAct.llave = aCert.llave;
                        certAct.rfc = aCert.rfc;
                        certAct.estatus = aCert.estatus;
                        certAct.id_diverza = aCert.id_diverza;
                        certAct.token_diverza = aCert.token_diverza;
                    }
                    Debug.WriteLine(aCert.numCert);
                }

                // byte[] BytesPrivateKey = Convert.FromBase64String(GetEncodeUTF8(certAct.llave));
               // byte[] BytesPrivateKey = Convert.FromBase64String(certAct.llave);

                //decryptedKey = PrivateKeyFactory.DecryptKey(certAct.contrasena.ToCharArray(), BytesPrivateKey);

                byte[] ClavePrivada = Convert.FromBase64String(certAct.llave);
                byte[] bytesFirmados = null;
                byte[] bCadenaOriginal = null;
          
                SecureString lSecStr = new SecureString();
                SHA256Managed sham = new SHA256Managed();
                lSecStr.Clear();

                foreach (char c in certAct.contrasena.ToCharArray())
                    lSecStr.AppendChar(c);

                RSACryptoServiceProvider lrsa = JavaScience.opensslkey.DecodeEncryptedPrivateKeyInfo(ClavePrivada, lSecStr);
                //  RSACryptoServiceProvider rsa = opensslkey.DecodeEncryptedPrivateKeyInfo(porg.LlavePrivada, passwordSeguro);

                string CadenaOriginal = "";
                StringWriter sw = new StringWriter();
                XsltArgumentList XS = new XsltArgumentList();
                Trans.Transform(xmlDoc, XS, sw);
                CadenaOriginal = sw.ToString();

                bCadenaOriginal = Encoding.UTF8.GetBytes(CadenaOriginal);
                try
                {
                    bytesFirmados = lrsa.SignData(Encoding.UTF8.GetBytes(CadenaOriginal), sham);

                }
                catch (NullReferenceException)
                {
                    throw new NullReferenceException("Clave privada incorrecta, revisa que la clave que escribes corresponde a los sellos digitales cargados");
                }
                string sellodigital = Convert.ToBase64String(bytesFirmados);

                /*
                textWriter = new StreamWriter(memoryStream);
                stringWriter = new System.IO.StringWriter();
                pemWriter = new PemWriter(stringWriter);
                pemWriter.WriteObject(decryptedKey);
                stringWriter.Close();
                XsltArgumentList xsltArgs = new XsltArgumentList();
                Singleton.Instance.XSLCompiledTransform.Transform(xmlDoc.CreateNavigator(), xsltArgs, CadenaOriginal);
                signer = SignerUtilities.GetSigner(algoritm);

                //String paso = GetEncodeUTF8(CadenaOriginal.ToString()).Replace("'", "&apos;");
                String paso = CadenaOriginal.ToString();
                paso = paso.Replace("'", "&apos;")
                           .Replace("\"", "&quot;")
                           .Replace("<", "&lt;")
                           .Replace(">", "&gt;");
                           
                paso = GetEncodeUTF8(paso);
                Debug.WriteLine(CadenaOriginal.ToString());
                Plaintext = GetBytes(/*CadenaOriginal.ToString()
                paso);

                //Debug.WriteLine(System.Convert.t(Plaintext));
                Debug.WriteLine("----------->" +System.Convert.ToBase64String(Plaintext));
                // SELLAR
                signer.Init(true, decryptedKey);
                signer.BlockUpdate(Plaintext, 0, Plaintext.Length);
               // Asignar sello
                string strComprobanteSello = Convert.ToBase64String(signer.GenerateSignature());*/
                //xmlDoc.DocumentElement.SetAttribute(attributeCertificado, Singleton.Instance.Certificado);
                xmlDoc.DocumentElement.SetAttribute(attributeCertificado, certAct.certificado);
                //xmlDoc.DocumentElement.SetAttribute(attributeSello, GetEncodeUTF8(strComprobanteSello));
                xmlDoc.DocumentElement.SetAttribute(attributeSello, sellodigital);

                //docRefId = xmlDoc.DocumentElement.GetAttribute("");
                String seriefilio = xmlDoc.DocumentElement.GetAttribute("Serie") + xmlDoc.DocumentElement.GetAttribute("Folio");
                Debug.WriteLine(seriefilio);

                MemoryStream xmlStream = new MemoryStream();
                xmlDoc.Save(xmlStream);
                xmlStream.Flush();
                xmlStream.Position = 0;

                byte[] bytesXml = xmlStream.ToArray();
                _logger.LogInformation("Info - El proceso de digestion fue correcto. ");
                return new Message()
                {
                    code = "200",
                    details = "El proceso de digestión fue correcto.",
                    message = Convert.ToBase64String(bytesXml),
                    file64 = Convert.ToBase64String(bytesXml)
                };

            }
            catch (Exception ex)
            {
                _logger.LogError("Error - Ocurrio un error en el procesamiento del docuemnto. ");
                _logger.LogError("Error - Mensaje: " + ex.Message);
                _logger.LogError("Error - StackTrace: " + ex.StackTrace);

                Debug.WriteLine("ERROR " + ex.StackTrace);

                return new Message()
                {
                    code = "1001",
                    details = "Proceso de procesamiento inválido.",
                    message = ex.Message + " " + ex.StackTrace
                };

            }
        }

        private byte[] GetBytes(string xmlFile)
        {
            byte[] byt = new byte[xmlFile.Length];
            for (int i=0;i<xmlFile.Length;i++)
            {
                byt[i] = Convert.ToByte(xmlFile[i]);
            }
            return byt;
        }

        // DELETE: api/Invoices/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInvoice([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var invoice = await _context.Invoice.FindAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            _context.Invoice.Remove(invoice);
            await _context.SaveChangesAsync();

            return Ok(invoice);
        }

        private bool InvoiceExists(int id)
        {
            return _context.Invoice.Any(e => e.Id == id);
        }

        // GET api/Invoices/about
        [HttpGet("About")]
        public ContentResult About()
        {
            return Content("API para la generación de facturas de Hunkabann.");
        }

        // GET api/Invoices/version
        [HttpGet("version")]
        public string Version()
        {
            return "Version 1.0.0";
        }
    }
}