using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioWebApi.SistemaVentas.Models.ViewModel
{
    public class VentaPorCodigoModel
    {
        public string idTipoComprobante { get; set; }
        public string nroSerie { get; set; }
        public int nroDocumento { get; set; }
        public string idCliente { get; set; }
        public string nomCliente { get; set; }
        public string dirCliente { get; set; }
        public string nroDocumentoCliente { get; set; }
        public string idMoneda { get; set; }
        public string sgnMoneda { get; set; }
        public string idTipoPago { get; set; }
        public string idTipoCondicion { get; set; }
        public string fecDocumento { get; set; }
        public string horDocumento { get; set; }
        public string fecVencimiento { get; set; }
        public string obsVenta { get; set; }
        public decimal totBruto { get; set; }
        public decimal totDescuento { get; set; }
        public decimal totImpuesto { get; set; }
        public decimal totVenta { get; set; }
        public decimal totVentaRedondeo { get; set; }
        public decimal tasDescuento { get; set; }
        public int estDocumento { get; set; }
        public string totVentaEnLetras { get; set; }
        public string idCajaCa { get; set; }
        public string idUsuarioCa { get; set; }
        public int correlativoCa { get; set; }
        public string nomCaja { get; set; }
        public string nomUsuarioCaja { get; set; }
        public List<VentaDetallePorCodigoModel> detalleVenta { get; set; }
    }
    public class VentaDetallePorCodigoModel
    {
        public string idArticulo { get; set; }
        public string nomArticulo { get; set; }
        public string idUm { get; set; }
        public string nomUm { get; set; }
        public decimal cantidad { get; set; }
        public decimal tasDescuento { get; set; }
        public decimal nroFactor { get; set; }
        public decimal precioArticulo { get; set; }
        public decimal precioUnitario { get; set; }
        public decimal importe { get; set; }
        public string abreviado { get; set; }
    }
}
