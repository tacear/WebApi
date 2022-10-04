using InvoiceAPI.Models;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;

namespace InvoiceAPI.Data
{
	public class BrokerDAL
	{
		string connString = string.Empty;

		string dbString = string.Empty;
		
        OracleConnection conn = null;
        //private readonly ILoggerManager _logger;

        public BrokerDAL()
		{
            /* < add key = "dbOraSQL" value = "2" /> < !--1 es Oracle 2 es SQL -->*/

            dbString = ConfigurationManager.AppSettings["dbOraSQL"].ToString();

            if (dbString.Equals("1"))
            { //Oracle
                connString = ConfigurationManager.ConnectionStrings["connOra"].ConnectionString;
                conn = new OracleConnection(connString);
            }
            else {
               
                connString = ConfigurationManager.ConnectionStrings["connSQL"].ConnectionString;
               

            }
       }

		

		/*public string InsertComprobanteLocal(Invoice comprobante)
		{
			try
			{

                con.Open();
				Debug.WriteLine("Connecting.....");
				Debug.WriteLine("Con..... " + connString);	

				var cmd = new SqlCommand("INSERT INTO [dbo].[invoices_api] ([xmlfile],[numCert],[username],[password],[rfc],[opc]" +
                                        ") VALUES ( + '"+comprobante.xmlfile+"','" + comprobante.numCert + "','" + comprobante.user + "','" +
                                        comprobante.password + "','" + comprobante.opc1 + "','" + comprobante.opc2 + "');", con);

				//cmd.Parameters.AddWithValue("@comprobante", comprobante.xmlfile);

				Debug.WriteLine("------------------QUERY------------------");
				Debug.WriteLine("INSERT INTO  [dbo].[invoices_api] ([xmlfile],[numCert],[username],[password],[rfc],[opc]" +
										") VALUES (" + "'@comprobante','" + comprobante.numCert + "','" + comprobante.user + "','" +
										comprobante.password + "','" + comprobante.opc1 + "','" + comprobante.opc2 + "');");
				Debug.WriteLine("-----------------------------------------");


				int row = cmd.ExecuteNonQuery();
                Debug.WriteLine("-----------------------------------------ROW "+ row);
                // cmd.ExecuteNonQuery();
                con.Close();
                return "OK";
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Ex..... " + ex.Message);
                Debug.WriteLine("Ex..... " + ex.Data);
                Debug.WriteLine("Ex..... " + ex.HResult);
                Debug.WriteLine("EX.... " + ex.StackTrace);

                return ex.Message;
			}
		}
        */
        public string InsertComprobanteSQL(ComprobantesDTO comprobante) {
            SqlConnection connection = new SqlConnection(connString);
            using (connection)
            {
                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    using (var command = new SqlCommand())
                    {
                        string format = "yyyy-MM-dd HH:mm:ss";
                        command.Connection = connection;
                        command.Transaction = transaction;
                        command.CommandType = CommandType.Text;
                        command.CommandText = "INSERT INTO[dbo].[CE_comprobantes] ([UUID],[username],[key_id],[version],[rfc],[serie]" +
										",[folio],[fecha],[selloDigest],[noSerieCert],[fechaEmision],[receptor_rfc],[total],[fechaTimbrado],[XML_Comprobante]" +
										",[RfcProvCertif]) VALUES ('" + comprobante.uuid + "','" + comprobante.username + "','" + comprobante.key_id + "','" +
										comprobante.version + "','" + comprobante.rfc + "','" + comprobante.serie + "','" + comprobante.folio + "', '" +
										comprobante.fecha.ToString(format) + "','" + comprobante.selloDigest + "','" +
										comprobante.noSerieCert + "','" +
										comprobante.fechaEmision.ToString(format) + "','" + comprobante.receptor_rfc + "'," + comprobante.total + ",'" +
										comprobante.fechaTimbrado.ToString(format) + "',@comprobante,'" + comprobante.RfcProvCertif + "');";
                       // command.Parameters.Add("@comprobante", SqlDbType.NVarChar, 50);
                        command.Parameters.AddWithValue("@comprobante", comprobante.XML_Comprobante);

                        try
                        {
                            /* foreach (var item in entities)
                             {
                                 command.Parameters["@desc"].Value = item.description;
                                 command.Parameters["@units"].Value = item.units;
                                 command.Parameters["@amount"].Value = item.amount;
                                 command.ExecuteNonQuery();
                             }*/
                            command.ExecuteNonQuery();
                            transaction.Commit();
                            connection.Close();
                            return "OK";
                        }
                        catch (Exception ex)
                        {
                            //transaction.Rollback();
                            connection.Close();
                            return ex.Message;
                        }
                    }
                }
            }
        }


		public string InsertComprobante(ComprobantesDTO comprobante)
		{
            SqlConnection con = new SqlConnection(connString);
            try
			{
               
                string format = "yyyy-MM-dd HH:mm:ss";
                Debug.WriteLine("Contenctando a la base de datos.....");
                Debug.WriteLine("String de conexión: " + connString);
                
                con.Open();

               var time = con;
			

				var cmd = new SqlCommand("INSERT INTO [dbo].[CE_comprobantes] ([UUID],[username],[key_id],[version],[rfc],[serie]" +
										",[folio],[fecha],[selloDigest],[noSerieCert],[fechaEmision],[receptor_rfc],[total],[fechaTimbrado],[XML_Comprobante]" +
										",[RfcProvCertif]) VALUES ('" + comprobante.uuid + "','" + comprobante.username + "','" + comprobante.key_id + "','" +
										comprobante.version + "','" + comprobante.rfc + "','" + comprobante.serie + "','" + comprobante.folio + "', '" +
										comprobante.fecha.ToString(format) + "','" + comprobante.selloDigest + "','" +
										comprobante.noSerieCert + "','" +
										comprobante.fechaEmision.ToString(format) + "','" + comprobante.receptor_rfc + "'," + comprobante.total + ",'" +
										comprobante.fechaTimbrado.ToString(format) + "',@comprobante,'" + comprobante.RfcProvCertif + "');", con);

				cmd.Parameters.AddWithValue("@comprobante", comprobante.XML_Comprobante);

				Debug.WriteLine("------------------QUERY------------------");
				Debug.WriteLine("INSERT INTO [dbo].[CE_comprobantes] ([UUID],[username],[key_id],[version],[rfc],[serie]" +
										",[folio],[fecha],[selloDigest],[noSerieCert],[fechaEmision],[receptor_rfc],[total],[fechaTimbrado],[XML_Comprobante]" +
										",[RfcProvCertif]) VALUES ('" + comprobante.uuid + "','" + comprobante.username + "','" + comprobante.key_id + "','" +
										comprobante.version + "','" + comprobante.rfc + "','" + comprobante.serie + "','" + comprobante.folio + "', '" +
										comprobante.fecha.ToString(format) + "','" + comprobante.selloDigest + "','" +
										comprobante.noSerieCert + "','" +
										comprobante.fechaEmision.ToString(format) + "','" + comprobante.receptor_rfc + "'," + comprobante.total + ",'" +
										comprobante.fechaTimbrado.ToString(format) + "',@comprobante,'" + comprobante.RfcProvCertif + "');");
				Debug.WriteLine("-----------------------------------------");
               

                int row = cmd.ExecuteNonQuery();
                con.Close();

				return "OK";
			}
			catch (Exception ex)
			{
                con.Close();
                Debug.WriteLine("Ex..... " + ex.Message);
                Debug.WriteLine("Ex..... " + ex.StackTrace);

               /* _logger.LogError("Error - Ocurrió un error en el método de insertar comprobante. ");
                _logger.LogError("Error - Mensaje: " + ex.Message);
                _logger.LogError("Error - StackTrace: " + ex.StackTrace);*/


                return ex.Message;
			}
		}

		public Message ExisteComprobanteSQL(ComprobantesDTO comprobante)
		{
          Message message = new Message();
			Debug.WriteLine("Contenctando a la base de datos.....");
			Debug.WriteLine("String de conexión: " + connString);
            SqlConnection con = new SqlConnection(connString);
            try
			{
                
                con.Open();

                String sql = "SELECT [XML_Comprobante] FROM [dbo].[CE_comprobantes] where  RFC = '" + comprobante.rfc + "' AND SERIE = '" + comprobante.serie + "' AND FOLIO = '" + comprobante.folio + "'";
                
                //define the SqlCommand object
                SqlCommand cmd = new SqlCommand(sql, con);

				//execute the SQLCommand
				SqlDataReader dr = cmd.ExecuteReader();

				byte[] byteData = null;
				string strData = null;
				while (dr.HasRows)
				{
					Debug.WriteLine("OBTIENE XML: ", dr.GetName(0));
                    while (dr.Read())
					{
                        byteData = (byte[])dr[0];
						strData = Encoding.UTF8.GetString(byteData);
						Debug.WriteLine("CONTENIDO XML: ",strData);
                  
					}
					dr.NextResult();
				}


                con.Close();
                if (strData == null)
                {
                    con.Close();
                    return new Message() { code = "200", details = "No existe el comprobante en la BD.", message = "No existe el comprobante en la BD." };
                }
                else
                {
                    con.Close();
                    return new Message() { code = "1000", details = "El comprobante ya fué timbrado.", message = "Ya se encuentra registrado el comprobante en la BD.", file64 = strData };
                }
                
            }
			catch (Exception ex)
			{
		

               con.Close();

                Debug.WriteLine("Error - Mensaje: " + ex.Message);
                Debug.WriteLine("Error - StackTrace: " + ex.StackTrace);

               /* _logger.LogError("Error - Ocurrió un error en el método de buscar comprobante existente en SQL.");
                _logger.LogError("Error - Mensaje: " + ex.Message);
                _logger.LogError("Error - StackTrace: " + ex.StackTrace);*/

                return new Message() { code = "1010", details = "Ocurrió un error al verificar el comprobante en la BD.", message = ex.Message };
			}
		}

public string InsertComprobanteOracle(ComprobantesDTO comprobante)
		{
            OracleConnection conn = new OracleConnection(connString);
            Debug.WriteLine("Contenctando a la base de datos.....");
            Debug.WriteLine("String de conexión: " + connString);

            using (conn)
            {
                conn.Open();

                using (OracleTransaction transaction = conn.BeginTransaction())
                {
                    using (var command = new OracleCommand())
                    {
                        string format = "yyyy-MM-dd HH:mm:ss";
                        command.Connection = conn;
                        command.Transaction = transaction;
                        command.CommandType = CommandType.Text;
                        command.CommandText = "select CE_COMPROBANTESSEQUENCE.nextval from dual";
                        long comprobanteId = Convert.ToInt64(command.ExecuteScalar());

                        string query = "INSERT INTO CE_COMPROBANTES(COMPROBANTE_ID, UUID, USERNAME, KEY_ID," +
                        "VERSION, RFC, SERIE, FOLIO, NOAPROBACION, " +
                        "FECHA, SELLODIGEST, NOSERIECERT, FECHAEMISION, RECEPTOR_RFC, RECEPTOR_NOMBRE, TIPO_COMPROBANTE, TOTAL, TOTALIMPUESTOSTRASLADADOS, " +
                        "INFADUANERA, CURRENCY_ID, DATEEXCHANGERATE, EXCHANGERATE, CANCELADO, FECHA_CANCELACION, FECHATIMBRADO, XML_COMPROBANTE)VALUES(" +
                        comprobanteId.ToString() + ",'" + comprobante.uuid + "','" + comprobante.username + "','" + comprobante.key_id + "','" +
                                        comprobante.version + "','" + comprobante.rfc + "','" + comprobante.serie + "','" + comprobante.folio + "',0,SYSDATE,'" + comprobante.RfcProvCertif + "','" +
                                        comprobante.noSerieCert + "',TO_DATE('" + comprobante.fechaEmision.ToString(format) + "','YYYY-MM-DD HH24:MI:SS'),'" + comprobante.receptor_rfc + "','" + comprobante.receptor_nombre.Replace("'","&apos;") + "','" +
                                        comprobante.tipo_comprobante + "'," + comprobante.total.ToString() + "," + comprobante.total_impuestos_t.ToString() + ",'', 1, NULL, NULL, '', NULL,TO_DATE('" +
                                        comprobante.fechaTimbrado.ToString(format) + "','YYYY-MM-DD HH24:MI:SS'),:XML_COMPROBANTE)";

                        Debug.WriteLine("-- Insert Oracle -- "+query);

                        var cmd = new OracleCommand(query, conn);

                        cmd.Parameters.Add("XML_COMPROBANTE", OracleDbType.Blob,comprobante.XML_Comprobante, ParameterDirection.Input);

                        

                        try
                        {
                          
                            cmd.ExecuteNonQuery();
                            transaction.Commit();
                            conn.Close();
                            return "OK";
                        }
                        catch (Exception ex)
                        {
                            //transaction.Rollback();
                            conn.Close();
                           /* _logger.LogError("Error - Ocurrió un error en la inserción del comprobante en la base de datos.");
                            _logger.LogError("Error - Mensaje: " + ex.Message);
                            _logger.LogError("Error - StackTrace: " + ex.StackTrace);*/

                            return ex.Message;
                        }
                    }
                }
            }


                      /*  try
                {
				string format = "yyyy-MM-dd HH:mm:ss";
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                conn.Open();

				OracleCommand loCmd = conn.CreateCommand();
				loCmd.CommandType = CommandType.Text;
				loCmd.CommandText = "select CE_COMPROBANTESSEQUENCE.nextval from dual";
				long comprobanteId = Convert.ToInt64(loCmd.ExecuteScalar());

				//string queryInsert = "INSERT INTO SGINVOICE.CE_COMPROBANTES(COMPROBANTE_ID, UUID, USERNAME, KEY_ID, VERSION, RFC, SERIE, FOLIO, NOAPROBACION, " +
				//    "FECHA, SELLODIGEST, NOSERIECERT, FECHAEMISION, RECEPTOR_RFC, RECEPTOR_NOMBRE, TIPO_COMPROBANTE, TOTAL, TOTALIMPUESTOSTRASLADADOS, " +
				//    "INFADUANERA, CURRENCY_ID, DATEEXCHANGERATE, EXCHANGERATE, CANCELADO, FECHA_CANCELACION, FECHATIMBRADO, XML_COMPROBANTE)VALUES(" +
				//    comprobanteId.ToString() + ",'" + comprobante.uuid + "','" + comprobante.username + "','" + comprobante.key_id + "','" +
				//                        comprobante.version + "','" + comprobante.rfc + "','" + comprobante.serie + "','" + comprobante.folio + "',0,SYSDATE,'" + comprobante.RfcProvCertif + "','" +
				//                        comprobante.noSerieCert + "',TO_DATE('" + comprobante.fechaEmision.ToString(format) + "','YYYY-MM-DD HH24:MI:SS'),'" + comprobante.receptor_rfc + "','" + comprobante.receptor_nombre + "','" +
				//                        comprobante.tipo_comprobante + "'," + comprobante.total.ToString() + "," + comprobante.total_impuestos_t.ToString() + ",'', 1, NULL, NULL, '', NULL,TO_DATE('" +
				//                        comprobante.fechaTimbrado.ToString(format) + "','YYYY-MM-DD HH24:MI:SS'),:comprobante);";



				var cmd = new OracleCommand("INSERT INTO CE_COMPROBANTES(COMPROBANTE_ID, UUID, USERNAME, KEY_ID, V" +
                    "ERSION, RFC, SERIE, FOLIO, NOAPROBACION, " +
					"FECHA, SELLODIGEST, NOSERIECERT, FECHAEMISION, RECEPTOR_RFC, RECEPTOR_NOMBRE, TIPO_COMPROBANTE, TOTAL, TOTALIMPUESTOSTRASLADADOS, " +
					"INFADUANERA, CURRENCY_ID, DATEEXCHANGERATE, EXCHANGERATE, CANCELADO, FECHA_CANCELACION, FECHATIMBRADO, XML_COMPROBANTE)VALUES(" +
					comprobanteId.ToString() + ",'" + comprobante.uuid + "','" + comprobante.username + "','" + comprobante.key_id + "','" +
										comprobante.version + "','" + comprobante.rfc + "','" + comprobante.serie + "','" + comprobante.folio + "',0,SYSDATE,'" + comprobante.RfcProvCertif + "','" +
										comprobante.noSerieCert + "',TO_DATE('" + comprobante.fechaEmision.ToString(format) + "','YYYY-MM-DD HH24:MI:SS'),'" + comprobante.receptor_rfc + "','" + comprobante.receptor_nombre + "','" +
										comprobante.tipo_comprobante + "'," + comprobante.total.ToString() + "," + comprobante.total_impuestos_t.ToString() + ",'', 1, NULL, NULL, '', NULL,TO_DATE('" +
										comprobante.fechaTimbrado.ToString(format) + "','YYYY-MM-DD HH24:MI:SS'),:XML_COMPROBANTE)", conn);

				cmd.Parameters.Add("XML_COMPROBANTE", OracleDbType.Blob,
							 comprobante.XML_Comprobante, ParameterDirection.Input);

				Debug.WriteLine("------------------QUERY------------------");
				Debug.WriteLine("INSERT INTO CE_COMPROBANTES(COMPROBANTE_ID, UUID, USERNAME, KEY_ID, V" +
                    "ERSION, RFC, SERIE, FOLIO, NOAPROBACION, " +
                    "FECHA, SELLODIGEST, NOSERIECERT, FECHAEMISION, RECEPTOR_RFC, RECEPTOR_NOMBRE, TIPO_COMPROBANTE, TOTAL, TOTALIMPUESTOSTRASLADADOS, " +
                    "INFADUANERA, CURRENCY_ID, DATEEXCHANGERATE, EXCHANGERATE, CANCELADO, FECHA_CANCELACION, FECHATIMBRADO, XML_COMPROBANTE)VALUES(" +
                    comprobanteId.ToString() + ",'" + comprobante.uuid + "','" + comprobante.username + "','" + comprobante.key_id + "','" +
                                        comprobante.version + "','" + comprobante.rfc + "','" + comprobante.serie + "','" + comprobante.folio + "',0,SYSDATE,'" + comprobante.RfcProvCertif + "','" +
                                        comprobante.noSerieCert + "',TO_DATE('" + comprobante.fechaEmision.ToString(format) + "','YYYY-MM-DD HH24:MI:SS'),'" + comprobante.receptor_rfc + "','" + comprobante.receptor_nombre + "','" +
                                        comprobante.tipo_comprobante + "'," + comprobante.total.ToString() + "," + comprobante.total_impuestos_t.ToString() + ",'', 1, NULL, NULL, '', NULL,TO_DATE('" +
                                        comprobante.fechaTimbrado.ToString(format) + "','YYYY-MM-DD HH24:MI:SS'),:XML_COMPROBANTE)");
				Debug.WriteLine("-----------------------------------------");

				int row = cmd.ExecuteNonQuery();

				conn.Close();

				return "OK";
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Ex..... " + ex.Message);
                Debug.WriteLine("EX....  "+ ex.StackTrace);

                _logger.LogError("Error - Ocurrió un error en el método de insertar comprobante Oracle.");
                _logger.LogError("Error - Mensaje: " + ex.Message);
                _logger.LogError("Error - StackTrace: " + ex.StackTrace);



				conn.Close();

				return ex.Message;
			}*/
		}

		public Message ExisteComprobanteOracle(ComprobantesDTO comprobante)
		{
			Message message = new Message();
            //var con = new OracleConnection(connString);

            try
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
                conn.Open();

                OracleCommand loCmd = conn.CreateCommand();
                loCmd.CommandType = CommandType.Text;
                //loCmd.CommandText = "SELECT /*+ FIRST_ROWS(1) */ FOLIO FROM  SGINVOICE.CE_COMPROBANTES WHERE RFC = '" + comprobante.rfc + "' AND SERIE = '" + comprobante.serie + "' AND FOLIO = '" + comprobante.folio + "'";
                //XML_COMPROBANTE

                //var comprobanteResult = loCmd.ExecuteScalar();
                //String sql = "SELECT [XML_Comprobante] FROM [dbo].[CE_comprobantes] where  RFC = '" + comprobante.rfc + "' AND SERIE = '" + comprobante.serie + "' AND FOLIO = '" + comprobante.folio + "'";


                loCmd.CommandText = "SELECT XML_COMPROBANTE FROM  CE_COMPROBANTES WHERE RFC = '" + comprobante.rfc + "' AND SERIE = '" + comprobante.serie + "' AND FOLIO = '" + comprobante.folio + "'";

                Debug.WriteLine("SELECT XML_COMPROBANTE FROM  CE_COMPROBANTES WHERE RFC = '" + comprobante.rfc + "' AND SERIE = '" + comprobante.serie + "' AND FOLIO = '" + comprobante.folio + "'");
                //define the SqlCommand object
                OracleDataReader dr = loCmd.ExecuteReader();


                 byte[] byteData = null;
                 string strData = null;

                 Debug.WriteLine("dr.RowSize " + dr.RowSize);
                 Debug.WriteLine("dr.HasRows " + dr.HasRows);



                 while (dr.HasRows)
                  {
                      Debug.WriteLine("\t{0}\t{1}", dr.GetName(0));

                      while (dr.Read())
                      {

                          byteData = (byte[])dr[0];
                          strData = System.Text.Encoding.UTF8.GetString(byteData, 0, byteData.Length);//dr.GetString(0) + dr.GetString(0);
                        Debug.WriteLine(strData);


                      }
                      dr.NextResult();
                  }

               /* OracleBlob b = dr.GetOracleBlob(0);
                var sr = new System.IO.StreamReader(b);
                var content = sr.ReadToEnd();*/

                // Debug.WriteLine("dr..... " + dr.ToString());


                if (strData == null)
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                    return new Message() { code = "200", details = "No existe el comprobante en la BD.", message = "No existe el comprobante en la BD." };
                }
                else
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                    //return new Message() { code = "1000", details = "El comprobante ya fué timbrado.", message = "Folio duplicado. RFC: " + comprobante.rfc + ". SERIE: " + comprobante.serie + ". FOLIO: " + comprobante.folio + "." }; ---
                    return new Message() { code = "1000", details = "El comprobante ya fué timbrado.", message = "Ya se encuentra registrado el comprobante en la BD.", file64 = strData };

                }

            }
            catch (Exception ex)
			{

                Debug.WriteLine("EX----- "+ex.Message);
                Debug.WriteLine("EX----- " + ex.StackTrace);

              // _logger.LogError("Error - Ocurrió un error en el método de búsqueda de comprobante. ");
              //  _logger.LogError("Error - Mensaje: " + ex.Message);
              //  _logger.LogError("Error - StackTrace: " + ex.StackTrace);

                conn.Close();

				return new Message() { code = "1000", details = "Ocurrió un error al verificar el comprobante en la BD.", message = ex.Message };
			}
		}
	}
}
