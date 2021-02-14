using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaVentas.WebApi.ViewModels.Usuario
{
    public class RequestRecuperarContraseniaViewModel
    {
        [Required(ErrorMessage = "Debe de ingresar el {0}")]
        [EmailAddress(ErrorMessage = "Formato de {0} mal ingresado")]
        public string Email { get; set; }
    }
}
