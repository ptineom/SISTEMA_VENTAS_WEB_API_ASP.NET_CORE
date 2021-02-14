using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaVentas.WebApi.ViewModels.Seguridad
{
    public class RequestUsuarioSucursalViewModel
    {
        [Required]
        public string idSucursal { get; set; }
        [Required]
        public string nomSucursal { get; set; }
        [Required]
        public string idUsuario { get; set; }
        [Required]
        public string password { get; set; }
    }
}
