using Entidades;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CapaDao
{
    public class DaoFamiliaUm
    {
        public List<UNIDAD_MEDIDA> GetAllByFamilyId(SqlConnection con, string idGrupo, string idFamilia)
        {
            List<UNIDAD_MEDIDA> lista = null;
            UNIDAD_MEDIDA modelo = null;
            using (SqlCommand cmd = new SqlCommand("PA_MANT_FAMILIA_UM", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@ACCION", SqlDbType.VarChar, 3).Value = "GET";
                cmd.Parameters.Add("@ID_GRUPO", SqlDbType.VarChar, 2).Value = idGrupo;
                cmd.Parameters.Add("@ID_FAMILIA", SqlDbType.VarChar, 3).Value = idFamilia;
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader != null)
                {
                    if (reader.HasRows)
                    {
                        lista = new List<UNIDAD_MEDIDA>();
                        while (reader.Read())
                        {
                            modelo = new UNIDAD_MEDIDA();
                            modelo.ID_UM = reader.GetString(reader.GetOrdinal("ID_UM"));
                            modelo.NOM_UM = reader.GetString(reader.GetOrdinal("NOM_UM"));
                            lista.Add(modelo);
                        }
                    }
                }
                reader.Close();
                reader.Dispose();
            }
            return lista;
        }
    }
}
