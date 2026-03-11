using System.Data;
using System.Data.SqlClient;

namespace Lab4_FeltIS
{
    internal static class Db
    {
        // ВСТАВЬ ПАРОЛЬ САМ ЛОКАЛЬНО (в чат не пиши)
        public const string ConnStr =
            "Data Source=tcp:10.193.135.143,1433;Initial Catalog=CourseProject_Lab3;User ID=sa;Password=StrongPass123!;TrustServerCertificate=True";

        public static DataTable Query(string sql)
        {
            using (var cn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, cn))
            using (var da = new SqlDataAdapter(cmd))
            {
                cn.Open();
                var dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }
    }
}