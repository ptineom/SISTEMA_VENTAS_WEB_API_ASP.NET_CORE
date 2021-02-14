using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaVentas.WebApi.ViewModels.Venta
{
    public class RequestVenta
    {
        public string idTipoComprobante { get; set; }
        public string nroSerie { get; set; }
        public string nroDocumento { get; set; }
        public string idCliente { get; set; }
        public string idMoneda { get; set; }
        public string fecDocumento { get; set; }
        public string horDocumento { get; set; }
        public string fecVencimiento { get; set; }
        public string obsVenta { get; set; }
        public decimal totBruto { get; set; }
        public decimal totDescuento { get; set; }
        public decimal totImpuesto { get; set; }
        public decimal totVenta { get; set; }
        public decimal tasDescuento { get; set; }
        public string idTipoPago { get; set; }
        public string idTipoCondicionPago { get; set; }
        public decimal abono { get; set; }
        public decimal saldo { get; set; }
        public string idCajaCa { get; set; }
        public string idUsuarioCa { get; set; }
        public int correlativoCa { get; set; }
        public List<RequestVentaDetalle> detalleVenta { get; set; }
    }

    public class RequestVentaDetalle
    {
        public string idArticulo { get; set; }
        public decimal cantidad { get; set; }
        public string idUm { get; set; }
        public decimal nroFactor { get; set; }
        public decimal precioBase { get; set; }
        public decimal tasDescuento { get; set; }
        public decimal tasIgv { get; set; }
        public decimal importe { get; set; }
    }
}
