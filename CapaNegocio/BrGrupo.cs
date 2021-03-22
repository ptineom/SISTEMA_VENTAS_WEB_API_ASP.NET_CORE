using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades;
using CapaDao;
using Helper;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace CapaNegocio
{
    public class BrGrupo
    {
        IConfiguration _configuration = null;
        DaoGrupo _dao = null;
        ResultadoOperacion _resultado = null;
        Conexion _conexion = null;

        public BrGrupo(IConfiguration configuration)
        {
            _dao = new DaoGrupo();
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
                   List<GRUPO> lista = _dao.GetAll(con);
                    if (lista != null)
                        lista.ForEach( x => x.NOM_GRUPO = ViewHelper.capitalizeFirstLetter(x.NOM_GRUPO));

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
        public ResultadoOperacion Register(GRUPO oModelo)
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
        public ResultadoOperacion Delete(string idGrupo, string idUsuario)
        {
            SqlTransaction trx = null;
            using (SqlConnection con = new SqlConnection(_conexion.getConexion))
            {
                try
                {
                    con.Open();
                    trx = con.BeginTransaction();
                    _dao.Delete(con, trx, idGrupo, idUsuario);
                    _resultado.SetResultado(true, Helper.Constantes.sMensajeEliminadoOk);
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
    }
}
