using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioWebApi.SistemaVentas.Models.Request
{
    public class ArticuloRequest
    {
        public string Accion { get; set; }
        public string IdArticulo { get; set; }
        public IFormFileCollection FileBinary { get; set; }
        public string NomArticulo { get; set; }
        public string NomVenta { get; set; }
        public decimal PrecioBase { get; set; }
        public decimal PrecioCompra { get; set; }
        public string IdGrupo { get; set; }
        public string IdFamilia { get; set; }
        public int IdMarca { get; set; }
        public string CodigoBarra { get; set; }
        public bool FlgImportado { get; set; }
        public bool FlgInactivo { get; set; }
        public decimal StockMinimo { get; set; }
        public string ArticulosUm { get; set; }
        public string Sucursales { get; set; }
    }

    public class RequestArticuloUm
    {
        public string IdUm { get; set; }
        public decimal NroFactor { get; set; }
        public int NroOrden { get; set; }
        public bool FlgPromocion { get; set; }
        public decimal Descuento1 { get; set; }
        public string FecInicioPromocion { get; set; }
        public string FecFinalPromocion { get; set; }
    }
}
