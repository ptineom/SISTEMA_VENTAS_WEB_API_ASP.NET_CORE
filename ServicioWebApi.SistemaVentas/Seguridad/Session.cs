using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using SistemaVentas.WebApi.ViewModels.Seguridad;
using SistemaVentas.WebApi.ViewModels.Usuario;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SistemaVentas.WebApi.Seguridad
{
    public class Session
    {
        private IHttpContextAccessor _httpContext { get; }

        public Session(IHttpContextAccessor httpContext)
        {
            _httpContext = httpContext;
        }

        public static bool existUserInSessionStatic()
        {
            bool bExiste = false;
            IHttpContextAccessor httpContext = new HttpContextAccessor();

            if (httpContext.HttpContext.User != null)
            {
                bExiste = httpContext.HttpContext.User.Identity.IsAuthenticated;
            }

            return bExiste;
        }

        public bool existUserInSession()
        {
            bool bExiste = false;

            if (_httpContext.HttpContext.User != null)
            {
                bExiste = _httpContext.HttpContext.User.Identity.IsAuthenticated;
            }

            return bExiste;
        }

        public UsuarioViewModel obtenerUsuarioLogueado()
        {
            UsuarioViewModel modelo = null;
            if (existUserInSession())
            {
                ClaimsIdentity identity = (ClaimsIdentity)_httpContext.HttpContext.User.Identity;
                if (identity != null)
                {
                    var claims = identity.Claims;
                    modelo = new UsuarioViewModel
                    {
                        idUsuario = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name).Value,
                        nomUsuario = claims.FirstOrDefault(x => x.Type == "fullName").Value,
                        nomRol = claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value,
                        idSucursal = claims.FirstOrDefault(x => x.Type == "idSucursal").Value,
                        flgCtrlTotal = Convert.ToBoolean(claims.FirstOrDefault( x => x.Type == "flgCtrlTotal").Value)
                    };
                }
            }
            return modelo;
        }

        public static UsuarioViewModel obtenerUsuarioLogueadoStatic()
        {
            UsuarioViewModel modelo = null;
            IHttpContextAccessor httpContextAccesor = new HttpContextAccessor();
            var httpContextUser = httpContextAccesor.HttpContext.User;

            if (httpContextUser != null && httpContextUser.Identity.IsAuthenticated)
            {
                ClaimsIdentity identity = (ClaimsIdentity)httpContextUser.Identity;
                var claims = identity.Claims;
                modelo = new UsuarioViewModel
                {
                    idUsuario = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name).Value,
                    nomUsuario = claims.FirstOrDefault(x => x.Type == "fullName").Value,
                    nomRol = claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value,
                    idSucursal = claims.FirstOrDefault(x => x.Type == "idSucursal").Value,
                    flgCtrlTotal = Convert.ToBoolean(claims.FirstOrDefault(x => x.Type == "flgCtrlTotal").Value)
                };
            }
            return modelo;
        }

    }
}
