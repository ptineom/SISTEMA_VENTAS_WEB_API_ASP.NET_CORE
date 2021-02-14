using CapaNegocio;
using Entidades;
using Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SistemaVentas.WebApi.Seguridad;
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
        private BrVenta _brVenta { get; }
        private IResultadoOperacion _resultado { get; set; }
        private IHttpContextAccessor _httpContextAccessor { get; set; }
        private string _idUsuario { get; set; }
        private string _idSucursal { get; set; }

        public VentaController(IResultadoOperacion resultado, IHttpContextAccessor httpContextAccessor)
        {
            _resultado = resultado;
            UsuarioViewModel usuario = new Session(httpContextAccessor).obtenerUsuarioLogueado();
            _idUsuario = usuario.idUsuario;
            _idSucursal = usuario.idSucursal;
            _brVenta = new BrVenta();
        }

        [HttpGet("getDataAsync")]
        public async Task<IActionResult> getDataAsync()
        {
            string tipoNCAnular = string.Empty;

            //Recuperar los listados para los combosBox de la pantalla.
            _resultado = await Task.Run(() => _brVenta.combosVentas(_idSucursal, _idUsuario, ref tipoNCAnular));

            if (!_resultado.bResultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.sMensaje, Status = "Error" });

            //objeto venta
            DOC_VENTA docVenta = (DOC_VENTA)_resultado.data;

            //Lista con solo los campos que necesitamos.
            List<object> listaDocumentos = null, listaComprobantes = null, listaMonedas = null, listaTipPag = null, listaTipCon = null,
                           listaEstados = null, listaDepartamentos = null;

            if (docVenta.listaDocumentos != null)
            {
                listaDocumentos = docVenta.listaDocumentos.Select(x => new
                {
                    idTipoDocumento = x.ID_TIPO_DOCUMENTO,
                    nomTipoDocumento = x.NOM_TIPO_DOCUMENTO,
                    abreviatura = x.ABREVIATURA,
                    flgNoNatural = x.FLG_NO_NATURAL,
                    maxDigitos = x.MAX_DIGITOS
                }).ToList<object>();
            }

            if (docVenta.listaComprobantes != null)
            {
                listaComprobantes = docVenta.listaComprobantes.Select(x => new
                {
                    idTipoComprobante = x.ID_TIPO_COMPROBANTE,
                    nomTipoComprobante = x.NOM_TIPO_COMPROBANTE,
                    flgRendirSunat = x.FLG_RENDIR_SUNAT
                }).ToList<object>();
            }

            if (docVenta.listaMonedas != null)
            {
                listaMonedas = docVenta.listaMonedas.Select(x => new
                {
                    idMoneda = x.ID_MONEDA,
                    nomMoneda = x.NOM_MONEDA,
                    flgLocal = x.FLG_LOCAL,
                    sgnMoneda = x.SGN_MONEDA
                }).ToList<object>();
            }

            if (docVenta.listaTipPag != null)
            {
                listaTipPag = docVenta.listaTipPag.Select(x => new
                {
                    idTipoPago = x.ID_TIPO_PAGO,
                    nomTipoPago = x.NOM_TIPO_PAGO
                }).ToList<object>();
            }

            if (docVenta.listaTipCon != null)
            {
                listaTipCon = docVenta.listaTipCon.Select(x => new
                {
                    idTipoCondicionPago = x.ID_TIPO_CONDICION_PAGO,
                    nomTipoCondicionPago = x.NOM_TIPO_CONDICION_PAGO,
                    flgNoEvaluaCredito = x.FLG_NO_EVALUA_CREDITO
                }).ToList<object>();
            }

            if (docVenta.listaEstados != null)
            {
                listaEstados = docVenta.listaEstados.Select(x => new
                {
                    idEstado = x.ID_ESTADO,
                    nomEstado = x.NOM_ESTADO
                }).ToList<object>();
            }

            if (docVenta.listaDepartamentos != null)
            {
                listaDepartamentos = docVenta.listaDepartamentos.Select(x => new
                {
                    idUbigeo = x.ID_UBIGEO,
                    ubigeoDepatamento = x.UBIGEO_DEPARTAMENTO
                }).ToList<object>();
            }

            //Recuperar los datos de la empresa.
            BrEmpresa brEmpresa = new BrEmpresa();
            _resultado = new ResultadoOperacion();
            _resultado = await Task.Run(() => brEmpresa.obtenerEmpresa(_idSucursal, false, true));

            if (!_resultado.bResultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.sMensaje, Status = "Error" });

            EMPRESA oEmpresa = (EMPRESA)_resultado.data;
            object empresa = null;

            //Solo devolvemos los campos que necesitamos
            if (oEmpresa != null)
            {
                empresa = new
                {
                    idEmpresa = oEmpresa.ID_EMPRESA,
                    nomEmpresa = oEmpresa.NOM_EMPRESA,
                    igv = oEmpresa.IGV,
                    numeroRuc = oEmpresa.NUMERO_RUC,
                    stockMinimo = oEmpresa.STOCK_MINIMO,
                    montoBoletaObligatorioCliente = oEmpresa.MONTO_BOLETA_OBLIGATORIO_CLIENTE
                };
            }

            //Resultado final 
            _resultado = new ResultadoOperacion();
            _resultado.SetResultado(true, new
            {
                listas = new
                {
                    listaDocumentos,
                    listaComprobantes,
                    listaMonedas,
                    listaTipPag,
                    listaTipCon,
                    listaEstados,
                    listaDepartamentos
                },
                empresa
            });
            return Ok(_resultado);
        }

        [HttpPost("grabarVentaAsync")]
        public async Task<IActionResult> grabarVentaAsync(RequestVenta venta)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Mesagge = ModelState, Status = "Error" });

            if (venta.detalleVenta == null)
                return BadRequest(new { Mesagge = "No se envió el detalle de la venta.", Status = "Error" });

            string nroSerie = string.Empty;
            string nroDocumento = string.Empty;

            //Ejecutar el grabado de la venta.
            _resultado = await Task.Run(() =>
            {
                //Agregando el detalle de la venta
                string jsonArticulos = JsonSerializer.Serialize(venta.detalleVenta)
                    .Replace("idArticulo", "ID_ARTICULO")
                    .Replace("precioBase", "PRECIO_ARTICULO")
                    .Replace("idUm", "ID_UM")
                    .Replace("cantidad", "CANTIDAD")
                    .Replace("tasDescuento", "TAS_DESCUENTO")
                    .Replace("tasIgv", "TAS_IGV")
                    .Replace("nroFactor", "NRO_FACTOR")
                    .Replace("importe", "IMPORTE");

                //Agregando la cabecera de la venta.
                DOC_VENTA docVenta = new DOC_VENTA()
                {
                    ACCION = "INS",
                    ID_USUARIO_REGISTRO = _idUsuario,
                    ID_SUCURSAL = _idSucursal,
                    ID_TIPO_COMPROBANTE = venta.idTipoComprobante,
                    ID_CLIENTE = venta.idCliente,
                    ID_MONEDA = venta.idMoneda,
                    FEC_DOCUMENTO = venta.fecDocumento,
                    HOR_DOCUMENTO = venta.horDocumento,
                    FEC_VENCIMIENTO = venta.fecVencimiento,
                    OBS_VENTA = venta.obsVenta,
                    TOT_BRUTO = venta.totBruto,
                    TOT_DESCUENTO = venta.totDescuento,
                    TOT_IMPUESTO = venta.totImpuesto,
                    TOT_VENTA = venta.totVenta,
                    TAS_DESCUENTO = venta.tasDescuento,
                    ID_TIPO_PAGO = venta.idTipoPago,
                    ID_TIPO_CONDICION_PAGO = venta.idTipoCondicionPago,
                    ABONO = venta.abono,
                    SALDO = venta.saldo,
                    ID_CAJA_CA = venta.idCajaCa,
                    ID_USUARIO_CA = _idUsuario,
                    CORRELATIVO_CA = venta.correlativoCa,
                    JSON_ARTICULOS = jsonArticulos
                };
                return _brVenta.grabarVenta(docVenta, ref nroSerie, ref nroDocumento);
            });

            if (_resultado.bResultado)
            {
                _resultado.data = new { nroSerie, nroDocumento = ViewHelper.getNroComprobante(nroDocumento) };
            }

            return Ok(_resultado);
        }

        [HttpGet("ventaPorCodigoAsync")]
        public async Task<IActionResult> ventaPorCodigoAsync([FromQuery] string idTipoComprobante, [FromQuery] string nroSerie, [FromQuery] int nroDocumento)
        {
            _resultado = await Task.Run(() => _brVenta.ventaPorCodigo(_idSucursal, idTipoComprobante, nroSerie, nroDocumento));

            if (!_resultado.bResultado)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.sMensaje, Status = "Error" });

            if (_resultado.data == null)
                return NotFound(new { Message = "No se encontraron datos.", Status = "Error" });

            DOC_VENTA docVenta = (DOC_VENTA)_resultado.data;

            //Construímos la data que queremos mostrar al cliente.
            _resultado.data = new
            {
                cabecera = new
                {
                    idTipoComprobante = docVenta.ID_TIPO_COMPROBANTE,
                    nroSerie = docVenta.NRO_SERIE,
                    nroDocumento = Helper.ViewHelper.getNroComprobante(docVenta.NRO_DOCUMENTO.ToString()),
                    idCliente = docVenta.ID_CLIENTE,
                    nomCliente = docVenta.NOM_CLIENTE,
                    dirCliente = docVenta.DIR_CLIENTE,
                    idTipoDocumento = docVenta.ID_TIPO_DOCUMENTO,
                    nroDocumentoCliente = docVenta.NRO_DOCUMENTO_CLIENTE,
                    idMoneda = docVenta.ID_MONEDA,
                    sgnMoneda = docVenta.SGN_MONEDA,
                    idTipoPago = docVenta.ID_TIPO_PAGO,
                    idTipoCondicion = docVenta.ID_TIPO_CONDICION_PAGO,
                    fecDocumento = docVenta.FEC_DOCUMENTO,
                    horDocumento = docVenta.HOR_DOCUMENTO,
                    fecVencimiento = docVenta.FEC_VENCIMIENTO,
                    obsVenta = docVenta.OBS_VENTA,
                    totBruto = docVenta.TOT_BRUTO,
                    totDescuento = docVenta.TOT_DESCUENTO,
                    totImpuesto = docVenta.TOT_IMPUESTO,
                    totVenta = docVenta.TOT_VENTA,
                    totVentaRedondeo = docVenta.TOT_VENTA_REDONDEO,
                    tasDescuento = docVenta.TAS_DESCUENTO,
                    estDocumento = docVenta.EST_DOCUMENTO,
                    totVentaEnLetras = docVenta.TOT_VENTA_EN_LETRAS,
                    idCajaCa = docVenta.ID_CAJA_CA,
                    idUsuarioCa = docVenta.ID_USUARIO_CA,
                    correlativoCa = docVenta.CORRELATIVO_CA,
                    nomCaja = docVenta.NOM_CAJA,
                    nomUsuarioCaja = docVenta.NOM_USUARIO_CAJA
                },
                detalle = docVenta.listaDetalle.Select(x => new
                {
                    idArticulo = x.ID_ARTICULO,
                    nomArticulo = x.NOM_ARTICULO,
                    idUm = x.ID_UM,
                    nomUm = x.NOM_UM,
                    cantidad = x.CANTIDAD,
                    tasDescuento = x.TAS_DESCUENTO,
                    nroFactor = x.NRO_FACTOR,
                    precioArticulo = x.PRECIO_ARTICULO,
                    precioUnitario = x.PRECIO_UNITARIO,
                    importe = x.IMPORTE,
                    abreviado = x.ABREVIADO
                }).ToList<object>()
            };

            return Ok(_resultado);
        }

        [HttpGet("listaVentasAsync")]
        public async Task<IActionResult> listaVentasAsync([FromQuery] string idCliente, [FromQuery] string idTipoComprobante, [FromQuery] string nroSerie,
            [FromQuery] int nroDocumento, [FromQuery] string fechaInicio, [FromQuery] string fechaFinal, [FromQuery] int idEstado)
        {
            _resultado = await Task.Run(() => _brVenta.listaVentas(_idSucursal, idCliente, idTipoComprobante, nroSerie, nroDocumento, fechaInicio, fechaFinal, idEstado));

            if (!_resultado.bResultado)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = _resultado.sMensaje, Status = "Error" });
            }
            if (_resultado.data == null)
            {
                return NotFound(new { Message = "No se encontraron datos.", Status = "Error" });
            }

            List<DOC_VENTA> listaDocVta = (List<DOC_VENTA>)_resultado.data; ;

            _resultado.data = listaDocVta.Select(x => new
            {
                comprobante = x.COMPROBANTE,
                docCliente = x.DOC_CLIENTE,
                nomCliente = x.NOM_CLIENTE,
                sgnMoneda = x.SGN_MONEDA,
                totVenta = x.TOT_VENTA,
                fecDocumento = x.FEC_DOCUMENTO,
                flgEvaluaCredito = !x.FLG_NO_EVALUA_CREDITO,
                nomTipoCondicionPago = x.NOM_TIPO_CONDICION_PAGO,
                estDocumento = x.EST_DOCUMENTO,
                nomEstado = x.NOM_ESTADO,
                idTipoComprobante = x.ID_TIPO_COMPROBANTE,
                nroSerie = x.NRO_SERIE,
                nroDocumento = x.NRO_DOCUMENTO,
                emailCliente = x.EMAIL_CLIENTE
            }).ToList<object>();

            return Ok(_resultado);
        }
    }
}
