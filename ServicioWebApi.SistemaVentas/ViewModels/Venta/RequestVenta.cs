using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaVentas.WebApi.ViewModels.Venta
{
    public class RequestVenta
    {
        public string IdTipoComprobante { get; set; }
        public string NroSerie { get; set; }
        public string NroDocumento { get; set; }
        public string IdCliente { get; set; }
        public string IdMoneda { get; set; }
        public string FecDocumento { get; set; }
        public string HorDocumento { get; set; }
        public string FecVencimiento { get; set; }
        public string ObsVenta { get; set; }
        public decimal TotBruto { get; set; }
        public decimal TotDescuento { get; set; }
        public decimal TotImpuesto { get; set; }
        public decimal TotVenta { get; set; }
        public decimal TasDescuento { get; set; }
        public string IdTipoPago { get; set; }
        public string IdTipoCondicionPago { get; set; }
        public decimal Abono { get; set; }
        public decimal Saldo { get; set; }
        public string IdCajaCa { get; set; }
        public string IdUsuarioCa { get; set; }
        public int CorrelativoCa { get; set; }
        public List<RequestVentaDetalle> DetalleVenta { get; set; }
    }

    public class RequestVentaDetalle
    {
        public string IdArticulo { get; set; }
        public decimal Cantidad { get; set; }
        public string IdUm { get; set; }
        public decimal NroFactor { get; set; }
        public decimal PrecioBase { get; set; }
        public decimal TasDescuento { get; set; }
        public decimal TasIgv { get; set; }
        public decimal Importe { get; set; }
    }
}
