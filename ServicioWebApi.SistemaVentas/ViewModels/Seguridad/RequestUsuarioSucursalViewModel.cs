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
        public string idSucursal { get; set; }
        [Required(ErrorMessage = "Debe ingresar el {0}")]
        public string nomSucursal { get; set; }
        [Required(ErrorMessage = "Debe de ingresar el {0}")]
        [Display(Name = "usuario")]
        public string idUsuario { get; set; }
        [Required(ErrorMessage = "Debe de ingresar el {0}")]
        public string password { get; set; }
    }

    public class RequestCambiarSucursalViewModel
    {
        [Required(ErrorMessage = "Debe ingresar el {0}")]
        public string idSucursal { get; set; }
        [Required(ErrorMessage = "Debe ingresar el {0}")]
        public string nomSucursal { get; set; }
    }
}
