using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioWebApi.SistemaVentas.Models.Request
{
    public class RecuperarContraseniaRequest
    {
        [Required(ErrorMessage = "Debe de ingresar el {0}")]
        [EmailAddress(ErrorMessage = "Formato de {0} mal ingresado")]
        public string Email { get; set; }
    }
}
