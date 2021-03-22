using CapaDao;
using Entidades;
using Helper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace CapaNegocio
{
    public class BrAplicacion
    {
        IConfiguration _configuration = null;
        Conexion _conexion = null;
        DaoAplicacion _dao = null;
        ResultadoOperacion _Resultado = null;

        public BrAplicacion(IConfiguration configuration)
        {
            _dao = new DaoAplicacion();
            _Resultado = new ResultadoOperacion();
            _configuration = configuration;
            _conexion = new Conexion(_configuration);
        }

        public ResultadoOperacion GetMenuByUserId(string idUsuario)
        {
            List<APLICACION> lista = null;
            using (SqlConnection con = new SqlConnection(_conexion.getConexion))
            {
                try
                {
                    con.Open();
                    lista = _dao.GetMenuByUserId(con, idUsuario);
                    if (lista != null)
                    {
                        //Agregado el menú home
                        lista.Insert(1, new APLICACION()
                        {
                            ID_APLICACION_PADRE = 1,
                            ID_APLICACION = lista.Select(x => x.ID_APLICACION).Max(),
                            NOM_APLICACION = "Home",
                            FLG_FORMULARIO = true,
                            ICON_SPA = "mdi-home",
                            ROUTE_SPA = "Home",
                            FLG_RAIZ = false,
                            BREADCRUMS = "Home",
                            FLG_HOME = true
                        });
                    }
                    _Resultado.SetResultado(true, "", lista);
                }
                catch (Exception ex)
                {
                    _Resultado.SetResultado(false, ex.Message);
                    Elog.save(this, ex);
                }
            }
            return _Resultado;
        }
    }
}
