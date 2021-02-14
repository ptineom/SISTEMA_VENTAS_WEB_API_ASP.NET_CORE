using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaVentas.WebApi.ViewModels
{
    public class TokensViewModel
    {
        public string accessToken { get; set; }
        public string refreshToken { get; set; }
    }
}
