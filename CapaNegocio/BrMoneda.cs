using CapaDao;
using Helper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaNegocio
{
    public class BrMoneda
    {
        DaoMoneda _dao = null;
        ResultadoOperacion _resultado = null;
        IConfiguration _configuration = null;
        Conexion _conexion = null;
        public BrMoneda(IConfiguration configuration)
        {
            _dao = new DaoMoneda();
            _resultado = new ResultadoOperacion();
            _configuration = configuration;
            _conexion = new Conexion(_configuration);
        }

        public ResultadoOperacion GetAll()
        {
            using (SqlConnection con = new SqlConnection(_conexion.getConexion))
            {
                try
                {
                    con.Open();
                    var lista = _dao.GetAll(con);

                    _resultado.SetResultado(true, lista);
                }
                catch (Exception ex)
                {
                    Elog.save(this, ex);
                    _resultado.SetResultado(false, ex.Message);
                }
            }
            return _resultado;
        }
    }
}
