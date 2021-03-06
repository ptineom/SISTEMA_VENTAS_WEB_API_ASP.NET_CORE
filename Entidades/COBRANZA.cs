﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entidades
{
    public class COBRANZA
    {
        public string ACCION { get; set; }
        public string ID_SUCURSAL { get; set; }
        public string ID_TIPO_COMPROBANTE { get; set; }
        public string NRO_SERIE { get; set; }
        public int NRO_DOCUMENTO { get; set; }
        public string ID_CLIENTE { get; set; }
        public string ID_MONEDA { get; set; }
        public decimal TOT_SALDO { get; set; }
        public decimal TOT_ABONO { get; set; }
        public string FEC_DOCUMENTO { get; set; }
        public string FEC_VENCIMIENTO { get; set; }
        public string ID_USUARIO_REGISTRO { get; set; }
        public string ID_CAJA_CA { get; set; }
        public string ID_USUARIO_CA { get; set; }
        public int CORRELATIVO_CA { get; set; }
        //
        public int CORRELATIVO { get; set; }
        public string ID_TIPO_PAGO { get; set; }
        public decimal TOT_COBRANZA { get; set; }
        public string FEC_COBRANZA { get; set; }
        //
        public int SECUENCIA { get; set; }
        public string ESTADO_PAGO { get; set; }
        public string COMPROBANTE { get; set; }
        public string DOC_CLIENTE { get; set; }
        public string NOM_CLIENTE { get; set; }
        public string SGN_MONEDA { get; set; }
        //
        public List<TIPO_DOCUMENTO> listaDocumentos { get; set; }
        public List<TIPO_COMPROBANTE> listaComprobantes { get; set; }
        public List<TIPO_PAGO> listaTipPag { get; set; }
    }
}
