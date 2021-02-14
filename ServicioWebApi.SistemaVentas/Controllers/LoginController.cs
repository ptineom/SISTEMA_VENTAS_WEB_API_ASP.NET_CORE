using CapaNegocio;
using Entidades;
using Helper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SistemaVentas.WebApi.Seguridad;
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
        private IWebHostEnvironment _hostingEnvironment { get; }

        public LoginController(IConfiguration configuration, IResultadoOperacion resultado, IWebHostEnvironment hostingEnvironment)
        {
            _configuration = configuration;
            _resultado = resultado;
            _hostingEnvironment = hostingEnvironment;
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

        /// <summary>
        /// Método recursivo que construirá el arbol de menus.
        /// </summary>
        /// <param name="aplicacion">Objeto del cuál se obtendrá los datos requeridos para la construcción del arbol.</param>
        /// <param name="listaGeneral">Lista de todos los menus que se usará para hacer filtrados</param>
        /// <param name="menuItem">Objeto donde se construirá el arbol de menu</param>
        private void setChildren(APLICACION aplicacion, List<APLICACION> listaGeneral, MenuItem menuItem)
        {
            menuItem.label = aplicacion.NOM_APLICACION;
            menuItem.icon = aplicacion.ICON_SPA;
            menuItem.route = aplicacion.ROUTE_SPA;
            menuItem.flgHome = aplicacion.FLG_HOME;

            if (aplicacion.FLG_FORMULARIO && !string.IsNullOrEmpty(aplicacion.BREADCRUMS))
            {
                //COnstruimos el array de objetos breadcrums
                string[] arrBreadCrums = aplicacion.BREADCRUMS.Split('|');
                List<object> breadCrums = new List<object>();
                for (int i = 0; i < arrBreadCrums.Length; i++)
                {
                    var obj = new
                    {
                        text = arrBreadCrums[i],
                        disabled = (i == (arrBreadCrums.Length - 1)) ? false : true
                    };
                    breadCrums.Add(obj);
                }
                menuItem.breadcrumbs = breadCrums;
            }
            //Si tiene hijos ejeuta la recursividad
            var childs = listaGeneral.Where(x => x.ID_APLICACION_PADRE == aplicacion.ID_APLICACION).ToList();
            if (childs.Count > 0)
            {
                List<MenuItem> listaSubMenu = new List<MenuItem>();
                foreach (var child in childs)
                {
                    MenuItem subMenu = new MenuItem();
                    setChildren(child, listaGeneral, subMenu);
                    listaSubMenu.Add(subMenu);
                };
                menuItem.children = listaSubMenu;
            }
        }

        private string avatarB64(string archivo)
        {
            string directorio = string.Empty;
            string b64 = "";
            byte[] foto = null;

            if (!string.IsNullOrEmpty(archivo))
            {
                string contentRootPath = _hostingEnvironment.WebRootPath;
                directorio = Path.Combine(contentRootPath, Configuraciones.UPLOAD_EMPLEADOS, archivo);
                foto = System.IO.File.ReadAllBytes(directorio);
                b64 = Convert.ToBase64String(foto);
            }
            else
            {
                string webRootPath = _hostingEnvironment.WebRootPath;
                directorio = Path.Combine(webRootPath, "Imagenes", "avatar_notFound.png");
                foto = System.IO.File.ReadAllBytes(directorio);
                b64 = Convert.ToBase64String(foto);
            }
            return b64;
        }

        private ResultadoOperacion getData(USUARIO modelo)
        {
            string token = string.Empty;
            string refreshToken = string.Empty;

            //Obtengo el avatar en b64
            string avatar = avatarB64(modelo.FOTO);

            BrAplicacion brAplicacion = new BrAplicacion();
            //Obtenemos la lista de menu según el usuario.
            ResultadoOperacion resultado = brAplicacion.listarMenuUsuario(modelo.ID_USUARIO);

            if (!resultado.bResultado)
                throw new Exception(resultado.sMensaje);

            //Construímos el menú a requerimiento del cliente.
            MenuItem menuItem = null;
            if (resultado.data != null)
            {
                menuItem = new MenuItem();
                List<APLICACION> listaGeneral = (List<APLICACION>)resultado.data;

                APLICACION aplicacionRaiz = listaGeneral.FirstOrDefault(x => x.FLG_RAIZ);

                //Método recursivo que construirá el arbol de menus.
                setChildren(aplicacionRaiz, listaGeneral, menuItem);
                //Marcamos a los primeros hijos como raiz para la renderización en la vista.
                menuItem.children.ForEach((elem) => elem.flgRaiz = true);
            }

            //Generamos el accessToken y refreshToken.
            TokensViewModel tokens = new TokenGenerator(_configuration).getTokens(new UsuarioViewModel()
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
            resultado = new ResultadoOperacion();
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
