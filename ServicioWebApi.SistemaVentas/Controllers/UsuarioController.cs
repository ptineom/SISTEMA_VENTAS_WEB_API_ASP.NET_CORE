using CapaNegocio;
using Entidades;
using Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ServicioWebApi.SistemaVentas.Models.Request;
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
        private IConfiguration _configuration = null;
        private IResultadoOperacion _resultado = null;
        private BrUsuario _brUsuario = null;
        private IHttpContextAccessor _accessor = null;

        public UsuarioController(IConfiguration configuration, IResultadoOperacion resultado, IHttpContextAccessor accessor)
        {
            _configuration = configuration;
            _resultado = resultado;
            _accessor = accessor;
            _brUsuario = new BrUsuario();
        }

        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePasswordAsync([FromBody] CambiarContraseniaRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Mesagge = ModelState, Status = "Error" });

            //verifíca si la contraseña actual enviada es la correcta
            string claveEncriptado = Helper.HashHelper.GetHash256(request.ContraseniaActual);
            _resultado = await Task.Run(() => _brUsuario.ValidateUser(request.IdUsuario, claveEncriptado));

            if (!_resultado.Resultado)
                return StatusCode(StatusCodes.Status404NotFound, new { Message = "La contraseña actual ingresada es incorrecta", Status = "Error" });

            USUARIO usuario = new USUARIO();
            usuario.ACCION = "CAM";
            usuario.ID_USUARIO_REGISTRO = request.IdUsuario;
            usuario.ID_USUARIO = request.IdUsuario;
            usuario.CLAVE = Helper.HashHelper.GetHash256(request.ContraseniaNueva);

            _resultado = new ResultadoOperacion();
            _resultado = _brUsuario.ChangePassword(usuario);

            return Ok(_resultado);
        }

        [HttpPost("GenerateTokenRecoveryPassword")]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateTokenRecoveryPasswordAsync([FromBody] RecuperarContraseniaRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Mesagge = ModelState, Status = "Error" });

            var guid = Guid.NewGuid().ToString("N");
            var guidHash = Helper.HashHelper.GetHash256(guid);

            _resultado = await Task.Run(() =>
            {
                return _brUsuario.SaveTokenRecoveryPassword(new USUARIO()
                {
                    ACCION = "GTK",
                    EMAIL_USUARIO = request.Email,
                    TOKEN_RECUPERACION_PASSWORD = guidHash
                });
            });

            if (!_resultado.Resultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.Mensaje, Status = "Error" });

            //Construir la url para mediante ésta el usuario pueda generar el token de recuperación de contraseña.
            string dominio = _configuration.GetSection("AppSettings:DominioCliente").Value;
            string url = System.IO.Path.Combine(dominio, string.Format("RecuperarContrasenia/{0}", guid).Replace(@"\", "/"));

            //mensaje del correo.
            string asunto = "Recuperar contraseña - Sistema de ventas";
            string cuerpo = string.Format("<span><strong>Para recuperar tu contraseña haga <a href = {0}  target={1}> click aqui </a></strong></span>", url, "_blank");
            EmailHelper email = new EmailHelper(_configuration, request.Email, cuerpo, asunto);

            //Enviar el email
            await Task.Run(() => email.SendMail());

            return Ok(_resultado);
        }

        [HttpPost("RestorePassword")]
        [AllowAnonymous]
        public async Task<IActionResult> RestorePasswordAsync([FromBody] NuevaContraseniaRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Mesagge = ModelState, Status = "Error" });
            }

            _resultado = await Task.Run(() =>
            {
                return _brUsuario.RestorePassword(new USUARIO()
                {
                    ACCION = "RCO",
                    CLAVE = Helper.HashHelper.GetHash256(request.NuevaContrasenia),
                    TOKEN_RECUPERACION_PASSWORD = HashHelper.GetHash256(request.TokenRecuperacionPassword)
                });
            });

            if (!_resultado.Resultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.Mensaje, Status = "Error" });

            return Ok(_resultado);
        }
      
    }
}
