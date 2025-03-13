using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using static D0004N.Schema;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Data;
using System.Reflection.PortableExecutable;

namespace D0004N
{
    static class Transactor
    {
        const string DB = "Server=localhost;Database=D0004N;Trusted_Connection=True;TrustServerCertificate=True";

        // ============= BIL =============
        public static async Task<List<(string RegNr, int BilTyp, string StatusText)>> QueryBilWithStatus()
        {
            var result = new List<(string, int, string)>();
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();
                string sql = @"SELECT b.RegNr, b.BilTyp,
                CASE 
                WHEN bk.MaxBokningsId IS NOT NULL THEN 'Uthyrd'
                ELSE 'Tillgänglig'
                END AS StatusText
                FROM Bil b
                LEFT JOIN (
                SELECT RegNr, MAX(BokningsId) AS MaxBokningsId
                FROM BokningBil
                WHERE GETDATE() >= StartDatum
                AND (SlutDatum IS NULL OR GETDATE() <= SlutDatum)
                GROUP BY RegNr
                ) bk ON b.RegNr = bk.RegNr;

                ";

                using var cmd = new SqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    string regNr = reader["RegNr"].ToString()!;
                    int biltyp = Convert.ToInt32(reader["BilTyp"]);
                    string status = reader["StatusText"].ToString()!;

                    result.Add((regNr, biltyp, status));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return result;
            }
            return result;
        }

        public static async Task<bool> NonQueryBil(string RegNr, int BilTyp)
        {
            try
            {
                using (var conn = new SqlConnection(DB))
                {
                    await conn.OpenAsync();
                    
                    string transaction = @"INSERT INTO dbo.Bil (RegNr, BilTyp) VALUES (@RegNr, @BilTyp)";

                    using (var cmd = new SqlCommand(transaction, conn))
                    {
                        cmd.Parameters.AddWithValue("@RegNr", RegNr);
                        cmd.Parameters.AddWithValue("@BilTyp", BilTyp);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}");
                return false;
            }
            return true;
        }

        public static async Task<bool> NonQueryBilStation(string regNr, int stationId)
        {
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = @"INSERT INTO BilStation (RegNr, StationId) VALUES (@R, @S);";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@R", regNr);
                cmd.Parameters.AddWithValue("@S", stationId);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        public static async Task<bool> QueryBilTyp(int bilTyp)
        {
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();
                string sql = @"SELECT COUNT(*) FROM BilTyp WHERE BilTyp = @B;";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@B", bilTyp);

                int count = (int)await cmd.ExecuteScalarAsync();
                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public static async Task<bool> NonQueryBilTyp(int bilTyp, decimal krDygn)
        {
            decimal krDygnHelg = krDygn * 0.5m; // WeekendKampanj!!! :-]]]]]]]]]]]]]] $$$$$$$$$$$$$$$$$
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = @"INSERT INTO BilTyp (BilTyp, KrDygn, KrDygnHelg)
                           VALUES (@BT, @K, @KH);";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@BT", bilTyp);
                cmd.Parameters.AddWithValue("@K", krDygn);
                cmd.Parameters.AddWithValue("@KH", krDygnHelg);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        public static async Task<BiltypDto> QueryBiltypByRegNr(string regNr)
        {
            BiltypDto result = null;

            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = @"SELECT bt.BilTyp,
                                      bt.KrDygn,
                                      bt.KrDygnHelg
                            FROM Bil b
                            INNER JOIN BilTyp bt ON b.BilTyp = bt.BilTyp
                            WHERE b.RegNr = @R
                ";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@R", regNr);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    result = new BiltypDto
                    {
                        BilTypId = reader.GetInt32(reader.GetOrdinal("BilTyp")),
                        KrDygn = reader.GetDecimal(reader.GetOrdinal("KrDygn")),
                        KrHelg = reader.GetDecimal(reader.GetOrdinal("KrDygnHelg"))
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

            return result;
        }


        // ============= KUND =============
        public static async Task<bool> CheckIfKundExists(string personnummer)
        {
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = @"SELECT COUNT(*) FROM Kunder WHERE Personnummer = @Pnr;";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@Pnr", SqlDbType.NChar, 12).Value = personnummer.Trim();

                int count = (int)await cmd.ExecuteScalarAsync();
                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }


        public static async Task<List<Kunder>> QueryAllKunder()
        {
            var result = new List<Kunder>();
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = @"SELECT * FROM Kunder;";
                using var cmd = new SqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var Kunder = new Schema.Kunder
                    {
                        KundId = reader.GetInt32(reader.GetOrdinal("KundId")),
                        Personnummer = reader.GetString(reader.GetOrdinal("Personnummer"))
                    };
                    result.Add(Kunder);
                }
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public static async Task<bool> NonQueryKund(string fNamn, string eNamn, string pnr, string? orgNr)
        {
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = @"INSERT INTO Kund (ForNamn, EfterNamn, Personnummer, OrgNr)
                           VALUES (@F, @E, @P, @O);";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@F", fNamn);
                cmd.Parameters.AddWithValue("@E", eNamn);
                cmd.Parameters.AddWithValue("@P", pnr);
                cmd.Parameters.AddWithValue("@O", (object?)orgNr ?? DBNull.Value);
                

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        public static async Task<int> NonQueryKunder(string personnummer)
        {
            int lastId = 0;
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();


                string read = @"SELECT ISNULL(MAX(KundId), 0) AS MaxKundId FROM Kunder;";
                using var cmdRead = new SqlCommand(read, conn);
                int maxId = (int)await cmdRead.ExecuteScalarAsync();

                lastId = maxId + 1;

                string sql = @"INSERT INTO Kunder (KundId, Personnummer) 
                           VALUES (@Id, @Pnr);";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", lastId);
                cmd.Parameters.AddWithValue("@Pnr", personnummer);



                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
            return lastId;
        }

        public static async Task<int> QueryKunder(string personnummer)
        {
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string query = "SELECT KundId FROM Kunder WHERE Personnummer = @Pnr;";
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Pnr", personnummer);

                var result = await cmd.ExecuteScalarAsync();
                if (result != null && int.TryParse(result.ToString(), out int kundId))
                {
                    return kundId;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
        }



        // ============= FÖRETAG =============
        public static async Task<bool> CheckIfForetagExists(string orgNr)
        {
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = @"SELECT COUNT(*) FROM Foretag WHERE OrgNr = @O;";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@O", orgNr);

                int count = (int)await cmd.ExecuteScalarAsync();
                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public static async Task<bool> NonQueryForetag(string orgNr, string namn, string adr)
        {
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = @"INSERT INTO Foretag (OrgNr, ForetagsNamn, Adress)
                           VALUES (@O, @N, @A);";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@O", orgNr);
                cmd.Parameters.AddWithValue("@N", namn);
                cmd.Parameters.AddWithValue("@A", adr);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        // ------------- STATION -------------

        public static async Task<bool> NonQueryStation(int stationId, string adress)
        {
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = @"INSERT INTO Station (StationId, Adress)
                       VALUES (@Id, @Adr);";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", stationId);
                cmd.Parameters.AddWithValue("@Adr", adress);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        public static async Task<List<Schema.Station>> QueryStation()
        {
            var result = new List<Schema.Station>();
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = "SELECT StationId, Adress FROM Station;";

                using var cmd = new SqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var station = new Schema.Station
                    {
                        StationId = reader.GetInt32(0),
                        Adress = reader.GetString(1)
                    };
                    result.Add(station);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return result;
            }
            return result;
        }


        // ============= BOKNING =============


        /// <summary>
        /// Skapa ny PK BOkID + insert.
        /// </summary>
        /// <param name="kundId"></param>
        /// <returns>New BokningsId</returns>
        public static async Task<int> NonQueryBokningKund(int kundId)
        {
            int lastId = 0; 
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string read = @"SELECT ISNULL(MAX(BokningsId), 0) AS MaxBokningsId FROM BokningKund;";
                using var cmdRead = new SqlCommand(read, conn);
                int maxId = (int)await cmdRead.ExecuteScalarAsync();
                lastId = maxId + 1;

                string sql = @"INSERT INTO BokningKund (BokningsId, KundId)VALUES (@BId, @KId);";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@BId", lastId);
                cmd.Parameters.AddWithValue("@KId", kundId);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
            return lastId;
        }


        /// <summary>
        /// Insätt refererande Composite Key i BokningBil -> BokningKund
        /// </summary>
        /// <param name="regNr"></param>
        /// <param name="start"></param>
        /// <param name="slut"></param>
        /// <param name="kundId"></param>
        /// <returns>OK/NO</returns>
        public static async Task<bool> NonQueryBokningBil(List<string> regNr, int bokId, DateTime start, DateTime? slut)
        {
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                foreach (string rnr in regNr)
                {
                    string sql = @"INSERT INTO BokningBil (BokningsId, RegNr, StartDatum, SlutDatum) 
                           VALUES (@Id, @Reg, @St, @End);";

                    using var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Id", bokId);
                    cmd.Parameters.AddWithValue("@Reg", rnr);
                    cmd.Parameters.AddWithValue("@St", start);
                    cmd.Parameters.Add("@End", SqlDbType.DateTime).Value = (object)slut ?? DBNull.Value;
                    await cmd.ExecuteNonQueryAsync();
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }




        // ============= FAKTURA =============
        public static async Task<bool> NonQueryFaktura(long FakturaId, int bokningsId, decimal belopp, DateTime datum, DateTime forfDatum, bool status)
        {
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                

                string sql = @"INSERT INTO Faktura (FakturaNr, FakturaDatum, Belopp, ForfalloDatum, Status)
                           VALUES (@Fid, @Date, @Belopp, @Fdate, @Ok);";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Fid", FakturaId);
                cmd.Parameters.AddWithValue("@Date", datum);
                cmd.Parameters.AddWithValue("@Belopp", belopp);
                cmd.Parameters.AddWithValue("@Fdate", forfDatum);
                cmd.Parameters.AddWithValue("@Ok", status);

                await cmd.ExecuteNonQueryAsync();

                string sqlJunction = @"INSERT INTO BokningFaktura (FakturaNr, BokningsId)
                                    VALUES (@F, @B);";

                using var cmdnew = new SqlCommand(sqlJunction, conn);
                cmdnew.Parameters.AddWithValue("@F", FakturaId);
                cmdnew.Parameters.AddWithValue("@B", bokningsId);
                await cmdnew.ExecuteNonQueryAsync();
                conn.Close();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        public static async Task<List<BokningBilDto>> QueryBokningBil(int bokId)
        {
            var result = new List<BokningBilDto>();
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = @"
            SELECT RegNr, StartDatum, SlutDatum
            FROM BokningBil
            WHERE BokningsId = @BID;";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@BID", bokId);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var dto = new BokningBilDto
                    {
                        RegNr = reader["RegNr"].ToString(),
                        StartDatum = (DateTime)reader["StartDatum"],
                        SlutDatum = reader["SlutDatum"] == DBNull.Value
                                    ? (DateTime?)null
                                    : (DateTime)reader["SlutDatum"]
                    };
                    result.Add(dto);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="kundId"></param>
        /// <returns>Alla BokningsID för en kund</returns>
        public static async Task<List<int>> QueryBokningarForKund(int kundId)
        {
            var result = new List<int>();
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = @"SELECT BokningsId FROM BokningKund WHERE KundId = @Kid;";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Kid", kundId);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(reader.GetInt32(0));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return result;
        }

        public static async Task<bool> UpdateSlutDatum(int bokId, List<string> regNrList, DateTime? newSlut)
        {
            if (regNrList == null || regNrList.Count == 0) return true;

            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                foreach (var reg in regNrList)
                {
                    string sql = @"UPDATE BokningBil 
                           SET SlutDatum = @Slut
                           WHERE BokningsId = @BID AND RegNr = @Reg;";
                    using var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@Slut", SqlDbType.DateTime).Value = (object)newSlut ?? DBNull.Value;
                    cmd.Parameters.AddWithValue("@BID", bokId);
                    cmd.Parameters.AddWithValue("@Reg", reg);

                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }


        // ============== AVTAL ===============
        public static async Task<bool> NonQueryAvtal(int anstId, int bokId, bool? status, string? filepath)
        {
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = @"INSERT INTO Avtal (AnstallningsId, BokningsId, Filepath)
                       VALUES (@A, @B, @F);";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@A", anstId);
                cmd.Parameters.AddWithValue("@B", bokId);
                cmd.Parameters.AddWithValue("@F", (object?)filepath ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        // ============= PERSONAL ==============
        public static async Task<bool> NonQueryPersonal(int anstId, string fNamn, string eNamn)
        {
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = @"INSERT INTO Anstalld (AnstallningsId, Fornamn, Efternamn)
                       VALUES (@Id, @F, @E);";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", anstId);
                cmd.Parameters.AddWithValue("@F", fNamn);
                cmd.Parameters.AddWithValue("@E", eNamn);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        public static async Task<bool> QueryPersonalScalar(int anstId)
        {
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = @"SELECT COUNT(*) FROM Anstalld WHERE AnstallningsId = @Id;";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", anstId);

                int count = (int)await cmd.ExecuteScalarAsync();
                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        // ============= KONTROLL =================

        public static async Task<bool> NonQueryKontroll(string regNr, int bokId, int anstId, int matarSt)
        {
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = @"INSERT INTO Kontroll (RegNr, BokningsId, AntallningsId, MatarStallning)
                       VALUES (@Reg, @BId, @AId, @Mst);";

                using var cmd = new SqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@Reg", regNr);
                cmd.Parameters.AddWithValue("@BId", bokId);
                cmd.Parameters.AddWithValue("@AId", anstId);
                cmd.Parameters.AddWithValue("@Mst", matarSt);

                await cmd.ExecuteNonQueryAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey(); // 
                return false;
            }
        }

        public static async Task<int> NonQuerySkada(string regNr, int bokId, DateTime? dtFixd, string? desc)
        {
            int lastId = 0;
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string read = @"SELECT ISNULL(MAX(SkadaId), 0) AS MaxSkadaId FROM Skada;";
                using var cmdRead = new SqlCommand(read, conn); 
                int maxId = (int)await cmdRead.ExecuteScalarAsync();
                lastId = maxId + 1; 

                string sql = @"INSERT INTO Skada (SkadaId, BokningsId, RegNr, DatumFixad, Beskrivning)
                                VALUES (@SId, @BId, @Reg, @DFx, @Dsc);";  
                using var cmd = new SqlCommand(sql, conn);    
                
                cmd.Parameters.AddWithValue("@SId", lastId);           
                cmd.Parameters.AddWithValue("@BId", bokId);
                cmd.Parameters.AddWithValue("@Reg", regNr);
                cmd.Parameters.AddWithValue("@DFx", (object?)dtFixd ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Dsc", (object?)desc ?? DBNull.Value);
                await cmd.ExecuteNonQueryAsync();         
            }         
            catch (Exception ex)              
            {      
                Console.WriteLine(ex.Message);                    
                return -1;
      
            }
            return lastId;   
        }
    }
}
