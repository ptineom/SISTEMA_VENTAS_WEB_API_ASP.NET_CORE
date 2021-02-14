using CapaNegocio;
using Entidades;
using Helper;
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
using System.Threading.Tasks;

namespace ServicioWebApi.SistemaVentas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticuloController : ControllerBase
    {
        private IResultadoOperacion _resultado;
        private BrArticulo _brArticulo;
        private string _idSucursal;
        private string _idUsuario;
        private IConfiguration _configuration;
        private IWebHostEnvironment _environment;
        private IHttpContextAccessor _accessor;
        public ArticuloController(IResultadoOperacion resultado, IConfiguration configuration, IWebHostEnvironment environment, IHttpContextAccessor accessor)
        {
            _configuration = configuration;
            _resultado = resultado;
            _brArticulo = new BrArticulo(configuration, environment);
            _accessor = accessor;

            UsuarioViewModel usuario = Session.obtenerUsuarioLogueadoStatic();
            //UsuarioViewModel usuario = new Session(_accessor).obtenerUsuarioLogueado();
            _idUsuario = usuario.idUsuario;
            _idSucursal = usuario.idSucursal;
            _environment = environment;
        }

        [HttpGet("getData")]
        public async Task<IActionResult> getDataAsync()
        {
            BrEmpresa brEmpresa = new BrEmpresa(_configuration, _environment);
            BrGrupo brGrupo = new BrGrupo();
            BrMoneda brMoneda = new BrMoneda();
            ResultadoOperacion resultEmpresa = await Task.Run(() => brEmpresa.obtenerEmpresa("", true, false));
            ResultadoOperacion resultGrupo = await Task.Run(() => brGrupo.listaGrupos());
            ResultadoOperacion resultMoneda = await Task.Run(() => brMoneda.listaMonedas());

            if (!resultEmpresa.bResultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = resultEmpresa.sMensaje, Status = "Error" });

            if (!resultGrupo.bResultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = resultGrupo.sMensaje, Status = "Error" });

            if (resultEmpresa.data == null)
                return StatusCode(StatusCodes.Status404NotFound, new { Message = "Datos no encontrado.", Status = "Error" });

            if (resultGrupo.data == null)
                return StatusCode(StatusCodes.Status404NotFound, new { Message = "Debe de configurar las clasificaciones de grupos.", Status = "Error" });

            if (resultMoneda.data == null)
                return StatusCode(StatusCodes.Status404NotFound, new { Message = "Debe de configurar las monedas.", Status = "Error" });

            EMPRESA modelo = (EMPRESA)resultEmpresa.data;

            if (modelo.sucursales == null)
                return StatusCode(StatusCodes.Status404NotFound, new { Message = "Debe de configurar al menos una sede.", Status = "Error" });


            List<object> sucursales = ((List<SUCURSAL>)modelo.sucursales).Select(x => new
            {
                idSucursal = x.ID_SUCURSAL,
                nomSucursal = x.NOM_SUCURSAL,
                direccion = x.DIRECCION,
                nomAlmacen = x.NOM_ALMACEN,
                stockActual = 0
            }).ToList<object>();

            List<object> grupos = ((List<GRUPO>)resultGrupo.data).Select(y => new
            {
                idGrupo = y.ID_GRUPO,
                nomGrupo = y.NOM_GRUPO
            }).ToList<object>();

            //Moneda local
            var monedaLocal = ((List<MONEDA>)resultMoneda.data).Where(x => x.FLG_LOCAL == true).Select(y => new
            {
                idMoneda = y.ID_MONEDA,
                nomMoneda = y.NOM_MONEDA,
                sgnMoneda = y.SGN_MONEDA
            }).FirstOrDefault();

            //Resultado final 
            _resultado = new ResultadoOperacion();
            _resultado.SetResultado(true, new
            {
                igv = modelo.IGV,
                sucursales,
                grupos,
                monedaLocal
            });

            return Ok(_resultado);
        }

        [HttpGet("listaArticulos")]
        public async Task<IActionResult> listaArticulosAsync([FromQuery] string accion, [FromQuery] string tipoFiltro, [FromQuery] string filtro)
        {
            _resultado = await Task.Run(() => _brArticulo.listaArticulos(accion, _idSucursal, tipoFiltro, filtro));

            if (!_resultado.bResultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.sMensaje, Status = "Error" });

            if (_resultado.data == null)
                return StatusCode(StatusCodes.Status404NotFound, new { Message = "Datos no encontrado.", Status = "Error" });

            _resultado.data = ((List<ARTICULO>)_resultado.data).Select(x => new
            {
                idArticulo = x.ID_ARTICULO,
                nomArticulo = x.NOM_ARTICULO,
                nomMarca = x.NOM_MARCA,
                nomGrupo = x.NOM_GRUPO,
                nomFamilia = x.NOM_FAMILIA,
                codigoBarra = x.CODIGO_BARRA,
                stockActual = x.STOCK_ACTUAL,
                precioVentaFinal = x.PRECIO_VENTA_FINAL,
                flgInactivo = x.FLG_INACTIVO,
                foto1 = x.FOTO1,
                foto2 = x.FOTO2
            });

            return Ok(_resultado);
        }

        [HttpPost("grabarArticulo")]
        public async Task<IActionResult> grabarArticuloAsync([FromForm] RequestGrabarArticulo modelo)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Mesagge = ModelState, Status = "Error" });

            string idArticulo = string.Empty, namePhoto = string.Empty;
            bool flgMismaFoto = false;

            IFormFileCollection files = modelo.fileBinary;
            int countFiles = 0;

            if (files != null)
            {
                countFiles = files.Count;
                //Si el modelo tiene foto, validamos el formato y tamaño del archivo.
                if (countFiles > 0)
                {
                    IFormFile file = files[0];
                    //Formato permitidos.
                    string[] arrExtensiones = new string[] { ".png", ".jpg", ".jpeg" };
                    string mensajeError = ImageHelper.TryParse(file, ImageHelper.TipoTamanio.mb, Configuraciones.TAMANIO_MAX_ARCHIVO_MB, arrExtensiones);

                    if (!string.IsNullOrEmpty(mensajeError))
                        return StatusCode(StatusCodes.Status400BadRequest, new { Message = mensajeError, Status = "Error" });

                    namePhoto = file.FileName;
                }
            }


            ARTICULO articulo = new ARTICULO()
            {
                ACCION = modelo.accion,
                ID_ARTICULO = modelo.idArticulo,
                NOM_ARTICULO = modelo.nomArticulo,
                NOM_VENTA = modelo.nomVenta,
                PRECIO_COMPRA = modelo.precioCompra,
                PRECIO_VENTA = modelo.precioBase,
                FLG_IMPORTADO = modelo.flgImportado,
                FLG_INACTIVO = modelo.flgInactivo,
                ID_MARCA = modelo.idMarca,
                ID_GRUPO = modelo.idGrupo,
                ID_FAMILIA = modelo.idFamilia,
                CODIGO_BARRA = modelo.codigoBarra,
                STOCK_MINIMO = modelo.stockMinimo,
                ID_USUARIO_REGISTRO = _idUsuario,
                ID_SUCURSAL = _idSucursal,
                JSON_SUCURSAL = modelo.sucursales,
                JSON_UM = modelo.articulosUm,
                NOM_FOTO = namePhoto
            };

            //Grabamos el artículo (Si realizamos una actualización, el procedimiento nos devolverá los parámetros de salida: 
            //jsonFotos != "" => Si devuelve las fotos, será para borrarlas del directorio
            //flgMismaFoto == true => No seguirá con el proceso de guardado de foto en el directorio.)
            string jsonFotos = string.Empty;
            _resultado = await Task.Run(() => _brArticulo.grabarArticulo(articulo, ref idArticulo, ref jsonFotos, ref flgMismaFoto));

            //Si el grabado fue exitoso, guardamos tambien las fotos en el directorio.
            if (!_resultado.bResultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.sMensaje, Status = "Error" });

            try
            {
                if (modelo.accion == "UPD")
                {
                    idArticulo = modelo.idArticulo;

                    if (flgMismaFoto)
                        countFiles = 0;
                    else
                    {
                        //Elimina las fotos del directorio
                        if (!string.IsNullOrEmpty(jsonFotos))
                            BrFotos.deleteFotosDirectory(jsonFotos, Path.Combine(_environment.WebRootPath, Configuraciones.UPLOAD_ARTICULOS));
                    }
                }

                if (countFiles > 0)
                {
                    IFormFile file = files[0];
                    string uri = Path.Combine(_environment.WebRootPath, Configuraciones.UPLOAD_ARTICULOS, file.FileName);

                    //Guardamos la imagen y posteriormente la usaremos de modelo para crear nuevas imagenes.
                    using (FileStream fs = new FileStream(uri, FileMode.Create))
                    {
                        file.CopyTo(fs);
                    }

                    ResultadoOperacion resultFoto = new ResultadoOperacion();
                    BrFotos brFoto = new BrFotos();

                    //La extensión a guardar será jpg.
                    string extension = Path.GetExtension(file.FileName);
                    if (extension.Equals(".png", StringComparison.OrdinalIgnoreCase))
                        extension = ".jpg";

                    //Generamos las fotos en 2 medidas, apartir de la imagen ya guardada.
                    int[] medidas = Configuraciones.SCALES_IMAGES_ARTICULOS;
                    FOTOS oFoto = new FOTOS()
                    {
                        ID_TIPO_FOTO = Configuraciones.ID_TIPO_FOTO_ARTICULO,
                        ID_ELEMENTO = idArticulo,
                        ID_USUARIO_REGISTRO = _idUsuario
                    };
                    //Proceso de guardado de fotos en bd y creacion de imagenes según las escalas indicadas.
                    resultFoto = await Task.Run(() => brFoto.procesoGrabadoFotos(uri, medidas, oFoto, extension));

                    if (!resultFoto.bResultado)
                        throw new Exception(resultFoto.sMensaje);

                    //Después crear la imágenes nuevas, eliminamos la imágen modelo.
                    ImageHelper.deleteFile(uri);
                }
            }
            catch (Exception ex)
            {
                Helper.Elog.save(this, ex);
            }

            return Ok(_resultado);
        }

        [HttpPost("anularArticulo/{idArticulo}")]
        public async Task<IActionResult> anularArticuloAsync(string idArticulo)
        {
            _resultado = await Task.Run(() => _brArticulo.anularArticulo(idArticulo, _idUsuario));

            if (!_resultado.bResultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.sMensaje, Status = "Error" });

            //Las fotos que se eliminaran el directorio
            if (_resultado.data != null)
                BrFotos.deleteFotosDirectory(_resultado.data.ToString(), Path.Combine(_environment.WebRootPath, Configuraciones.UPLOAD_ARTICULOS));

            return Ok(_resultado);
        }

        [HttpGet("obtenerArticuloPorCodigo/{idArticulo}")]
        public async Task<IActionResult> obtenerArticuloPorCodigoAsync(string idArticulo)
        {
            ARTICULO articulo = null;
            List<object> listaFamilia = null;
            List<object> listaUm = null;
            List<object> listaArticuloUm = null;
            List<object> listaSucursal = null;
            object modelo = null;

            //Obtenemos el artículo y algunas listas de la BD.
            _resultado = await Task.Run(() => _brArticulo.articuloPorCodigo(_idSucursal, idArticulo));

            if (!_resultado.bResultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.sMensaje, Status = "Error" });

            if (_resultado.data == null)
                return StatusCode(StatusCodes.Status404NotFound, new { Message = "Datos no encontrados.", Status = "Error" });

            articulo = (ARTICULO)_resultado.data;

            //Lista de articulo_Um
            if (articulo.listaArticuloUm != null)
            {
                listaArticuloUm = articulo.listaArticuloUm.Select(x => new
                {
                    idArticulo = x.ID_ARTICULO,
                    idUm = x.ID_UM,
                    nroFactor = x.NRO_FACTOR,
                    nroOrden = x.NRO_ORDEN,
                    flgPromocion = x.FLG_PROMOCION,
                    descuento = x.DESCUENTO1,
                    fecInicioPromocion = x.FEC_INICIO_PROMOCION,
                    fecFinalPromocion = x.FEC_FINAL_PROMOCION,
                    precioVenta = x.PRECIO_VENTA,
                    precioVentaFinal = x.PRECIO_VENTA_FINAL
                }).ToList<object>();
            }

            //Lista de sucursales
            if (articulo.listaSucursal != null)
            {
                listaSucursal = articulo.listaSucursal.Select(x => new
                {
                    flgEnUso = x.FLG_EN_USO,
                    nomAlmacen = x.NOM_ALMACEN,
                    stockActual = x.STOCK_ACTUAL,
                    idSucursal = x.ID_SUCURSAL,
                    nomSucursal = x.NOM_SUCURSAL,
                    direccion = $"{x.DIRECCION} {x.NOM_UBIGEO}",
                }).ToList<object>();
            }

            ResultadoOperacion _resultFamilia = new ResultadoOperacion();
            BrFamilia _brFamilia = new BrFamilia();

            //Obtenemos las familias por idGrupo de la BD.
            _resultFamilia = _brFamilia.cboFamilia(articulo.ID_GRUPO);

            if (!_resultFamilia.bResultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultFamilia.sMensaje, Status = "Error" });

            if (_resultFamilia.data != null)
            {
                List<FAMILIA> familias = (List<FAMILIA>)_resultFamilia.data;
                listaFamilia = familias.Select(x => new { idFamilia = x.ID_FAMILIA, nomFamilia = x.NOM_FAMILIA }).ToList<object>();
            }

            ResultadoOperacion _resultUm = new ResultadoOperacion();
            BrUnidadMedida _brUm = new BrUnidadMedida();

            //Obtenemos las um_familia por idGrupo y idFamilia de la BD.
            if (!string.IsNullOrEmpty(articulo.ID_GRUPO) && !string.IsNullOrEmpty(articulo.ID_FAMILIA))
            {
                _resultUm = _brUm.listaUmPorFamilia(articulo.ID_GRUPO, articulo.ID_FAMILIA);

                if (!_resultUm.bResultado)
                    return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultFamilia.sMensaje, Status = "Error" });

                if (_resultUm.data != null)
                {
                    List<UNIDAD_MEDIDA> ums = (List<UNIDAD_MEDIDA>)_resultUm.data;
                    listaUm = ums.Select(x => new { idUm = x.ID_UM, nomUm = x.NOM_UM }).ToList<object>();
                }
            }

            //Modelo de artículo.
            modelo = new
            {
                idArticulo = articulo.ID_ARTICULO,
                nomArticulo = articulo.NOM_ARTICULO,
                nomVenta = articulo.NOM_VENTA,
                codigoBarra = articulo.CODIGO_BARRA,
                idMarca = articulo.ID_MARCA,
                nomMarca = articulo.NOM_MARCA,
                idGrupo = articulo.ID_GRUPO,
                idFamilia = articulo.ID_FAMILIA,
                precioVenta = articulo.PRECIO_VENTA,
                precioCompra = articulo.PRECIO_COMPRA,
                stockMinimo = articulo.STOCK_MINIMO,
                flgImportado = articulo.FLG_IMPORTADO,
                flgInactivo = articulo.FLG_INACTIVO,
                foto1 = articulo.FOTO1,
                foto2 = articulo.FOTO2,
                fotoB64 = articulo.FOTO_B64
            };

            //modelo y listas a devolver
            _resultado.data = new
            {
                modelo,
                listaArticuloUm,
                listaFamilia,
                listaUm,
                listaSucursal
            };

            return Ok(_resultado);
        }

        [HttpGet("listaArticulosGeneral")]
        public async Task<IActionResult> listaArticulosGeneralAsync([FromQuery] string tipoFiltro = "", [FromQuery] string filtro = "", [FromQuery] string accion = "")
        {
            MONEDA moneda = null;
            _resultado = await Task.Run(() => _brArticulo.listaArticulosGeneral(accion, _idSucursal, tipoFiltro, filtro, ref moneda));

            if (!_resultado.bResultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.sMensaje, Status = "Error" });

            if (_resultado.data == null)
                return NotFound(new { Message = "No se encontraron datos.", Status = "Error" });

            List<ARTICULO> lista = (List<ARTICULO>)_resultado.data;

            _resultado.data = new
            {
                lista = lista.Select(x => new
                {
                    idArticulo = x.ID_ARTICULO,
                    codigo = string.IsNullOrEmpty(x.CODIGO_BARRA) ? x.ID_ARTICULO : x.CODIGO_BARRA,
                    nomArticulo = x.NOM_ARTICULO,
                    nomMarca = x.NOM_MARCA,
                    nomUm = x.NOM_UM,
                    stockActual = x.STOCK_ACTUAL,
                    precioVentaFinal = x.PRECIO_VENTA_FINAL,
                    descuento1 = x.DESCUENTO1,
                    idUm = x.ID_UM,
                    nroFactor = x.NRO_FACTOR,
                    unidadMedidas = x.listaArticuloUm.Select(y => new
                    {
                        value = y.ID_UM,
                        text = y.NOM_UM,
                        nroFactor = y.NRO_FACTOR,
                        descuento1 = y.DESCUENTO1,
                        precioVenta = y.PRECIO_VENTA,
                        precioVentaFinal = y.PRECIO_VENTA_FINAL
                    }).ToList(),
                    precioBase = x.PRECIO_BASE,
                    precioVenta = x.PRECIO_VENTA,
                    stockMinimo = x.STOCK_MINIMO
                }).ToList(),
                moneda = new
                {
                    idMoneda = moneda.ID_MONEDA,
                    nomMoneda = moneda.NOM_MONEDA,
                    sgnMoneda = moneda.SGN_MONEDA,
                    flgLocal = moneda.FLG_LOCAL
                }
            };

            return Ok(_resultado);
        }
    }
}
