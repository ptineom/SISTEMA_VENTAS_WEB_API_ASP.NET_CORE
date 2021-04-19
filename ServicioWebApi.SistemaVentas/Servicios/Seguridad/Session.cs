using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ServicioWebApi.SistemaVentas.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ServicioWebApi.SistemaVentas.Servicios.Seguridad
{
    public class Session
    {
        private IHttpContextAccessor _accessor;

        public Session(IHttpContextAccessor accessor)
        {
            _accessor = accessor;
        }

        public bool ExistUserInSession()
        {
            bool bExiste = false;

            if (_accessor.HttpContext.User != null)
            {
                bExiste = _accessor.HttpContext.User.Identity.IsAuthenticated;
            }

            return bExiste;
        }

        public UsuarioModel GetUserLogged()
        {
            UsuarioModel modelo = null;
            if (_accessor.HttpContext.User != null && _accessor.HttpContext.User.Identity.IsAuthenticated)
            {
                ClaimsIdentity identity = (ClaimsIdentity)_accessor.HttpContext.User.Identity;
                if (identity != null)
                {
                    var claims = identity.Claims;
                    modelo = new UsuarioModel
                    {
                        IdUsuario = claims.FirstOrDefault(x => x.Type == ClaimTypes.Name).Value,
                        NomUsuario = claims.FirstOrDefault(x => x.Type == "FullName").Value,
                        NomRol = claims.FirstOrDefault(x => x.Type == ClaimTypes.Role).Value,
                        IdSucursal = claims.FirstOrDefault(x => x.Type == "IdSucursal").Value,
                        FlgCtrlTotal = Convert.ToBoolean(claims.FirstOrDefault( x => x.Type == "FlgCtrlTotal").Value),
                        NameIdentifier = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value
                    };
                }
            }
            return modelo;
        }

    }
}
