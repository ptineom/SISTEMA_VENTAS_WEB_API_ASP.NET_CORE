using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using SistemaVentas.WebApi.ViewModels.Seguridad;
using SistemaVentas.WebApi.ViewModels.Usuario;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SistemaVentas.WebApi.Servicios.Seguridad
{
    public class Session
    {
        private IHttpContextAccessor _accessor { get; }

        public Session(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
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

            if (_accessor.HttpContext.User != null)
            {
                bExiste = _accessor.HttpContext.User.Identity.IsAuthenticated;
            }

            return bExiste;
        }

        public UsuarioViewModel obtenerUsuarioLogueado()
        {
            UsuarioViewModel modelo = null;
            if (_accessor.HttpContext.User != null && _accessor.HttpContext.User.Identity.IsAuthenticated)
            {
                ClaimsIdentity identity = (ClaimsIdentity)_accessor.HttpContext.User.Identity;
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

    }
}
