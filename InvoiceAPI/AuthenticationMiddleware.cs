using InvoiceAPI.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceAPI
{
	public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }
        public static string SHA1HashStringForUTF8String(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);

            var sha1 = SHA1.Create();

                        byte[] hashBytes = sha1.ComputeHash(bytes);

            return HexStringFromBytes(hashBytes);
        }

        public static string HexStringFromBytes(byte[] bytes)
        {
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                var hex = b.ToString("x2");
                sb.Append(hex);
            }
            return sb.ToString();
        }
        public static string HashSHA1Decryption(string value)
        {
            var shaSHA1 = System.Security.Cryptography.SHA1.Create();
            var inputEncodingBytes = Encoding.ASCII.GetBytes(value);
            var hashString = shaSHA1.ComputeHash(inputEncodingBytes);

            var stringBuilder = new StringBuilder();
            for (var x = 0; x < hashString.Length; x++)
            {
                stringBuilder.Append(hashString[x].ToString("X2"));
            }
            return stringBuilder.ToString();
        }


        public Boolean ValidaUsuario(String usuarioR, String passwordR) {

            bool acceso = false;

            // Singleton.Instance.usuariosCache

            Debug.WriteLine("NOMBRE DE USUARIO Y PSS QUE LLEGAN: " + usuarioR+" | " + passwordR);

            passwordR = SHA1HashStringForUTF8String(passwordR);

            Debug.WriteLine(" PSS SHA: "  + passwordR);


            foreach (Usuarios aUser in Singleton.Instance.usuariosCache)
            {
                Debug.WriteLine("NOMBRE DE USUARIO: "+aUser.usuario);
                Debug.WriteLine("NOMBRE DE CONTRSEÑA: " + aUser.contrasena);
                Debug.WriteLine("Decript hash: " + SHA1HashStringForUTF8String(aUser.contrasena));

                if (aUser.usuario.Equals(usuarioR) && aUser.contrasena.Equals(passwordR) && aUser.estatus.Equals("1") /*&& aUser.fecha_expiracion <= DateTime.Today*/)
                {
                    acceso = true;
                }
            }

            return acceso;

    }

    public async Task Invoke(HttpContext context)
    {
        string authHeader = context.Request.Headers["Authorization"];

            

            if (authHeader != null && authHeader.StartsWith("Basic"))
            {

             //   var x = context.Request.Headers.TryGetValue("Content-Type").First();

               Debug.WriteLine("REQUEST ----------> QUE TRAE " + context.Request.Body.ToString());

                Debug.WriteLine("REQUEST ----------> " + context.Request.Headers.ToString());
                Debug.WriteLine("REQUEST ----------> " + context.Request.Body.ToString());
                Debug.WriteLine("authHeader ----------> " + authHeader);
                //Extract credentials
                string encodedUsernamePassword = authHeader.Substring("Basic ".Length).Trim();
                Encoding encoding = Encoding.GetEncoding("iso-8859-1");
                string usernamePassword = encoding.GetString(Convert.FromBase64String(encodedUsernamePassword));

                Debug.WriteLine("USUARIOS ----------> " + Singleton.Instance.usuariosCache.Count);


                int seperatorIndex = usernamePassword.IndexOf(':');

                var username = usernamePassword.Substring(0, seperatorIndex);
                var password = usernamePassword.Substring(seperatorIndex + 1);

                var acceso = ValidaUsuario(username, password);

                if (acceso)
                {
                    await _next.Invoke(context);
                }
                else
                {
                    context.Response.ContentType = "application/json";

                    string jsonString = JsonConvert.SerializeObject(new Message
                    {
                        code = "401",
                        details = "Usuario no autorizado",
                        message = "Error"

                    });
                    context.Response.StatusCode = 401; //Unauthorizedy

                    await context.Response.WriteAsync(jsonString, Encoding.UTF8);

                    //	context.Response.StatusCode = 401; //Unauthorizedy
                    return;
                }
            }

            /*if(username == "test" && password == "test" )
            {
                await _next.Invoke(context);
            }
                else if (username == "attcfdi40" && password == "sJwlqjtjwWKMbPWRxLTR")
                {
                    await _next.Invoke(context);
                }
                else
            {
                    context.Response.ContentType = "application/json";

                    string jsonString = JsonConvert.SerializeObject(new Message
                    {
                        code = "401",
                        details = "Usuario no autorizado",
                        message = "Error"

                    });
                    context.Response.StatusCode = 401; //Unauthorizedy

                    await context.Response.WriteAsync(jsonString, Encoding.UTF8);

                    //	context.Response.StatusCode = 401; //Unauthorizedy
                return;
            }
        }*/
            else
            {
                // no authorization header
                //context.Response.StatusCode = 401; //Unauthorized
                context.Response.ContentType = "application/json";

                string jsonString = JsonConvert.SerializeObject(new Message
                {
                    code = "401",
                    details = "Usuario no autorizado",
                    message = "Error"

                });
                context.Response.StatusCode = 401; //Unauthorizedy

                await context.Response.WriteAsync(jsonString, Encoding.UTF8);
                return;
            }
    }
}
}
