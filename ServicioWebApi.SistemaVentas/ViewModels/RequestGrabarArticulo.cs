using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaVentas.WebApi.ViewModels
{
    public class RequestGrabarArticulo
    {
        public string accion { get; set; }
        public string idArticulo { get; set; }
        public IFormFileCollection fileBinary { get; set; }
        public string nomArticulo { get; set; }
        public string nomVenta { get; set; }
        public decimal precioBase { get; set; }
        public decimal precioCompra { get; set; }
        public string idGrupo { get; set; }
        public string idFamilia { get; set; }
        public int idMarca { get; set; }
        public string codigoBarra { get; set; }
        public bool flgImportado { get; set; }
        public bool flgInactivo { get; set; }
        public decimal stockMinimo { get; set; }
        public string articulosUm { get; set; }
        public string sucursales { get; set; }
    }

    public class RequestArticuloUm
    {
        public string idUm { get; set; }
        public decimal nroFactor { get; set; }
        public int nroOrden { get; set; }
        public bool flgPromocion { get; set; }
        public decimal descuento1 { get; set; }
        public string fecInicioPromocion { get; set; }
        public string fecFinalPromocion { get; set; }
    }
}
