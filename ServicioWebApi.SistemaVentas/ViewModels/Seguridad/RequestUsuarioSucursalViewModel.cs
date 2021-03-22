using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaVentas.WebApi.ViewModels.Seguridad
{
    public class RequestUsuarioSucursalViewModel
    {
        [Required(ErrorMessage ="Debe ingresar el {0}")]
        public string IdSucursal { get; set; }
        [Required(ErrorMessage = "Debe ingresar el {0}")]
        public string NomSucursal { get; set; }
        [Required(ErrorMessage = "Debe de ingresar el {0}")]
        [Display(Name = "usuario")]
        public string IdUsuario { get; set; }
        [Required(ErrorMessage = "Debe de ingresar el {0}")]
        public string Password { get; set; }
    }

    public class RequestCambiarSucursalViewModel
    {
        [Required(ErrorMessage = "Debe ingresar el {0}")]
        public string IdSucursal { get; set; }
        [Required(ErrorMessage = "Debe ingresar el {0}")]
        public string NomSucursal { get; set; }
    }
}
