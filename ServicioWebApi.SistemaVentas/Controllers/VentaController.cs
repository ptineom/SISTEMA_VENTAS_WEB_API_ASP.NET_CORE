using CapaNegocio;
using Entidades;
using Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SistemaVentas.WebApi.Servicios.Seguridad;
using SistemaVentas.WebApi.ViewModels.Seguridad;
using SistemaVentas.WebApi.ViewModels.Venta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServicioWebApi.SistemaVentas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentaController : ControllerBase
    {
        private BrVenta _brVenta = null;
        private IResultadoOperacion _resultado = null;
        private IHttpContextAccessor _accessor = null;
        private IConfiguration _configuration = null;
        private string _idUsuario = string.Empty;
        private string _idSucursal = string.Empty;

        public VentaController(IResultadoOperacion resultado, IHttpContextAccessor accessor, IConfiguration configuration)
        {
            _configuration = configuration;
            _resultado = resultado;
            _accessor = accessor;
            UsuarioViewModel usuario = new Session(_accessor).GetUserLogged();
            _idUsuario = usuario.IdUsuario;
            _idSucursal = usuario.IdSucursal;
            _brVenta = new BrVenta(_configuration);
        }

        [HttpGet("GetData")]
        public async Task<IActionResult> GetDataAsync()
        {
            string tipoNCAnular = string.Empty;

            //Recuperar los listados para los combosBox de la pantalla.
            _resultado = await Task.Run(() => _brVenta.GetData(_idSucursal, _idUsuario, ref tipoNCAnular));

            if (!_resultado.Resultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.Mensaje, Status = "Error" });

            //objeto venta
            DOC_VENTA docVenta = (DOC_VENTA)_resultado.Data;

            //Lista con solo los campos que necesitamos.
            List<object> listaDocumentos = null, listaComprobantes = null, listaMonedas = null, listaTipPag = null, listaTipCon = null,
                           listaEstados = null, listaDepartamentos = null;

            if (docVenta.listaDocumentos != null)
            {
                listaDocumentos = docVenta.listaDocumentos.Select(x => new
                {
                    IdTipoDocumento = x.ID_TIPO_DOCUMENTO,
                    NomTipoDocumento = x.NOM_TIPO_DOCUMENTO,
                    Abreviatura = x.ABREVIATURA,
                    MaxDigitos = x.MAX_DIGITOS,
                    FlgRuc = x.FLG_RUC
                }).ToList<object>();
            }

            if (docVenta.listaComprobantes != null)
            {
                listaComprobantes = docVenta.listaComprobantes.Select(x => new
                {
                    IdTipoComprobante = x.ID_TIPO_COMPROBANTE,
                    NomTipoComprobante = ViewHelper.capitalizeFirstLetter(x.NOM_TIPO_COMPROBANTE),
                    FlgRendirSunat = x.FLG_RENDIR_SUNAT
                }).ToList<object>();
            }

            if (docVenta.listaMonedas != null)
            {
                listaMonedas = docVenta.listaMonedas.Select(x => new
                {
                    IdMoneda = x.ID_MONEDA,
                    NomMoneda = x.NOM_MONEDA,
                    FlgLocal = x.FLG_LOCAL,
                    SgnMoneda = x.SGN_MONEDA
                }).ToList<object>();
            }

            if (docVenta.listaTipPag != null)
            {
                listaTipPag = docVenta.listaTipPag.Select(x => new
                {
                    IdTipoPago = x.ID_TIPO_PAGO,
                    NomTipoPago = ViewHelper.capitalizeFirstLetter(x.NOM_TIPO_PAGO)
                }).ToList<object>();
            }

            if (docVenta.listaTipCon != null)
            {
                listaTipCon = docVenta.listaTipCon.Select(x => new
                {
                    IdTipoCondicionPago = x.ID_TIPO_CONDICION_PAGO,
                    NomTipoCondicionPago = ViewHelper.capitalizeFirstLetter(x.NOM_TIPO_CONDICION_PAGO),
                    FlgEvaluaCredito = x.FLG_EVALUA_CREDITO
                }).ToList<object>();
            }

            if (docVenta.listaEstados != null)
            {
                listaEstados = docVenta.listaEstados.Select(x => new
                {
                    IdEstado = x.ID_ESTADO,
                    NomEstado = ViewHelper.capitalizeFirstLetter(x.NOM_ESTADO)
                }).ToList<object>();
            }

            if (docVenta.listaDepartamentos != null)
            {
              
                listaDepartamentos = docVenta.listaDepartamentos.Select(x => new
                {
                    IdDepartamento = x.ID_UBIGEO,
                    NomDepartamento = ViewHelper.capitalizeAll(x.UBIGEO_DEPARTAMENTO)
                }).ToList<object>();
            }

            //Recuperar los datos de la empresa.
            BrEmpresa brEmpresa = new BrEmpresa();
            _resultado = new ResultadoOperacion();
            _resultado = await Task.Run(() => brEmpresa.GetByFilters(_idSucursal, false, true));

            if (!_resultado.Resultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.Mensaje, Status = "Error" });

            EMPRESA oEmpresa = (EMPRESA)_resultado.Data;
            object empresa = null;

            //Solo devolvemos los campos que necesitamos
            if (oEmpresa != null)
            {
                empresa = new
                {
                    IdEmpresa = oEmpresa.ID_EMPRESA,
                    NomEmpresa = oEmpresa.NOM_EMPRESA,
                    Igv = oEmpresa.IGV,
                    NumeroRuc = oEmpresa.NUMERO_RUC,
                    StockMinimo = oEmpresa.STOCK_MINIMO,
                    MontoBoletaObligatorioCliente = oEmpresa.MONTO_BOLETA_OBLIGATORIO_CLIENTE
                };
            }

            //Resultado final 
            _resultado = new ResultadoOperacion();
            _resultado.SetResultado(true, new
            {
                Listas = new
                {
                    ListaTipoDocumento = listaDocumentos,
                    ListaTipoComprobante = listaComprobantes,
                    ListaMoneda = listaMonedas,
                    ListaTipoPago = listaTipPag,
                    ListaTipoCondicion =listaTipCon,
                    ListaEstado = listaEstados,
                    ListaDepartamento = listaDepartamentos
                },
                Empresa = empresa
            });
            return Ok(_resultado);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> RegisterAsync(RequestVenta request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Mesagge = ModelState, Status = "Error" });

            if (request.DetalleVenta == null)
                return BadRequest(new { Mesagge = "No se envió el detalle de la venta.", Status = "Error" });


            //Ejecutar el grabado de la venta.
            _resultado = await Task.Run(() =>
            {
                //Agregando el detalle de la venta
                string jsonArticulos = JsonSerializer.Serialize(request.DetalleVenta)
                    .Replace("IdArticulo", "ID_ARTICULO")
                    .Replace("PrecioBase", "PRECIO_ARTICULO")
                    .Replace("IdUm", "ID_UM")
                    .Replace("Cantidad", "CANTIDAD")
                    .Replace("TasDescuento", "TAS_DESCUENTO")
                    .Replace("TasIgv", "TAS_IGV")
                    .Replace("NroFactor", "NRO_FACTOR")
                    .Replace("Importe", "IMPORTE");

                //Agregando la cabecera de la venta.
                DOC_VENTA docVenta = new DOC_VENTA()
                {
                    ACCION = "INS",
                    ID_USUARIO_REGISTRO = _idUsuario,
                    ID_SUCURSAL = _idSucursal,
                    ID_TIPO_COMPROBANTE = request.IdTipoComprobante,
                    ID_CLIENTE = request.IdCliente,
                    ID_MONEDA = request.IdMoneda,
                    FEC_DOCUMENTO = request.FecDocumento,
                    HOR_DOCUMENTO = request.HorDocumento,
                    FEC_VENCIMIENTO = request.FecVencimiento,
                    OBS_VENTA = request.ObsVenta,
                    TOT_BRUTO = request.TotBruto,
                    TOT_DESCUENTO = request.TotDescuento,
                    TOT_IMPUESTO = request.TotImpuesto,
                    TOT_VENTA = request.TotVenta,
                    TAS_DESCUENTO = request.TasDescuento,
                    ID_TIPO_PAGO = request.IdTipoPago,
                    ID_TIPO_CONDICION_PAGO = request.IdTipoCondicionPago,
                    ABONO = request.Abono,
                    SALDO = request.Saldo,
                    ID_CAJA_CA = request.IdCajaCa,
                    ID_USUARIO_CA = _idUsuario,
                    CORRELATIVO_CA = request.CorrelativoCa,
                    JSON_ARTICULOS = jsonArticulos
                };

                return _brVenta.Register(docVenta);
            });

            if (!_resultado.Resultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.Mensaje, Status = "Error" });

            return Ok(_resultado);
        }

        [HttpGet("GetById")]
        public async Task<IActionResult> GetByIdAsync([FromQuery] string idTipoComprobante, [FromQuery] string nroSerie, [FromQuery] int nroDocumento)
        {
            _resultado = await Task.Run(() => _brVenta.GetById(_idSucursal, idTipoComprobante, nroSerie, nroDocumento));

            if (!_resultado.Resultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.Mensaje, Status = "Error" });

            if (_resultado.Data == null)
                return NotFound(new { Message = "No se encontraron datos.", Status = "Error" });

            DOC_VENTA docVenta = (DOC_VENTA)_resultado.Data;

            //Construímos la data que queremos mostrar al cliente.
            _resultado.Data = new
            {
                Cabecera = new
                {
                    IdTipoComprobante = docVenta.ID_TIPO_COMPROBANTE,
                    NroSerie = docVenta.NRO_SERIE,
                    NroDocumento = Helper.ViewHelper.GetNroComprobante(docVenta.NRO_DOCUMENTO.ToString()),
                    IdCliente = docVenta.ID_CLIENTE,
                    NomCliente = ViewHelper.capitalizeAll(docVenta.NOM_CLIENTE),
                    DirCliente = docVenta.DIR_CLIENTE,
                    IdTipoDocumento = docVenta.ID_TIPO_DOCUMENTO,
                    NroDocumentoCliente = docVenta.NRO_DOCUMENTO_CLIENTE,
                    IdMoneda = docVenta.ID_MONEDA,
                    SgnMoneda = docVenta.SGN_MONEDA,
                    IdTipoPago = docVenta.ID_TIPO_PAGO,
                    IdTipoCondicion = docVenta.ID_TIPO_CONDICION_PAGO,
                    FecDocumento = docVenta.FEC_DOCUMENTO,
                    HorDocumento = docVenta.HOR_DOCUMENTO,
                    FecVencimiento = docVenta.FEC_VENCIMIENTO,
                    ObsVenta = docVenta.OBS_VENTA,
                    TotBruto = docVenta.TOT_BRUTO,
                    TotDescuento = docVenta.TOT_DESCUENTO,
                    TotImpuesto = docVenta.TOT_IMPUESTO,
                    TotVenta = docVenta.TOT_VENTA,
                    TotVentaRedondeo = docVenta.TOT_VENTA_REDONDEO,
                    TasDescuento = docVenta.TAS_DESCUENTO,
                    EstDocumento = docVenta.EST_DOCUMENTO,
                    TotVentaEnLetras = docVenta.TOT_VENTA_EN_LETRAS,
                    IdCajaCa = docVenta.ID_CAJA_CA,
                    IdUsuarioCa = docVenta.ID_USUARIO_CA,
                    CorrelativoCa = docVenta.CORRELATIVO_CA,
                    NomCaja = docVenta.NOM_CAJA,
                    NomUsuarioCaja = docVenta.NOM_USUARIO_CAJA
                },
                Detalle = docVenta.listaDetalle.Select(x => new
                {
                    IdArticulo = x.ID_ARTICULO,
                    NomArticulo = ViewHelper.capitalizeAll(x.NOM_ARTICULO),
                    IdUm = x.ID_UM,
                    NomUm = ViewHelper.capitalizeAll(x.NOM_UM),
                    Cantidad = x.CANTIDAD,
                    TasDescuento = x.TAS_DESCUENTO,
                    NroFactor = x.NRO_FACTOR,
                    PrecioArticulo = x.PRECIO_ARTICULO,
                    PrecioUnitario = x.PRECIO_UNITARIO,
                    Importe = x.IMPORTE,
                    Abreviado = x.ABREVIADO,
                    Codigo = string.IsNullOrEmpty(x.CODIGO_BARRA) ? x.ID_ARTICULO : x.CODIGO_BARRA
                }).ToList<object>()
            };

            return Ok(_resultado);
        }

        [HttpGet("GetAllByFilters")]
        public async Task<IActionResult> GetAllByFiltersAsync([FromQuery] string idCliente, [FromQuery] string idTipoComprobante, [FromQuery] string nroSerie,
            [FromQuery] int nroDocumento, [FromQuery] string fechaInicio, [FromQuery] string fechaFinal, [FromQuery] int idEstado)
        {
            _resultado = await Task.Run(() => _brVenta.GetAllByFilters(_idSucursal, idCliente, idTipoComprobante, nroSerie, nroDocumento, fechaInicio, fechaFinal, idEstado));

            if (!_resultado.Resultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.Mensaje, Status = "Error" });

            if (_resultado.Data == null)
                return NotFound(new { Message = "No se encontraron datos.", Status = "Error" });

            List<DOC_VENTA> listaDocVta = (List<DOC_VENTA>)_resultado.Data; ;

            _resultado.Data = listaDocVta.Select(x => new
            {
                Comprobante = x.COMPROBANTE,
                DocCliente = x.DOC_CLIENTE,
                NomCliente = ViewHelper.capitalizeAll(x.NOM_CLIENTE),
                SgnMoneda = x.SGN_MONEDA,
                TotVenta = x.TOT_VENTA,
                FecDocumento = x.FEC_DOCUMENTO,
                FlgEvaluaCredito = x.FLG_EVALUA_CREDITO,
                NomTipoCondicionPago = ViewHelper.capitalizeFirstLetter(x.NOM_TIPO_CONDICION_PAGO),
                EstDocumento = x.EST_DOCUMENTO,
                NomEstado = ViewHelper.capitalizeAll(x.NOM_ESTADO),
                IdTipoComprobante = x.ID_TIPO_COMPROBANTE,
                NroSerie = x.NRO_SERIE,
                NroDocumento = x.NRO_DOCUMENTO,
                EmailCliente = x.EMAIL_CLIENTE
            }).ToList<object>();

            return Ok(_resultado);
        }
    }
}
