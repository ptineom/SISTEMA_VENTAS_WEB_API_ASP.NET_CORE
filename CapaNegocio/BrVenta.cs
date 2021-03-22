using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades;
using CapaDao;
using System.Data.SqlClient;
using Helper;
using Microsoft.Extensions.Configuration;

namespace CapaNegocio
{
    public class BrVenta
    {
        private DaoVenta _dao = null;
        private ResultadoOperacion _resultado = null;
        private IConfiguration _configuration = null;
        private Conexion _conexion = null;

        public BrVenta(IConfiguration configuration)
        {
            _dao = new DaoVenta();
            _resultado = new ResultadoOperacion();
            _configuration = configuration;
            _conexion = new Conexion(_configuration);
        }
        #region Proceso ventas
        public ResultadoOperacion GetData(string idSucursal, string idUsuario, ref string tipoNCAnular)
        {
            using (SqlConnection con = new SqlConnection(_conexion.getConexion))
            {
                try
                {
                    con.Open();
                    DOC_VENTA modelo = _dao.GetData(con, idSucursal, idUsuario, ref tipoNCAnular);
                    _resultado.SetResultado(true, modelo);
                }
                catch (Exception ex)
                {
                    Elog.save(this, ex);
                    _resultado.SetResultado(false, ex.Message);
                }
            }
            return _resultado;
        }

        public ResultadoOperacion Register(DOC_VENTA oModelo)
        {
            SqlTransaction trx = null;
            using (SqlConnection con = new SqlConnection(_conexion.getConexion))
            {
                try
                {
                    con.Open();
                    trx = con.BeginTransaction();

                    string serie = string.Empty;
                    string documento = string.Empty;
                    _dao.Register(con, trx, oModelo, ref serie, ref documento);

                    _resultado.SetResultado(true, Helper.Constantes.sMensajeGrabadoOk, new
                    {
                        serie = serie,
                        documento = ViewHelper.GetNroComprobante(documento)
                    });

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

        public ResultadoOperacion GetAllByFilters(string idSucursal, string idCliente, string idTipoComprobante,
            string nroSerie, int nroDocumento, string fechaInicio, string fechaFinal, int idEstado)
        {
            using (SqlConnection con = new SqlConnection(_conexion.getConexion))
            {
                try
                {
                    con.Open();
                    List<DOC_VENTA> lista = _dao.GetAllByFilters(con, idSucursal, idCliente, idTipoComprobante, nroSerie, nroDocumento, fechaInicio, fechaFinal, idEstado);

                    _resultado.SetResultado(true, "", lista);
                }
                catch (Exception ex)
                {
                    Elog.save(this, ex);
                    _resultado.SetResultado(false, ex.Message);
                }
            }
            return _resultado;
        }

        public ResultadoOperacion GetById(string idSucursal, string idTipoComprobante, string nroSerie, int nroDocumento)
        {
            using (SqlConnection con = new SqlConnection(_conexion.getConexion))
            {
                try
                {
                    con.Open();
                    DOC_VENTA modelo = _dao.GetById(con, idSucursal, idTipoComprobante, nroSerie, nroDocumento);
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

        public ResultadoOperacion Delete(string idSucursal, string idTipoComprobante,
            string nroSerie, int nroDocumento, string idUsuario, ref string nroSerieNC, ref int nroDocumentoNC)
        {
            SqlTransaction trx = null;
            using (SqlConnection con = new SqlConnection(_conexion.getConexion))
            {
                try
                {
                    con.Open();
                    trx = con.BeginTransaction();

                    _dao.anularVenta(con, trx, idSucursal, idTipoComprobante, nroSerie, nroDocumento, idUsuario, ref nroSerieNC, ref nroDocumentoNC);

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
        #endregion

        #region Consultas y reportes
        public ResultadoOperacion combosReportesVentas(string idSucursal)
        {
            using (SqlConnection con = new SqlConnection(_conexion.getConexion))
            {
                try
                {
                    con.Open();
                    DOC_VENTA modelo = _dao.combosReportesVentas(con, idSucursal);
                    if (modelo != null)
                    {
                        _resultado.Data = modelo;
                    }
                    _resultado.SetResultado(true, "");
                }
                catch (Exception ex)
                {
                    Elog.save(this, ex);
                    _resultado.SetResultado(false, ex.Message);
                }
            }
            return _resultado;
        }

        public ResultadoOperacion consultaVentas(string idSucursal, string idCliente, string idTipoComprobante,
            string fechaInicio, string fechaFinal, string idTipoCondicionPago, string idUsuarioCaja)
        {
            using (SqlConnection con = new SqlConnection(_conexion.getConexion))
            {
                try
                {
                    con.Open();
                    var lista = _dao.consultaVentas(con, idSucursal, idCliente, idTipoComprobante, fechaInicio, fechaFinal, idTipoCondicionPago, idUsuarioCaja);
                    if (lista != null)
                    {
                        _resultado.Data = lista;// lista.ToList<Object>();
                    }
                    _resultado.SetResultado(true, "");
                }
                catch (Exception ex)
                {
                    Elog.save(this, ex);
                    _resultado.SetResultado(false, ex.Message);
                }
            }
            return _resultado;
        }

        public ResultadoOperacion consultaVentasDetalle(string idSucursal, string idTipoComprobante, string nroSerie,
            int nroDocumento)
        {
            using (SqlConnection con = new SqlConnection(_conexion.getConexion))
            {
                try
                {
                    con.Open();
                    var lista = _dao.consultaVentasDetalle(con, idSucursal, idTipoComprobante, nroSerie, nroDocumento);
                    if (lista != null)
                    {
                        _resultado.Data = lista;// lista.ToList<Object>();
                    }
                    _resultado.SetResultado(true, "");
                }
                catch (Exception ex)
                {
                    Elog.save(this, ex);
                    _resultado.SetResultado(false, ex.Message);
                }
            }
            return _resultado;
        }

        public ResultadoOperacion consultaVentasPorUsuario(string idSucursal, string fechaInicio, string fechaFinal, string tipo)
        {
            using (SqlConnection con = new SqlConnection(_conexion.getConexion))
            {
                try
                {
                    con.Open();
                    var lista = _dao.consultaVentasPorUsuario(con, idSucursal, fechaInicio, fechaFinal, tipo);
                    if (lista != null)
                    {
                        _resultado.Data = lista;// lista.ToList<Object>();
                    }
                    _resultado.SetResultado(true, "");
                }
                catch (Exception ex)
                {
                    Elog.save(this, ex);
                    _resultado.SetResultado(false, ex.Message);
                }
            }
            return _resultado;
        }

        public ResultadoOperacion consultaVentasPorMes(string idSucursal, int anio)
        {
            using (SqlConnection con = new SqlConnection(_conexion.getConexion))
            {
                try
                {
                    con.Open();
                    var lista = _dao.consultaVentasPorMes(con, idSucursal, anio);
                    if (lista != null)
                    {
                        _resultado.Data = lista;// lista.ToList<Object>();
                    }
                    _resultado.SetResultado(true, "");
                }
                catch (Exception ex)
                {
                    Elog.save(this, ex);
                    _resultado.SetResultado(false, ex.Message);
                }
            }
            return _resultado;
        }
        #endregion
    }
}
