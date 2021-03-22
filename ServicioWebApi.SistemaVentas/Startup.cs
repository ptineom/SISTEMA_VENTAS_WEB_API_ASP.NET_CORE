using Helper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CapaNegocio;
using Microsoft.AspNetCore.Http;

namespace ServicioWebApi.SistemaVentas
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public string _myCors { get; } = "myCors";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ////Por defecto el api, retorna los atributos del json en LowercamelCase, pero configuremos para que respete los nombres de los atributos del json.
            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
            //     .AddJsonOptions(opt => opt.SerializerSettings.ContractResolver = new DefaultContractResolver());

            services.AddControllers().AddJsonOptions(opt => opt.JsonSerializerOptions.PropertyNamingPolicy = null);

            //CORS
            services.AddCors((options) =>
            {
                options.AddPolicy(_myCors, (builder) =>
                {
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                    builder.WithOrigins("*");
                });
            });

            //Autorizacion global
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            services.AddMvc(options =>
            {
                options.Filters.Add(new AuthorizeFilter(policy));
                //options.Filters.Add(new AuthorizationCustomAttribute());
            });
            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("ApiUser", pol => pol.RequireClaim(ClaimTypes.Role, "Admin"));
            //});

            //JWT
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["AppSettings:Jwt:IssuerToken"],
                    ValidAudience = Configuration["AppSettings:Jwt:AudienceToken"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(HashHelper.GetHash256(Configuration["AppSettings:Jwt:SecretKey"])))
                };
            });

            services.AddTransient<IResultadoOperacion, ResultadoOperacion>();
            //Servicio para hacer uso con inyección de dependencia el httpContext
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ServicioWebApi.SistemaVentas", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ServicioWebApi.SistemaVentas v1"));
            }

            app.UseRouting();

            app.UseAuthorization();

            //Cors
            app.UseCors(_myCors);
            //Jwt
            app.UseAuthentication();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
