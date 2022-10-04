using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using InvoiceAPI.Models;
using System.Text;
using System.Xml;
using System.IO;
using System.Xml.Xsl;
using Swashbuckle.AspNetCore.Swagger;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace InvoiceAPI
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

			services.AddDbContext<InvoiceAPIContext>(options =>
					options.UseSqlServer(Configuration.GetConnectionString("InvoiceAPIContext")));

            services.AddSingleton<ILoggerManager, LoggerManager>();

            /*services.AddAuthentication()
				.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", options => { });

			services.AddAuthorization(options =>
			{
				options.AddPolicy("BasicAuthentication", new AuthorizationPolicyBuilder("BasicAuthentication").RequireAuthenticatedUser().Build());
			});*/


            services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info
				{
					Version = "v2",
					Title = "INVOICE API"
				});
			});
		}

		
			// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
			public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseHsts();
			}

			app.UseMiddleware<AuthenticationMiddleware>();
			app.UseHttpsRedirection();
			app.UseMvc();

			//app.UseSwagger();
			/*app.UseSwaggerUI(c => {
				c.SwaggerEndpoint("/swagger/swagger.json","INVOICE API");
			});*/

			app.UseSwaggerUI(c => { c.SwaggerEndpoint("swagger/swagger.json", "API name"); });


			byte[] buffer = Convert.FromBase64String(Singleton.Instance.XSLTFile);
			int sizeReceived;
			sizeReceived = buffer.Length;
			MemoryStream memory = new MemoryStream(buffer, 0, sizeReceived);

			XslCompiledTransform xslCompiledTransform = new XslCompiledTransform();
			XsltSettings xsltSettings = new XsltSettings(true, true);
			var resolver = new XmlUrlResolver();
			xslCompiledTransform.Load(new XmlTextReader(memory), xsltSettings, resolver);
			Singleton.Instance.XSLCompiledTransform = xslCompiledTransform;

			

		}
		private static string GetEncodeUTF8(string stringEncode)
		{
			Encoding encodingText = Encoding.UTF8;
			byte[] encodingBytes = Encoding.UTF8.GetBytes(stringEncode);
			stringEncode = Encoding.UTF8.GetString(encodingBytes);
			return stringEncode;
		}



		public XmlDocument GetEntryXmlDoc(byte[] Bytes)
		{
			XmlDocument xmlDoc = new XmlDocument();
			using (MemoryStream ms = new MemoryStream(Bytes))
			{
				xmlDoc.Load(ms);
			}
			return xmlDoc;
		}
	}
}
