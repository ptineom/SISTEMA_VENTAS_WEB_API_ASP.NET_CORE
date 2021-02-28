using CapaNegocio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SistemaVentas.WebApi.Servicios.Seguridad;
using SistemaVentas.WebApi.ViewModels;
using SistemaVentas.WebApi.ViewModels.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioWebApi.SistemaVentas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SucursalUsuarioController : ControllerBase
    {
        private IConfiguration _configuration { get; }
        public IHttpContextAccessor _accessor { get; set; }
        private IResultadoOperacion _resultado { get; set; }
        private BrSucursalUsuario oBrSucursalUsuario = null;

        public SucursalUsuarioController(IResultadoOperacion resultado, IConfiguration configuration, IHttpContextAccessor accessor)
        {
            _configuration = configuration;
            _resultado = resultado;
            _accessor = accessor;
            oBrSucursalUsuario = new BrSucursalUsuario();
        }

        [HttpGet("listaSucursalPorUsuarioAsync/{idUsuario?}")]
        [AllowAnonymous]
        public async Task<IActionResult> listaSucursalPorUsuarioAsync(string idUsuario)
        {
            //Lista de sucursales por usuario.
            _resultado = await Task.Run(() => oBrSucursalUsuario.listaSucursalPorUsuario(idUsuario));
            if (!_resultado.bResultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.sMensaje, Status = "Error" });

            return Ok(_resultado);
        }

        [HttpPost("cambiarSucursal")]
        public IActionResult cambiarSucursal([FromBody] RequestCambiarSucursalViewModel sucursal)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Mesagge = ModelState, Status = "Error" });

            //Obtenemos el usuario en sessión.
            UsuarioViewModel userCurrent = new Session(_accessor).obtenerUsuarioLogueado();

            //Generamos el accessToken y refreshToken.
            TokenGenerator tokenGenerator = new TokenGenerator(_configuration);
            TokensViewModel tokens = tokenGenerator.getTokens(new UsuarioViewModel()
            {
                idUsuario = userCurrent.idUsuario,
                nomUsuario = userCurrent.nomUsuario,
                nomRol = userCurrent.nomRol,
                idSucursal = sucursal.idSucursal,
                nomSucursal = sucursal.nomSucursal,
                flgCtrlTotal = userCurrent.flgCtrlTotal,
                ipAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString()
            });

            _resultado.SetResultado(true, "", tokens);

            return Ok(_resultado);
        }
    }
}
