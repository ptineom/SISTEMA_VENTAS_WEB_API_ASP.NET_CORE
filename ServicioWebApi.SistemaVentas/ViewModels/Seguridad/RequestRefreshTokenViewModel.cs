﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaVentas.WebApi.ViewModels.Seguridad
{
    public class RequestRefreshTokenViewModel
    {
        [Required]
        public string idRefreshToken { get; set; }
    }
}