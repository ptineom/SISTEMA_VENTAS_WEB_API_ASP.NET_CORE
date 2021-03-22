﻿using CapaNegocio;
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
        private IConfiguration _configuration = null;
        public IHttpContextAccessor _accessor = null;
        private IResultadoOperacion _resultado = null;
        private BrSucursalUsuario oBrSucursalUsuario = null;

        public SucursalUsuarioController(IResultadoOperacion resultado, IConfiguration configuration, IHttpContextAccessor accessor)
        {
            _configuration = configuration;
            _resultado = resultado;
            _accessor = accessor;
            oBrSucursalUsuario = new BrSucursalUsuario();
        }

        [HttpGet("GetSucursalesByUserId/{idUsuario?}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSucursalesByUserIdAsync(string idUsuario)
        {
            //Lista de sucursales por usuario.
            _resultado = await Task.Run(() => oBrSucursalUsuario.GetSucursalesByUserId(idUsuario));
            if (!_resultado.Resultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.Mensaje, Status = "Error" });

            return Ok(_resultado);
        }

        [HttpPost("ChangeSucursal")]
        public IActionResult ChangeSucursal([FromBody] RequestCambiarSucursalViewModel sucursal)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Mesagge = ModelState, Status = "Error" });

            //Obtenemos el usuario en sessión.
            UsuarioViewModel userCurrent = new Session(_accessor).GetUserLogged();

            //Generamos el accessToken y refreshToken.
            TokenGenerator tokenGenerator = new TokenGenerator(_configuration);
            TokensViewModel tokens = tokenGenerator.GetTokens(new UsuarioViewModel()
            {
                IdUsuario = userCurrent.IdUsuario,
                NomUsuario = userCurrent.NomUsuario,
                NomRol = userCurrent.NomRol,
                IdSucursal = sucursal.IdSucursal,
                NomSucursal = sucursal.NomSucursal,
                FlgCtrlTotal = userCurrent.FlgCtrlTotal,
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString()
            });

            _resultado.SetResultado(true, "", new { Token = tokens.AccessToken,  tokens.RefreshToken});

            return Ok(_resultado);
        }
    }
}
