using CapaNegocio;
using Entidades;
using Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ServicioWebApi.SistemaVentas.Servicios.Seguridad;
using SistemaVentas.WebApi.Servicios.Seguridad;
using SistemaVentas.WebApi.ViewModels;
using SistemaVentas.WebApi.ViewModels.Seguridad;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServicioWebApi.SistemaVentas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private IConfiguration _configuration { get; }
        private IResultadoOperacion _resultado { get; set; }

        private BrUsuario oBrUsuario = null;
        private IWebHostEnvironment _environment { get; }
        private IHttpContextAccessor _accessor { get; set; }

        public LoginController(IConfiguration configuration, IResultadoOperacion resultado,
            IWebHostEnvironment environment, IHttpContextAccessor accessor)
        {
            _configuration = configuration;
            _resultado = resultado;
            _environment = environment;
            _accessor = accessor;
            oBrUsuario = new BrUsuario();
        }

        [HttpPost("accederAsync")]
        [AllowAnonymous]
        public async Task<IActionResult> accederAsync([FromBody] RequestLoginViewModel login)
        {

            string token = string.Empty;
            string refreshToken = string.Empty;

            if (!ModelState.IsValid)
                return BadRequest(new { Mesagge = ModelState, Status = "Error" });

            if (string.IsNullOrWhiteSpace(login.idUsuario) || string.IsNullOrWhiteSpace(login.password))
                return BadRequest(new { Message = "Usuario y/o contraseñas incorrectas", Status = "Error" });


            //Validamos la existencia del usuario en la BD.
            string passwordHash256 = HashHelper.GetHash256(login.password);
            _resultado = await Task.Run(() => oBrUsuario.acceder(login.idUsuario, passwordHash256));

            if (!_resultado.bResultado)
                return StatusCode(StatusCodes.Status404NotFound, new { Message = _resultado.sMensaje, Status = "Error" });

            try
            {
                //Datos del usuario
                USUARIO modelo = (USUARIO)_resultado.data;
                int countSedes = modelo.COUNT_SEDES;

                if (countSedes == 0)
                    return NotFound(new { Message = "Este usuario no tiene configurado al menos una sede.", Status = "Error" });

                _resultado = new ResultadoOperacion();

                //**** Si tiene una sede asignada, generamos el token.
                if (countSedes == 1)
                {
                    try
                    {
                        //Generamos el jwt, refresjToken, menú, avatar, etc.
                        _resultado = await Task.Run(() => getData(modelo));
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message, Status = "Error" });
                    }
                }
                else if (countSedes > 1)
                {
                    //**** Si tiene varias sedes asignadas, se enviará la lista de sedes para su previa selección antes de generar el token.
                    BrSucursalUsuario oBrSucursalUsuario = new BrSucursalUsuario();

                    //Lista de sucursales por usuario.
                    _resultado = await Task.Run(() => oBrSucursalUsuario.listaSucursalPorUsuario(modelo.ID_USUARIO));
                    if (!_resultado.bResultado)
                        return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.sMensaje, Status = "Error" });

                    _resultado.data = new { lista = _resultado.data, bVariasSedes = true };
                }
            }
            catch (InvalidOperationException ex)
            {
                object obj = new { Message = ex.Message, Status = "Error" };
                return StatusCode(StatusCodes.Status500InternalServerError, obj);
            }
            catch (Exception ex)
            {
                object obj = new { Message = ex.Message, Status = "Error" };
                return StatusCode(StatusCodes.Status500InternalServerError, obj);
            }
            return Ok(_resultado);
        }

        [HttpPost("generarTokenAsync")]
        [AllowAnonymous]
        public async Task<IActionResult> generarTokenAsync([FromBody] RequestUsuarioSucursalViewModel login)
        {
            string token = string.Empty;
            if (ModelState.IsValid)
            {
                try
                {
                    //Datos del usuario
                    _resultado = await Task.Run(() => oBrUsuario.acceder(login.idUsuario, HashHelper.GetHash256(login.password)));

                    if (!_resultado.bResultado)
                        return StatusCode(StatusCodes.Status404NotFound, new { Message = "Usuario y/o contraseñas incorrectas", Status = "Error" });

                    USUARIO modelo = (USUARIO)_resultado.data;
                    //Pasamo las sucursal seleccionada en el cliente.
                    modelo.ID_SUCURSAL = login.idSucursal;
                    modelo.NOM_SUCURSAL = login.nomSucursal;
                    try
                    {
                        //Generamos el jwt, refresjToken, menú, avatar, etc.
                        _resultado = await Task.Run(() => getData(modelo));
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new { Message = ex.Message, Status = "Error" });
                    }
                }
                catch (InvalidOperationException ex)
                {
                    object obj = new { Message = ex.Message, Status = "Error" };
                    return StatusCode(StatusCodes.Status500InternalServerError, obj);
                }
                catch (Exception ex)
                {
                    object obj = new { Message = ex.Message, Status = "Error" };
                    return StatusCode(StatusCodes.Status500InternalServerError, obj);
                }
            }
            else
            {
                //var errors = string.Join(" | ", ModelState.Values.SelectMany(q => q.Errors).Select(w => w.ErrorMessage));
                return BadRequest(new { Mesagge = ModelState, Status = "Error" });
            }
            return Ok(_resultado);
        }

        [AllowAnonymous]
        [HttpPost("generarTokenWithRefreshTokenAsync")]
        public async Task<IActionResult> generarTokenWithRefreshTokenAsync([FromBody] RequestRefreshTokenViewModel modelo)
        {
            //Generación de un nuevo accesToken con el refrehToken.
            BrRefreshToken br = new BrRefreshToken();

            _resultado = await Task.Run(() => br.refreshTokenPorCodigo(Helper.HashHelper.GetHash256(modelo.idRefreshToken)));
            //Validamos la existencia del token
            if (_resultado.data == null)
            {
                //    throw new SecurityTokenException()
                return StatusCode(StatusCodes.Status401Unauthorized, new { Message = "Refresh token no encontrado", Status = "Error" });
            }

            REFRESH_TOKEN oRefreshToken = (REFRESH_TOKEN)_resultado.data;
            //Comprobamos si aún no ha expirádo el token.
            if (oRefreshToken.FEC_EXPIRACION_UTC < DateTime.UtcNow)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new { Message = "Refresh token expirado", Status = "Error" });
            }

            if (string.IsNullOrEmpty(oRefreshToken.JSON_CLAIMS))
                return StatusCode(StatusCodes.Status404NotFound, new { Message = "Datos del usuario no encontrado", Status = "Error" });

            string jsonClaims = oRefreshToken.JSON_CLAIMS.Replace("ID_USUARIO", "idUsuario")
            .Replace("NOM_USUARIO", "nomUsuario").Replace("NOM_ROL", "nomRol").Replace("ID_SUCURSAL", "idSucursal")
            .Replace("NOM_SUCURSAL", "nomSucursal").Replace("FLG_CTRL_TOTAL", "flgCtrlTotal");

            UsuarioViewModel usuarioViewModel = JsonSerializer.Deserialize<UsuarioViewModel>(jsonClaims);

            //Generamos el accessToken y refreshToken con el modelo.
            TokensViewModel tokens = new TokenGenerator(_configuration).getTokens(usuarioViewModel);

            return Ok(new { token = tokens.accessToken, tokens.refreshToken });
        }

        private ResultadoOperacion getData(USUARIO modelo)
        {
            string token = string.Empty;
            string refreshToken = string.Empty;
            Menu servicioMenu = new Menu(_environment);

            //Obtengo el avatar en b64
            string avatar = servicioMenu.avatarB64(modelo.FOTO);

            //Construímos el menú.
            MenuItem menuItem = servicioMenu.obtenerMenuPorUsuario(modelo.ID_USUARIO);

            //Generamos el accessToken y refreshToken.
            TokenGenerator tokenGenerator = new TokenGenerator(_configuration);
            TokensViewModel tokens = tokenGenerator.getTokens(new UsuarioViewModel()
            {
                idUsuario = modelo.ID_USUARIO,
                nomUsuario = modelo.NOM_USUARIO,
                nomRol = modelo.NOM_ROL,
                idSucursal = modelo.ID_SUCURSAL,
                nomSucursal = modelo.NOM_SUCURSAL,
                flgCtrlTotal = modelo.FLG_CTRL_TOTAL,
                ipAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString()
            });

            //Resultado final.
            ResultadoOperacion resultado = new ResultadoOperacion();
            resultado.SetResultado(true, new
            {
                token = tokens.accessToken,
                refreshToken = tokens.refreshToken,
                menuItem,
                avatarB64 = avatar,
                bVariasSedes = false
            });

            return resultado;
        }
    }
}
