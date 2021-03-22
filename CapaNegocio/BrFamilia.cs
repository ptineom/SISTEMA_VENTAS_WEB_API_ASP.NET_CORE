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
    public class BrFamilia
    {
        private DaoFamilia _dao = null;
        private ResultadoOperacion _resultado = null;
        private IConfiguration _configuration = null;
        private Conexion _conexion = null;
        public BrFamilia(IConfiguration configuration)
        {
            _configuration = configuration;
            _dao = new DaoFamilia();
            _resultado = new ResultadoOperacion();
            _conexion = new Conexion(_configuration);
        }
        public ResultadoOperacion GetAllByGroupId(string idGrupo)
        {
            using (SqlConnection con = new SqlConnection(_conexion.getConexion))
            {
                try
                {
                    con.Open();
                    var lista = _dao.GetAllByGroupId(con, idGrupo);
                  
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
        public ResultadoOperacion GetAllByGroupIdHelper(string idGrupo)
        {
            using (SqlConnection con = new SqlConnection(_conexion.getConexion))
            {
                try
                {
                    con.Open();
                    var lista = _dao.GetAllByGroupIdHelper(con, idGrupo);
                    if (lista != null)
                        lista.ForEach(x => x.NOM_FAMILIA = ViewHelper.capitalizeFirstLetter(x.NOM_FAMILIA));

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
        public ResultadoOperacion Register(FAMILIA oModelo)
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
        public ResultadoOperacion Delete(string idGrupo, string idFamilia, string idUsuario)
        {
            SqlTransaction trx = null;
            using (SqlConnection con = new SqlConnection(_conexion.getConexion))
            {
                try
                {
                    con.Open();
                    trx = con.BeginTransaction();
                    _dao.Delete(con, trx, idGrupo, idFamilia, idUsuario);
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
