using CapaNegocio;
using Entidades;
using Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SistemaVentas.WebApi.ViewModels.Usuario;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ServicioWebApi.SistemaVentas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private IConfiguration _configuration { get; }
        private IResultadoOperacion _resultado { get; set; }
        private BrUsuario oBrUsuario = null;
        public IHttpContextAccessor _httpContext { get; set; }

        public UsuarioController(IConfiguration configuration, IResultadoOperacion resultado, IHttpContextAccessor httpContext)
        {
            _configuration = configuration;
            _resultado = resultado;
            _httpContext = httpContext;
            oBrUsuario = new BrUsuario();
        }

        [HttpPost("cambiarContraseniaAsync")]
        public async Task<IActionResult> cambiarContraseniaAsync([FromBody] CambiarContraseniaViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Mesagge = ModelState, Status = "Error" });
            }

            //verifíca si la contraseña actual enviada es la correcta
            string claveEncriptado = Helper.HashHelper.GetHash256(modelo.CONTRASENIA_ACTUAL);
            _resultado = await Task.Run(() => oBrUsuario.acceder(modelo.ID_USUARIO, claveEncriptado));
            if (!_resultado.bResultado)
            {
                _resultado.sMensaje = "La contraseña actual ingresada es incorrecta";
                return StatusCode(StatusCodes.Status404NotFound, new { Message = _resultado.sMensaje, Status = "Error" });
            }

            USUARIO usuario = new USUARIO();
            usuario.ACCION = "CAM";
            usuario.ID_USUARIO_REGISTRO = modelo.ID_USUARIO;
            usuario.ID_USUARIO = modelo.ID_USUARIO;
            usuario.CLAVE = Helper.HashHelper.GetHash256(modelo.CONTRASENIA_NUEVA);

            _resultado = new ResultadoOperacion();
            _resultado = oBrUsuario.cambiarContrasenia(usuario);

            return Ok(_resultado);
        }

        [HttpPost("generarTokenRecuperacionAsync")]
        [AllowAnonymous]
        public async Task<IActionResult> generarTokenRecuperacionAsync([FromBody] RequestRecuperarContraseniaViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Mesagge = ModelState, Status = "Error" });
            }

            var guid = Guid.NewGuid().ToString("N");
            var guidHash = Helper.HashHelper.GetHash256(guid);

            _resultado = await Task.Run(() =>
            {
                return oBrUsuario.guardarTokenRecuperacionPassword(new USUARIO()
                {
                    ACCION = "GTK",
                    EMAIL_USUARIO = modelo.Email,
                    TOKEN_RECUPERACION_PASSWORD = guidHash
                });
            });

            if (!_resultado.bResultado)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.sMensaje, Status = "Error" });
            }

            //Construir la url para mediante ésta el usuario pueda generar el token de recuperación de contraseña.
            string dominio = _configuration.GetSection("DominioCliente").Value;
            string url = System.IO.Path.Combine(dominio, string.Format("RecuperarContrasenia/{0}", guid).Replace(@"\", "/"));

            //mensaje del correo.
            string asunto = "Recuperar contraseña - Sistema de ventas";
            string cuerpo = string.Format("<span><strong>Para recuperar tu contraseña haga <a href = {0}  target={1}> click aqui </a></strong></span>", url, "_blank");
            EmailHelper email = new EmailHelper(modelo.Email, cuerpo, asunto);

            //Enviar el email
            await Task.Run(() => email.sendMail());

            return Ok(_resultado);
        }

        [HttpPost("restablecerContraseniaAsync")]
        [AllowAnonymous]
        public async Task<IActionResult> restablecerContraseniaAsync([FromBody] RequestNuevaContraseniaViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Mesagge = ModelState, Status = "Error" });
            }

            _resultado = await Task.Run(() =>
            {
                return oBrUsuario.recuperarContrasenia(new USUARIO()
                {
                    ACCION = "RCO",
                    CLAVE = Helper.HashHelper.GetHash256(modelo.NuevaContrasenia),
                    TOKEN_RECUPERACION_PASSWORD = HashHelper.GetHash256(modelo.TokenRecuperacionPassword)
                });
            });

            if (!_resultado.bResultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.sMensaje, Status = "Error" });

            return Ok(_resultado);
        }

        [HttpGet("getUsuario/{idUsuario?}/{rol?}")]
        //public async Task<IActionResult> getUsuario([FromQuery] viewUser modelo)
        public async Task<IActionResult> getUsuario(string idUsuario, string rol)
        {
            var jj = ipAddress();
            USUARIO usuario = await Task.Run(() =>
            {
                return new USUARIO()
                {
                    ID_USUARIO = "SISTEMAS",
                    NOM_USUARIO = "HECTOR TORO VALENCIA",
                    NOM_ROL = "ADMINISTRADOR"
                };
            });
            _resultado.SetResultado(true, usuario);
            return Ok(_resultado);
        }

        private string ipAddress()
        {
            var ww = HttpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
            var gg = Request.Headers.ContainsKey("X-Forwarded-For");
            var hh = Request.Headers["X-Forwarded-For"];
            var jj = HttpContext.Connection.RemoteIpAddress;
            var ff = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            var ff2 = HttpContext.Connection.RemoteIpAddress.MapToIPv6().ToString();
            var ss = Dns.GetHostName();
            var kk = Dns.GetHostAddresses(ss);
            var ll = Dns.GetHostByName(ss);
            var aa = Dns.GetHostEntry(ss);

            var hh5 = HttpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault();
            string header = (HttpContext.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ?? HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault());
            if (IPAddress.TryParse(header, out IPAddress ip))
            {
                var yy = ip;
            }

            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}
