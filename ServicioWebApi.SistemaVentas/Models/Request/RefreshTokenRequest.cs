using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ServicioWebApi.SistemaVentas.Models.Request
{
    public class RefreshTokenRequest
    {
        [Required]
        public string IdRefreshToken { get; set; }
    }
}
