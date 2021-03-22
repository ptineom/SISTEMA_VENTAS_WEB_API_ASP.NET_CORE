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
        private IConfiguration _configuration = null;
        private IResultadoOperacion _resultado = null;
        private BrUsuario _brUsuario = null;
        private IWebHostEnvironment _environment = null;
        private IHttpContextAccessor _accessor = null;

        public LoginController(IConfiguration configuration, IResultadoOperacion resultado,
            IWebHostEnvironment environment, IHttpContextAccessor accessor)
        {
            _configuration = configuration;
            _resultado = resultado;
            _environment = environment;
            _accessor = accessor;
            _brUsuario = new BrUsuario();
        }

        [HttpGet("GetTorito")]
        [AllowAnonymous]
        public IActionResult GetToritoAsync()
        {
            return Ok(new { name = "Hector toro valcneia" });
        }

        [HttpPost("ValidateUser")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateUserAsync([FromBody] RequestLoginViewModel request)
        {

            string token = string.Empty;
            string refreshToken = string.Empty;

            if (!ModelState.IsValid)
                return BadRequest(new { Mesagge = ModelState, Status = "Error" });

            if (string.IsNullOrWhiteSpace(request.IdUsuario) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { Message = "Usuario y/o contraseñas incorrectas", Status = "Error" });


            //Validamos la existencia del usuario en la BD.
            string passwordHash256 = HashHelper.GetHash256(request.Password);
            _resultado = await Task.Run(() => _brUsuario.ValidateUser(request.IdUsuario, passwordHash256));

            if (!_resultado.Resultado)
                return StatusCode(StatusCodes.Status404NotFound, new { Message = _resultado.Mensaje, Status = "Error" });

            try
            {
                //Datos del usuario
                USUARIO modelo = (USUARIO)_resultado.Data;
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
                        _resultado = await Task.Run(() => GetData(modelo));
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
                    _resultado = await Task.Run(() => oBrSucursalUsuario.GetSucursalesByUserId(modelo.ID_USUARIO));
                    if (!_resultado.Resultado)
                        return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.Mensaje, Status = "Error" });

                    _resultado.Data = new { Lista = _resultado.Data, FlgVariasSedes = true };
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

        [HttpPost("GenerateToken")]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateTokenAsync([FromBody] RequestUsuarioSucursalViewModel request)
        {
            string token = string.Empty;
            if (ModelState.IsValid)
            {
                try
                {
                    //Datos del usuario
                    _resultado = await Task.Run(() => _brUsuario.ValidateUser(request.IdUsuario, HashHelper.GetHash256(request.Password)));

                    if (!_resultado.Resultado)
                        return StatusCode(StatusCodes.Status404NotFound, new { Message = "Usuario y/o contraseñas incorrectas", Status = "Error" });

                    USUARIO modelo = (USUARIO)_resultado.Data;
                    //Pasamo las sucursal seleccionada en el cliente.
                    modelo.ID_SUCURSAL = request.IdSucursal;
                    modelo.NOM_SUCURSAL = request.NomSucursal;
                    try
                    {
                        //Generamos el jwt, refresjToken, menú, avatar, etc.
                        _resultado = await Task.Run(() => GetData(modelo));
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
        [HttpPost("GenerateTokenWithRefreshToken")]
        public async Task<IActionResult> GenerateTokenWithRefreshTokenAsync([FromBody] RequestRefreshTokenViewModel request)
        {
            //Generación de un nuevo accesToken con el refrehToken.
            BrRefreshToken br = new BrRefreshToken(_configuration);

            _resultado = await Task.Run(() => br.GetById(Helper.HashHelper.GetHash256(request.IdRefreshToken)));
            //Validamos la existencia del token
            if (_resultado.Data == null)
            {
                //    throw new SecurityTokenException()
                return StatusCode(StatusCodes.Status401Unauthorized, new { Message = "Refresh token no encontrado", Status = "Error" });
            }

            REFRESH_TOKEN oRefreshToken = (REFRESH_TOKEN)_resultado.Data;
            //Comprobamos si aún no ha expirádo el token.
            if (oRefreshToken.FEC_EXPIRACION_UTC < DateTime.UtcNow)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new { Message = "Refresh token expirado", Status = "Error" });
            }

            if (string.IsNullOrEmpty(oRefreshToken.JSON_CLAIMS))
                return StatusCode(StatusCodes.Status404NotFound, new { Message = "Datos del usuario no encontrado", Status = "Error" });

            string jsonClaims = oRefreshToken.JSON_CLAIMS.Replace("ID_USUARIO", "IdUsuario")
            .Replace("NOM_USUARIO", "NomUsuario").Replace("NOM_ROL", "NomRol").Replace("ID_SUCURSAL", "IdSucursal")
            .Replace("NOM_SUCURSAL", "NomSucursal").Replace("FLG_CTRL_TOTAL", "FlgCtrlTotal");

            UsuarioViewModel usuarioViewModel = JsonSerializer.Deserialize<UsuarioViewModel>(jsonClaims);

            //Generamos el accessToken y refreshToken con el modelo.
            TokensViewModel tokens = new TokenGenerator(_configuration).GetTokens(usuarioViewModel);

            return Ok(new { Token = tokens.AccessToken, tokens.RefreshToken });
        }

        private ResultadoOperacion GetData(USUARIO modelo)
        {
            Menu servicioMenu = new Menu(_environment,_configuration);

            //Obtengo el avatar en b64
            string avatar = servicioMenu.avatarB64(modelo.FOTO);

            //Construímos el menú.
            MenuItem menuItem = servicioMenu.GetMenuByUserId(modelo.ID_USUARIO);

            //Generamos el accessToken y refreshToken.
            TokenGenerator tokenGenerator = new TokenGenerator(_configuration);
            TokensViewModel tokens = tokenGenerator.GetTokens(new UsuarioViewModel()
            {
                IdUsuario = modelo.ID_USUARIO,
                NomUsuario = modelo.NOM_USUARIO,
                NomRol = modelo.NOM_ROL,
                IdSucursal = modelo.ID_SUCURSAL,
                NomSucursal = modelo.NOM_SUCURSAL,
                FlgCtrlTotal = modelo.FLG_CTRL_TOTAL,
                IpAddress = Request.HttpContext.Connection.RemoteIpAddress.ToString()
            });

            //Resultado final.
            ResultadoOperacion resultado = new ResultadoOperacion();
            resultado.SetResultado(true, new
            {
                Token = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken,
                MenuItem = menuItem,
                AvatarB64 = avatar,
                FlgVariasSedes = false
            });

            return resultado;
        }
    }
}
