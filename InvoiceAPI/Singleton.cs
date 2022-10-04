using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using System.Configuration;
using System.Xml;
using System.Xml.Xsl;
using InvoiceAPI.Models;
using System.Collections;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Data;
using InvoiceAPI.Data;
using Oracle.ManagedDataAccess.Client;
using System.IO;
using System.Text;
using System.Reflection;
using log4net.Core;
using Oracle.ManagedDataAccess.Types;
using log4net;

namespace InvoiceAPI
{
    public sealed class Singleton
    {
        //We are using volatile to ensure that
        //assignment to the instance variable finishes before it’s
        //access.
        private static volatile Singleton instance;
        private static object lockObject = new Object();
        //private readonly ILoggerManager _logger;

        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static log4net.ILog logger = log4net.LogManager.GetLogger
(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
      

        const int IndexSeguriData = 0;
        private Singleton() {
            if (logger == null)
            {
                
               // string logConfigPath = "log4net.cofig"; // this contains the path of the xml
               // System.IO.FileInfo fileInfo = new System.IO.FileInfo(logConfigPath);
                // log4net.Config.DOMConfigurator.Configure(fileInfo);
                //log4net.Config.

                string loggerName = ConfigurationManager.AppSettings.Get("log4net"); // this contains the name of the logger class

                logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);//log4net.LogManager.GetLogger(loggerName);
            }
        }
        public static Singleton Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                            instance = new Singleton();
                       
                    }
                }
                return instance;
            }
        }

        private static List<Certificados> cargaCertificados()
        {
            // .LogInformation("SINGLETON INICIA...");
            logger.Debug("-------------->");
            var certificados = new List<Certificados>();

            //<add key="dbOraSQL" value="2"/> <!-- 1 es Oracle 2 es SQL -->
            String dbString = ConfigurationManager.AppSettings["dbOraSQL"].ToString();
            String connString = string.Empty; ;

            if (dbString.Equals("1")) {
               // _logger.LogInformation("Info - Búsqueda de certificados en Oracle");
                //Busca en Oracle
                connString = ConfigurationManager.ConnectionStrings["connOra"].ConnectionString;
                //Singleton.Instance.Log.Info("Info - Datos de conección: " + connString);

                //   _logger.Info("Info - Búsqeda de certificados en BD Oracle.");
               // _logger.LogInformation("Info - Datos de conexión: " + connString);
                Debug.WriteLine("Info - Datos de conexión: " + connString);
              //  _logger.LogInformation("Info - Datos de conexión: " + connString);

                var con = new OracleConnection(connString);
                try
                {
                    con.Open();
                    OracleCommand loCmd = con.CreateCommand();
                    loCmd.CommandType = CommandType.Text;
                    loCmd.CommandText = "SELECT RFC,NUMCERTIFICADO,DBMS_LOB.substr(CERTIFICADO, 32000),DBMS_LOB.substr(LLAVE, 32000),CONTRASENA,ESTATUS,ID_DIVERZA,TOKEN_DIVERZA FROM CERTIFICADOS_API";
                      //  "SELECT RFC,NUMCERTIFICADO,CERTIFICADO,LLAVE,CONTRASENA,ESTATUS,ID_DIVERZA,TOKEN_DIVERZA FROM CERTIFICADOS_API";

                    //   var comprobanteResult = loCmd.ExecuteScalar();
                    OracleDataReader dr = loCmd.ExecuteReader();
                    // dr.Read();

                    //  SqlDataReader dr = cmd.ExecuteReader();
                    // string strData = null;
                    while (dr.HasRows)
                    {
                        Debug.WriteLine("\t{0}\t{1}", dr.GetName(0)+" "+dr.RowSize);
                        while (dr.Read())
                        {
                            //dr.GetString(0)
                            Certificados cert = new Certificados();
                            cert.rfc = dr.GetString(0);
                            cert.numCert = dr.GetString(1);
                            //cert.certificado = Encoding.UTF8.GetString(dr.GetOracleBlob(2));


                           // OracleBlob blob = dr.GetOracleBlob(2);
                           // Byte[] buffer = blob.Value;
                            //var cer = new String(Encoding.UTF8.GetChars(buffer));
                            cert.certificado = dr.GetString(2);

                            Debug.WriteLine("CERTIFICADO - > "+ dr.GetString(2));

                            //cert.llave = dr.GetString(3);

                           
                           // blob = dr.GetOracleBlob(3);
                            //buffer = blob.Value;
                            //var llave = new String(Encoding.UTF8.GetChars(buffer));
                            cert.llave = dr.GetString(3);

                            Debug.WriteLine("LLAVE - > " + dr.GetString(3));

                            cert.contrasena = dr.GetString(4);
                            cert.estatus = dr.GetInt16(5);
                            cert.id_diverza = dr.GetString(6);
                            cert.token_diverza = dr.GetString(7);
                            Debug.WriteLine(cert.numCert);
                            certificados.Add(cert);
                        }
                        dr.NextResult();
                    }
                    Debug.WriteLine("dr..... " + dr.ToString());
                    con.Close();
                   
                }
                catch (Exception ex)
                {
                  
                   /* _logger.LogError("Error - Ocurrió un error al leer los certificados de la base de datos.");
                    _logger.LogError("Error - Mensaje: " + ex.Message);
                    _logger.LogError("Error - StackTrace: " + ex.StackTrace);*/
                    Debug.WriteLine("EX ---> " + ex.Message);
                    Debug.WriteLine("EX ---> " + ex.StackTrace);

                   con.Close();

                    return certificados;
                }
            } else {
                //  Singleton.Instance.Log.Info("Info - Búsqueda de certificados en SQL");
                //Busca en SQL
                connString = ConfigurationManager.ConnectionStrings["connSQL"].ConnectionString;

               /* _logger.LogInformation("Info - Búsqeda de certificados en BD SQL.");
                _logger.LogError("Info - Datos de conexión: " + connString);*/
                Debug.WriteLine("Info - Datos de conexión: " + connString);

                var conn = new SqlConnection(connString);
                try {
                    conn.Open();
                    String sql = "SELECT [rfc],[numcertificado],[certificado],[llave],[contrasena],[estatus],[id_diverza],[token_diverza] FROM [dbo].[certificados_api] ";
                    //define the SqlCommand object
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    //execute the SQLCommand
                    SqlDataReader dr = cmd.ExecuteReader();
                    // string strData = null;
                    while (dr.HasRows)
                    {
                        Debug.WriteLine("\t{0}\t{1}", dr.GetName(0));
                        while (dr.Read())
                        {
                            Certificados cert = new Certificados();
                            cert.rfc = (string)dr[0];
                            cert.numCert = (string)dr[1];
                            cert.certificado = (string)dr[2];
                            cert.llave = (string)dr[3];
                            cert.contrasena = (string)dr[4];
                            cert.estatus = (int)dr[5];
                            cert.id_diverza = (string)dr[6];
                            cert.token_diverza = (string)dr[7];
                            Debug.WriteLine(cert.numCert);
                            certificados.Add(cert);
                        }
                        dr.NextResult();
                    }
                    Debug.WriteLine("dr..... " + dr.ToString());
                    
                        conn.Close();
                }
                catch (Exception ex)
                {
                    /*_logger.LogError("Error - Ocurrió un error al leer los certificados de la base de datos.");
                    _logger.LogError("Error - Mensaje: " + ex.Message);
                    _logger.LogError("Error - StackTrace: " + ex.StackTrace);*/
                    Debug.WriteLine("EX ---> " + ex.Message);

                    if (conn.State != ConnectionState.Closed)
                    {
                        conn.Close();
                    }

                    return certificados;
                }
            }
          return certificados;
        }

        private static List<Usuarios> cargaUsuariosSQL()
        {
            var usuarios = new List<Usuarios>();

            String dbString = ConfigurationManager.AppSettings["dbOraSQL"].ToString();
            String connString;

            if (dbString.Equals("1"))
            {
                connString = ConfigurationManager.ConnectionStrings["ConnOra"].ConnectionString;

                /*_logger.LogInformation("Info - Búsqeda de usuarios en BD Oracle.");
                _logger.LogError("Info - Datos de conexión: " + connString);*/
                Debug.WriteLine("Info - Datos de conexión: " + connString);

                //ES ORACLE
                var con = new OracleConnection(connString);
                try
                {
                    con.Open();
                    OracleCommand loCmd = con.CreateCommand();
                    loCmd.CommandType = CommandType.Text;
                    loCmd.CommandText = "SELECT USERNAME,PASSWORD,ACTIVO,PWD_EXPIRE FROM USUARIOS";
                    OracleDataReader dr = loCmd.ExecuteReader();

                    // string strData = null;
                    // string strData = null;
                    while (dr.HasRows)
                    {
                        Debug.WriteLine("\t{0}\t{1}", dr.GetName(0));

                        while (dr.Read())
                        {
                            Usuarios user = new Usuarios();
                            user.usuario = dr.GetString(0);
                            user.contrasena = dr.GetString(1);
                            user.estatus = dr.GetString(2);
                            Debug.WriteLine("q pasa ---> " + dr.GetDateTime(3));
                            //  Debug.WriteLine("q pasa ---> " + (DateTime)dr[3]);
                            //   if (dr[3] != null  ) {
                            // if (!dr.IsDBNull(3))
                            //  {
                            user.fecha_expiracion = dr.GetDateTime(3);

                            // }

                            Debug.WriteLine(user.usuario);
                            usuarios.Add(user);


                        }
                        dr.NextResult();
                    }
                    Debug.WriteLine("dr..... " + dr.ToString());

                    con.Close();           
                }
                catch (Exception ex)
                {
                    /*_logger.LogError("Error - Ocurrió un error al leer los usuarios de la base de datos.");
                    _logger.LogError("Error - Mensaje: "+ex.Message);
                    _logger.LogError("Error - StackTrace: " + ex.StackTrace);*/
                    Debug.WriteLine("Error - Ocurrió un error al leer los usuarios de la base de datos.");
                    Debug.WriteLine("Error - Mensaje: " + ex.Message);
                    Debug.WriteLine("Error - StackTrace: " + ex.StackTrace);

                    if (con.State != ConnectionState.Closed)
                    {
                        con.Close();
                    }

                    return usuarios;
                }
            }
            else
            {
                //ES SQL
                connString = ConfigurationManager.ConnectionStrings["ConnSQL"].ConnectionString;
                var conn = new SqlConnection(connString);

                /*_logger.LogInformation("Info - Búsqeda de certificados en BD SQL.");
                _logger.LogError("Info - Datos de conexión: " + connString);*/
                Debug.WriteLine("Info - Datos de conexión: " + connString);
                try
                {
                    conn.Open();

                    String sql = "SELECT [username], [password],[activo],[pwd_expire] FROM [Usuarios] ";

                    //define the SqlCommand object
                    SqlCommand cmd = new SqlCommand(sql, conn);


                    //execute the SQLCommand
                    SqlDataReader dr = cmd.ExecuteReader();


                    // string strData = null;
                    while (dr.HasRows)
                    {
                        Debug.WriteLine("\t{0}\t{1}", dr.GetName(0));

                        while (dr.Read())
                        {
                            Usuarios user = new Usuarios();
                            user.usuario = (string)dr[0];
                            user.contrasena = (string)dr[1];
                            user.estatus = (string)dr[2];
                            Debug.WriteLine("q pasa ---> " + dr[3]);
                            //  Debug.WriteLine("q pasa ---> " + (DateTime)dr[3]);
                            //   if (dr[3] != null  ) {
                            if (!dr.IsDBNull(3))
                            {
                                user.fecha_expiracion = (DateTime)dr[3];

                            }

                            Debug.WriteLine(user.usuario);
                            usuarios.Add(user);


                        }
                        dr.NextResult();
                    }
                    Debug.WriteLine("dr..... " + dr.ToString());

                   conn.Close();
                   

                }
                catch (Exception ex)
                {
                    /*_logger.LogError("Error - Ocurrió un error al leer los usuarios de la base de datos.");
                    _logger.LogError("Error - Mensaje: " + ex.Message);
                    _logger.LogError("Error - StackTrace: " + ex.StackTrace);*/
                    Debug.WriteLine("EX ---> " + ex.Message);

                    if (conn.State != ConnectionState.Closed)
                    {
                        conn.Close();
                    }

                    return usuarios;
                }
            }
          return usuarios;
        }


        private readonly ILoggerManager _logger;

        public int countDiverza { get; set; }
        public int countSeguriData { get; set; }
        public string MediaContentType { get; set; } = ConfigurationManager.AppSettings["MediaTypeContent"].ToString();
        public int ProcessCount { get; set; } = 0;
        //public string Certificado { get; } = ConfigurationManager.AppSettings["Certificado"].ToString();
        //public string KeyCertificado { get; } = ConfigurationManager.AppSettings["KeyCert"].ToString();
        //public string KeyPassword { get; } = ConfigurationManager.AppSettings["KeyPassword"].ToString();
        //public string CerificateNumber { get; } = ConfigurationManager.AppSettings["CerificateNumber"].ToString();
        public string XSLTFile { get; } = ConfigurationManager.AppSettings["XSLTFile"].ToString();
        public XmlReader XSLTSchema { get; set; }
        public XslCompiledTransform XSLCompiledTransform { get; set; }
        public bool ConConsulta { get; } = ConfigurationManager.AppSettings["ConConsulta"].ToString() == "0" ? false : true;
        public string dbString = ConfigurationManager.AppSettings["dbOraSQL"].ToString();

        //SEGURIDATA
        /*public string IdPACSeguridata { get; } = ConfigurationManager.AppSettings["IdPACSeguridata"].ToString();
        public string NombreSeguridata { get; } = ConfigurationManager.AppSettings["NombreSeguridata"].ToString();
        public string DescripcionSeguridata { get; } = ConfigurationManager.AppSettings["DescripcionSeguridata"].ToString();
        public string PrioridadSeguridata { get; } = ConfigurationManager.AppSettings["PrioridadSeguridata"].ToString();
        public int DistribucionSeguridata { get; } = Convert.ToInt32(ConfigurationManager.AppSettings["DistribucionSeguridata"].ToString());
        public string MaximoSeguridata { get; } = ConfigurationManager.AppSettings["MaximoSeguridata"].ToString();
        public decimal PorcentajeSeguridata { get; } = Convert.ToDecimal(ConfigurationManager.AppSettings["PorcentajeSeguridata"].ToString());
        public decimal PromedioSeguridata { get; } = Convert.ToDecimal(ConfigurationManager.AppSettings["PromedioSeguridata"].ToString());
        public string HttpUriSeguridata { get; } = ConfigurationManager.AppSettings["HttpUriSeguridata"].ToString();
        public string UsuarioSeguridata { get; } = ConfigurationManager.AppSettings["UsuarioSeguridata"].ToString();
        public string PasswordSeguridata { get; } = ConfigurationManager.AppSettings["PasswordSeguridata"].ToString();
        public string HttpMethodSeguridata { get; } = ConfigurationManager.AppSettings["HttpMethodSeguridata"].ToString();
        public string CredIdSeguridata { get; } = ConfigurationManager.AppSettings["CredIdSeguridata"].ToString();
        public string CredTokenSeguridata { get; } = ConfigurationManager.AppSettings["CredTokenSeguridata"].ToString();
        public string RecemsFormatSeguridata { get; } = ConfigurationManager.AppSettings["RecemsFormatSeguridata"].ToString();
        public string RecemsTemplateSeguridata { get; } = ConfigurationManager.AppSettings["RecemsTemplateSeguridata"].ToString();
        public string DocSectionSeguridata { get; } = ConfigurationManager.AppSettings["DocSectionSeguridata"].ToString();
        public string DocFormatSeguridata { get; } = ConfigurationManager.AppSettings["DocFormatSeguridata"].ToString();
        public string DocTemplateSeguridata { get; } = ConfigurationManager.AppSettings["DocTemplateSeguridata"].ToString();
        public string DocTypeSeguridata { get; } = ConfigurationManager.AppSettings["DocTypeSeguridata"].ToString();*/

        //DIVERZA
        public string IdPACDiverza { get; } = ConfigurationManager.AppSettings["IdPACDiverza"].ToString();
        public string NombreDiverza { get; } = ConfigurationManager.AppSettings["NombreDiverza"].ToString();
        public string DescripcionDiverza { get; } = ConfigurationManager.AppSettings["DescripcionDiverza"].ToString();
        public string PrioridadDiverza { get; } = ConfigurationManager.AppSettings["PrioridadDiverza"].ToString();
        public int DistribucionDiverza { get; } = Convert.ToInt32(ConfigurationManager.AppSettings["DistribucionDiverza"].ToString());
        public string MaximoDiverza { get; } = ConfigurationManager.AppSettings["MaximoDiverza"].ToString();
        public decimal PorcentajeDiverza { get; } = Convert.ToDecimal(ConfigurationManager.AppSettings["PorcentajeDiverza"].ToString());
        public decimal PromedioDiverza { get; } = Convert.ToDecimal(ConfigurationManager.AppSettings["PromedioDiverza"].ToString());
        public string HttpUriDiverza { get; } = ConfigurationManager.AppSettings["HttpUriDiverza"].ToString();
        //public string UsuarioDiverza { get; } = ConfigurationManager.AppSettings["UsuarioDiverza"].ToString();
        //public string PasswordDiverza { get; } = ConfigurationManager.AppSettings["PasswordDiverza"].ToString();
        public string HttpMethodDiverza { get; } = ConfigurationManager.AppSettings["HttpMethodDiverza"].ToString();
        //public string CredIdDiverza { get; } = ConfigurationManager.AppSettings["CredIdDiverza"].ToString();
		//public string CredTokenDiverza { get; } = ConfigurationManager.AppSettings["CredTokenDiverza"].ToString();
		public string RecemsFormatDiverza { get; } = ConfigurationManager.AppSettings["RecemsFormatDiverza"].ToString();
		public string RecemsTemplateDiverza { get; } = ConfigurationManager.AppSettings["RecemsTemplateDiverza"].ToString();
		public string DocSectionDiverza { get; } = ConfigurationManager.AppSettings["DocSectionDiverza"].ToString();
		public string DocFormatDiverza { get; } = ConfigurationManager.AppSettings["DocFormatDiverza"].ToString();
		public string DocTemplateDiverza { get; } = ConfigurationManager.AppSettings["DocTemplateDiverza"].ToString();
        public string DocTypeDiverza { get; } = ConfigurationManager.AppSettings["DocTypeDiverza"].ToString();
        public BrokerDAL broker = new BrokerDAL();
        public List<Certificados> certificadosCache = cargaCertificados();
        public List<Usuarios> usuariosCache = cargaUsuariosSQL();
        public string Reemplazo { get; } = ConfigurationManager.AppSettings["Remplazo"].ToString();
        public string LimpiaXML { get; } = ConfigurationManager.AppSettings["LimpiaXML"].ToString();
        public string GuardarDB { get; } = ConfigurationManager.AppSettings["GuardaDB"].ToString();

        public string IdPACLuna { get; } = ConfigurationManager.AppSettings["IdPACLuna"].ToString();
        public string NombreLuna { get; } = ConfigurationManager.AppSettings["NombreLuna"].ToString();
        public string DescripcionLuna { get; } = ConfigurationManager.AppSettings["DescripcionLuna"].ToString();
        public string PrioridadLuna { get; } = ConfigurationManager.AppSettings["PrioridadLuna"].ToString();
        public int DistribucionLuna { get; } = Convert.ToInt32(ConfigurationManager.AppSettings["DistribucionLuna"].ToString());
        public string MaximoLuna { get; } = ConfigurationManager.AppSettings["MaximoLuna"].ToString();
        public decimal PorcentajeLuna { get; } = Convert.ToDecimal(ConfigurationManager.AppSettings["PorcentajeLuna"].ToString());
        public decimal PromedioLuna { get; } = Convert.ToDecimal(ConfigurationManager.AppSettings["PromedioLuna"].ToString());
        public string HttpUriLuna { get; } = ConfigurationManager.AppSettings["HttpUriLuna"].ToString();
        public string PssLuna { get; } = ConfigurationManager.AppSettings["PssLuna"].ToString();
        public string TokenLuna { get; } = ConfigurationManager.AppSettings["TokenLuna"].ToString();
        public string UrlLuna { get; } = ConfigurationManager.AppSettings["UrlLuna"].ToString();

        public int PacPrinc { get; } = Convert.ToInt16(ConfigurationManager.AppSettings["PacPrincipal"].ToString());
        public int HacerFileOver{ get; } = Convert.ToInt16(ConfigurationManager.AppSettings["FileOver"].ToString());


        // protected readonly log4net.ILog _logger = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        // private static readonly ILoggerManager _logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
       // private readonly log4net.ILog _logger = log4net.LogManager.GetLogger(typeof(ILoggerManager));
       /* private readonly ILoggerManager _logger;
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger
                (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);*/


    }
}

