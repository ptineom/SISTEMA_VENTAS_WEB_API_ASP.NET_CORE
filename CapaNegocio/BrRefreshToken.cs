using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using CapaDao;
using Entidades;
using Helper;
using Microsoft.Extensions.Configuration;

namespace CapaNegocio
{
    /// <summary>
    /// 
    /// </summary>
    public class BrRefreshToken
    {
        DaoRefreshToken _dao = null;
        ResultadoOperacion _resultado = null;
        IConfiguration _configuration = null;
        Conexion _conexion = null;

        public BrRefreshToken(IConfiguration configuration)
        {
            _dao = new DaoRefreshToken();
            _resultado = new ResultadoOperacion();
            _configuration = configuration;
            _conexion = new Conexion(_configuration);
        }

        public ResultadoOperacion Register(REFRESH_TOKEN oModelo)
        {
            SqlTransaction trx = null;
            using (SqlConnection con = new SqlConnection(_conexion.getConexion))
            {
                try
                {
                    con.Open();
                    trx = con.BeginTransaction();
                    _dao.Register(con, trx, oModelo);
                    _resultado.SetResultado(true, Helper.Constantes.sMensajeGrabadoOk);
                    trx.Commit();
                }
                catch (Exception ex)
                {
                    _resultado.SetResultado(false, ex.Message.ToString());
                    trx.Rollback();
                    Elog.save(this, ex);
                }
            }
            return _resultado;
        }

        public ResultadoOperacion GetById(string idRefreshToken)
        {
            using (SqlConnection con = new SqlConnection(_conexion.getConexion))
            {
                try
                {
                    con.Open();
                    REFRESH_TOKEN modelo = _dao.GetById(con, idRefreshToken);

                    _resultado.SetResultado(true, "", modelo);
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
