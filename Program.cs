using System.Data.SqlClient;
using System.Text.RegularExpressions;

namespace D0004N
{


    public class Program
    {
        public static async Task Main(string[] args)
        {
            await Meny();
            Console.WriteLine("Tryck på valfri knapp för att avsluta...");
            Console.ReadKey();
        }

        private static async Task Meny()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Green Rental DB - Huvudmeny");
                Console.WriteLine("1. Registrera bil");
                Console.WriteLine("2. Visa alla bilar + status");
                Console.WriteLine("3. Boka hyrning");
                Console.WriteLine("4. Skapa faktura (test)");
                Console.WriteLine("5. Registrera station");
                Console.WriteLine("6. Visa stationer");
                Console.WriteLine("7. Kontroll av bil"); // Om den är tillgänglig d.v.s. (query bokning eller bokning sort DESC by RegNr). När en kontroll utförs kan vi bestämma om en skada hittas eller ej (J/N - detta skall resultera i automatisk fakturering.
                Console.WriteLine("8. Registrera Personal"); // Ett "avtal" måste signeras för att stödja verksamhetslogiken. För detta krävs en FK till "AnställningsId" i "dbo.Anstalld". 
                Console.WriteLine("q. Avsluta\n");

                var input = Console.ReadLine()?.Trim();

                switch (input)
                {
                    case "1":
                        await RegistreraBil();
                        break;
                    case "2":
                        await VisaAllaBilarMedStatus();
                        break;
                    case "3":
                        await BokaHyrning();
                        break;
                    case "4":
                        await SkapaFaktura();
                        break;
                    case "5":
                        await RegistreraStation();
                        break;
                    case "6":
                        await VisaStationer();
                        break;
                    case "7":
                        // Gör Kontroll
                        break;
                    case "8":
                        await RegistreraPersonal();
                        break;

                    case "q":
                        return;
                    default:
                        Console.WriteLine("Felaktig inmatning.");
                        Console.ReadKey();
                        break;
                }
            }
        }

        /// <summary>
        /// Registrera Station
        /// strikt enligt denna modell
        /// </summary>
        /// <returns></returns>
        private static async Task RegistreraStation()
        {
            Console.WriteLine("Registrera station.\n");
            Console.Write("StationId: ");
            var stationIdStr = Console.ReadLine() ?? "";
            if (!int.TryParse(stationIdStr, out int stationId))
            {
                Console.WriteLine("Felaktig inmatning för StationId. Avbryter...");
                Console.ReadKey();
                return;
            }

            Console.Write("Adress: ");
            var adress = Console.ReadLine() ?? "";

            var success = await Transactor.NonQueryStation(stationId, adress);
            if (!success)
            {
                Console.WriteLine("Fel vid registrering av station (INSERT).");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Station registrerad!");
            Console.ReadKey();
        }

        private static async Task VisaStationer()
        {
            var stationer = await Transactor.QueryStation();
            if (stationer == null || stationer.Count == 0)
            {
                Console.WriteLine("Inga stationer hittades i databasen.");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("StationId | Adress");
            Console.WriteLine("----------------------");
            foreach (var s in stationer)
            {
                Console.WriteLine($"{s.StationId}         {s.Adress}");
            }

            Console.ReadKey();
        }


        /// <summary>
        /// Registrera en ny bil i dbo.Bil (och koppla bil till en station).
        /// </summary>
        private static async Task RegistreraBil()
        {
            Console.WriteLine("Registrera bil.\n");

            Console.Write("StationId: ");
            var stationIdStr = Console.ReadLine() ?? "";
            if (!int.TryParse(stationIdStr, out int stationId))
            {
                Console.WriteLine("Felaktig inmatning av StationID. Avbryter...");
                Console.ReadKey();
                return;
            }

            Console.Write("RegNr: ");
            var regNr = Console.ReadLine() ?? "";

            Console.Write("Biltyp (0–5): ");
            var bilTypString = Console.ReadLine() ?? "";
            if (!int.TryParse(bilTypString, out int bilTyp))
            {
                Console.WriteLine("Felaktig inmatning av Biltyp (0–5). Avbryter...");
                Console.ReadKey();
                return;
            }

            var successStation = await Transactor.NonQueryBilStation(regNr, stationId);
            if (!successStation)
            {
                Console.WriteLine("Fel vid registrering av BilStation (INSERT).");
                Console.ReadKey();
                return;
            }

            var successBil = await Transactor.NonQueryBil(regNr, bilTyp);
            if (!successBil)
            {
                Console.WriteLine("Fel vid registrering av bil (INSERT).");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("BilStation och Bil registrerades korrekt.");
            Console.WriteLine("Tryck valfri tangent för att fortsätta...");
            Console.ReadKey();
        }


        private static async Task RegistreraPersonal()
        {
            Console.WriteLine("Registrera ny personal.");

            Console.Write("AnställningsId (heltal): ");
            var idStr = Console.ReadLine() ?? "";
            if (!int.TryParse(idStr, out int anstId))
            {
                Console.WriteLine("Felaktigt inmatning av AnställningsId.");
                Console.ReadKey();
                return;
            }

            Console.Write("Förnamn: ");
            var fNamn = Console.ReadLine() ?? "";
            Console.Write("Efternamn: ");
            var eNamn = Console.ReadLine() ?? "";

            var success = await Transactor.NonQueryPersonal(anstId, fNamn, eNamn);
            if (!success)
            {
                Console.WriteLine("Fel vid registrering av personal.");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("Personal registrerad!");
            Console.ReadKey();
        }


        /// <summary>
        /// Enkel variant
        /// </summary>
        private static async Task VisaAllaBilarMedStatus()
        {
            var bilLista = await Transactor.QueryBilWithStatus();
            if (bilLista == null || bilLista.Count == 0)
            {
                Console.WriteLine("Inga bilar hittades i databasen.");
            }
            else
            {
                Console.WriteLine("RegNr | BilTyp | Status");
                Console.WriteLine("-----------------------");
                foreach (var b in bilLista)
                {
                    Console.WriteLine($"{b.RegNr} | {b.BilTyp} | {b.StatusText}");
                }
            }

            Console.WriteLine("\nTryck valfri tangent för att fortsätta...");
            Console.ReadKey();
        }

        /// <summary>
        /// Bestäm vad som ska transactioneras lmao
        /// </summary>
        private static async Task BokaHyrning()
        {
            Console.WriteLine("Är det en företagskund? (J/N):");
            var input = Console.ReadLine()?.Trim().ToLower();
            bool isFöretag = input == "j";

            if (isFöretag)
            {
                Console.Write("OrgNr: ");
                var orgNr = Console.ReadLine() ?? "";
                Console.Write("Företagsnamn: ");
                var fNamn = Console.ReadLine() ?? "";
                Console.Write("Företagsadress: ");
                var fAdress = Console.ReadLine() ?? "";

                var foretagExists = await Transactor.CheckIfForetagExists(orgNr);
                if (!foretagExists)
                {
                    var createdF = await Transactor.NonQueryForetag(orgNr, fNamn, fAdress);
                    if (!createdF)
                    {
                        Console.WriteLine("Fel vid registrering av Företag.");
                        Console.ReadKey();
                        return;
                    }
                }

                Console.Write("Kontaktpersonens personnummer: ");
                var pnr = Console.ReadLine() ?? "";

                var kundExists = await Transactor.CheckIfKundExists(pnr);
                if (!kundExists)
                {
                    Console.Write("Kontaktpersonens förnamn: ");
                    var kFnamn = Console.ReadLine() ?? "";
                    Console.Write("Kontaktpersonens efternamn: ");
                    var kEnamn = Console.ReadLine() ?? "";
                    var createdKund = await Transactor.NonQueryKund(kFnamn, kEnamn, pnr, orgNr);
                    if (!createdKund)
                    {
                        Console.WriteLine("Fel vid registrering av kund.");
                        Console.ReadKey();
                        return;
                    }
                }

                int kundId = await Transactor.NonQueryKunder(0, pnr);
                if (kundId <= 0)
                {
                    Console.WriteLine("Fel vid skapande av Kunder-rad.");
                    Console.ReadKey();
                    return;
                }

                int bokId = await SkapaBokning();
                if (bokId <= 0)
                {
                    Console.WriteLine("Fel vid bokning.");
                    Console.ReadKey();
                    return;
                }

                var okBK = await Transactor.NonQueryBokningKund(bokId, kundId);
                Console.WriteLine(okBK ? "BokningKund skapad." : "Fel i BokningKund.");

                Console.Write("AnställningsId för den som skriver avtal: ");
                var anstStr = Console.ReadLine() ?? "";
                if (!int.TryParse(anstStr, out int anstallningsId))
                {
                    Console.WriteLine("Felaktigt AnställningsId.");
                    Console.ReadKey();
                    return;
                }

                var avtalOk = await Transactor.NonQueryAvtal(anstallningsId, bokId, true, null);
                Console.WriteLine(avtalOk ? "Avtal signerat." : "Fel vid avtalssignering.");
                Console.ReadKey();
            }
            else
            {
                Console.Write("Personnummer: ");
                var pnr = Console.ReadLine() ?? "";
                var kundExists = await Transactor.CheckIfKundExists(pnr);
                if (!kundExists)
                {
                    Console.Write("Förnamn: ");
                    var fNamn = Console.ReadLine() ?? "";
                    Console.Write("Efternamn: ");
                    var eNamn = Console.ReadLine() ?? "";
                    var createdKund = await Transactor.NonQueryKund(fNamn, eNamn, pnr, null);
                    if (!createdKund)
                    {
                        Console.WriteLine("Fel vid registrering av kund.");
                        Console.ReadKey();
                        return;
                    }
                }

                int kundId = await Transactor.NonQueryKunder(0, pnr);
                if (kundId <= 0)
                {
                    Console.WriteLine("Fel vid skapande av Kunder-rad.");
                    Console.ReadKey();
                    return;
                }

                int bokId = await SkapaBokning();
                if (bokId <= 0)
                {
                    Console.WriteLine("Fel vid bokning.");
                    Console.ReadKey();
                    return;
                }

                var okBK = await Transactor.NonQueryBokningKund(bokId, kundId);
                Console.WriteLine(okBK ? "BokningKund skapad." : "Fel i BokningKund.");

                Console.Write("AnställningsId för den som skriver avtal: ");
                var anstStr = Console.ReadLine() ?? "";
                if (!int.TryParse(anstStr, out int anstallningsId))
                {
                    Console.WriteLine("Felaktigt AnställningsId.");
                    Console.ReadKey();
                    return;
                }

                var avtalOk = await Transactor.NonQueryAvtal(anstallningsId, bokId, true, null);
                Console.WriteLine(avtalOk ? "Avtal signerat." : "Fel vid avtalssignering.");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Ny Bokning Agnostic.
        /// </summary>
        /// <returns></returns>
        private static async Task<int> SkapaBokning()
        {
            Console.Write("RegNr: ");
            var reg = Console.ReadLine() ?? "";
            Console.Write("Startdatum (yyyy-mm-dd): ");
            var startStr = Console.ReadLine() ?? "";
            if (!DateTime.TryParse(startStr, out DateTime start))
            {
                Console.WriteLine("Felaktigt datumformat.");
                return -1;
            }
            Console.Write("Slutdatum (yyyy-mm-dd): ");
            var endStr = Console.ReadLine() ?? "";
            if (!DateTime.TryParse(endStr, out DateTime slut))
            {
                Console.WriteLine("Felaktigt datumformat.");
                return -1;
            }

            int bokId = await Transactor.NonQueryBokning(reg, start, slut);
            if (bokId <= 0)
            {
                Console.WriteLine("Kunde ej skapa Bokning.");
                return -1;
            }
            return bokId;
        }


        /// <summary>
        /// Skapa en enkel faktura för en bokning.
        /// </summary>
        private static async Task SkapaFaktura()
        {
            Console.Write("BokningsId: ");
            var bokStr = Console.ReadLine() ?? "";
            if (!int.TryParse(bokStr, out int bokningsId))
            {
                Console.WriteLine("Felaktigt BokningsID.");
                return;
            }
            Console.Write("Belopp: ");
            var beloppStr = Console.ReadLine() ?? "";
            if (!decimal.TryParse(beloppStr, out decimal belopp))
            {
                Console.WriteLine("Felaktigt belopp.");
                return;
            }
            Console.Write("FakturaId (ex: 1): ");
            var fidStr = Console.ReadLine() ?? "";
            if (!int.TryParse(fidStr, out int fakturaId))
            {
                Console.WriteLine("Felaktigt FakturaId.");
                return;
            }

            DateTime datum = DateTime.Now;
            DateTime forfDatum = datum.AddDays(30);
            bool status = false;

            var success = await Transactor.NonQueryFaktura(fakturaId, bokningsId, belopp, datum, forfDatum, status);
            Console.WriteLine(success ? "Faktura skapad!" : "Fel vid skapande av faktura.");
            Console.ReadKey();
        }
    }







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
                // Boolen blir "Tillgänglig" etc
                string sql = @"
                SELECT b.RegNr, b.BilTyp,
                CASE 
                  WHEN bk.BokningsId IS NOT NULL THEN 'Uthyrd'
                  ELSE 'Tillgänglig'
                END AS StatusText
                FROM Bil b
                LEFT JOIN (
                    SELECT BokningsId, RegNr 
                    FROM Bokning 
                    WHERE GETDATE() BETWEEN StartDatum AND SlutDatum
                ) bk ON b.RegNr = bk.RegNr;";

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

        // ============= KUND =============
        public static async Task<bool> CheckIfKundExists(string personnummer)
        {
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = @"SELECT COUNT(*) FROM Kund WHERE Personnummer = @Pnr;";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Pnr", personnummer);

                int count = (int)await cmd.ExecuteScalarAsync();
                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
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

        public static async Task<int> NonQueryKunder(int kundId, string personnummer)
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


        // ============= FÖRETAG =============
        public static async Task<bool> CheckIfForetagExists(string orgNr)
        {
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = @"SELECT COUNT(*) FROM Företag WHERE OrgNr = @O;";
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

                string sql = @"INSERT INTO Företag (OrgNr, ForetagsNamn, Adress)
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

        public static async Task<List<Station>> QueryStation()
        {
            var result = new List<Station>();
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = "SELECT StationId, Adress FROM Station;";

                using var cmd = new SqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var station = new Station
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
        public static async Task<int> NonQueryBokning(string regNr, DateTime start, DateTime slut)
        {
            int lastId = 0;
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();


                string read = @"SELECT ISNULL(MAX(BokningsId), 0) AS MaxBokningsId FROM Bokning;";
                using var cmdRead = new SqlCommand(read, conn);
                int maxId = (int)await cmdRead.ExecuteScalarAsync();

                lastId = maxId + 1;

                string sql = @"INSERT INTO Bokning (BokningsId, RegNr, StartDatum, SlutDatum) 
                           VALUES (@Id, @R, @St, @Sl);";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Id", lastId);
                cmd.Parameters.AddWithValue("@R", regNr);
                cmd.Parameters.AddWithValue("@St", start);
                cmd.Parameters.AddWithValue("@Sl", slut);
                

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return -1;
            }
            return lastId;
        }

        public static async Task<bool> NonQueryBokningKund(int bokningsId, int kundId)
        {
            //int lastId = 0; 
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                //string read = @"SELECT ISNULL(MAX(KundId), 0) AS MaxKundId FROM BokningKund;";
                //using var cmdRead = new SqlCommand(read, conn);
                //int maxId = (int)await cmdRead.ExecuteScalarAsync();
                //lastId = maxId + 1;

                string sql = @"
            INSERT INTO BokningKund (BokningsId, KundId)
            VALUES (@BokId, @Kid);";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@BokId", bokningsId);
                cmd.Parameters.AddWithValue("@Kid", kundId);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }


        // ============= FAKTURA =============
        public static async Task<bool> NonQueryFaktura(int FakturaId, int bokningsId, decimal belopp, DateTime datum, DateTime forfDatum, bool status)
        {
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = @"INSERT INTO Faktura (FakturaId, BokningsId, FakturaDatum, Belopp, ForfalloDatum, Status)
                           VALUES (@Fid, @Bid, @Date, @Belopp, @Fdate, @Ok);";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Fid", FakturaId);
                cmd.Parameters.AddWithValue("@Bid", bokningsId);
                cmd.Parameters.AddWithValue("@Date", datum);
                cmd.Parameters.AddWithValue("@Belopp", belopp);
                cmd.Parameters.AddWithValue("@Fdate", forfDatum);
                cmd.Parameters.AddWithValue("@Ok", status);

                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }


        // -------------- AVTAL -----------------
        public static async Task<bool> NonQueryAvtal(int anstId, int bokId, bool status, string? filepath)
        {
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = @"INSERT INTO Avtal (AnstallningsId, BokningsId, Status, Filepath)
                       VALUES (@A, @B, @S, @F);";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@A", anstId);
                cmd.Parameters.AddWithValue("@B", bokId);
                cmd.Parameters.AddWithValue("@S", status);
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

        // -------------- PERSONAL ---------------
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




    }


    #region Modeller som kan användas för att läsa och skriva enligt DB-schemat
    public class Kund
    {
        public string ForNamn { get; set; } = "";
        public string EfterNamn { get; set; } = "";
        public string Personnummer { get; set; } = ""; // PK
        public string? OrgNr { get; set; } // Optional FK

    }

    public class Företag // dbo.Företag
    {
        public string OrgNr { get; set; } = ""; // PK
        public string ForetagsNamn { get; set; } = "";
        public string Adress { get; set; } = "";
    }

    public class Bil
    {
        public string RegNr { get; set; } = ""; // PK
        public int BilTyp { get; set; }
    }

    public class Personal // Denna heter "dbo.Anstalld"
    {
        public string ForNamn { get; set; } = "";
        public string EfterNamn { get; set; } = "";
        public int AnställningsId { get; set; } // PK
    }

    public class BokningKund
    {
        public int BokningsId { get; set; } // PK
        public string Personnummer { get; set; } = ""; // FK
        public string OrgNr { get; set; } = ""; // FK
    }
        
    public class Station
    {
        public int StationId { get; set; } // Pk
        public string Adress { get; set; } = "";
    }

    public class BilStation
    {
        public string RegNr { get; set; } = ""; // Composite PK [0]
        public int StationId { get; set; } // Composite PK [1]
    }

    public class Kunder
    {
        public int KundId { get; set; } // PK
        public string Personnummer { get; set; } // FK -> Kund
    }

    public class Bokning {
        public int BokningsId { get; set; } // PK
        public string RegNr { get; set; } = ""; // FK
        public DateTime StartDatum { get; set; }
        public DateTime SlutDatum { get; set; }
    }

    public class Kontroll
    {
        public int BokningsId { get; set; } // PK && FK
        public DateTime Datum { get; set; }
        public int AnstallningsId { get; set; } // FK 
        public bool Status { get; set; } // False = OK, True = Skada. Default False.
        public string? Filepath { get; set; } // Sökväg/länk till de facto dokumentationen.
    }

    public class Skada
    {
        public int SkadaId { get; set; } // PK
        public int BokningsId { get; set; } // FK -> Kontroll
        public DateTime Datum { get; set; }
        public string? Beskrivning { get; set; } = ""; // Valfri. 
        public decimal? Kostnad { get; set; } // Har inte inkluderats i ERD. Do not use.
    }


    public class Faktura
    {
        public int FakturaId { get; set; } // PK
        public int BokningsId { get; set; } // FK -> Bokning
        public DateTime Fakturadatum { get; set; } = DateTime.Now;
        public DateTime Forfallodatum { get; set; } = DateTime.Now.AddDays(30);
        public decimal Belopp { get; set; } // FLOAT i SSMS. Använd denna för både Företagsfakturering samt skada-fakturering.
        public bool Status { get; set; } = false; // False = ej betald, True = Betald.
    }

    public class Avtal
    {
        public int AnstallningsId { get; set; } // FK -> Anstalld
        public int BokningsId { get; set; } // PK, FK -> Bokning
        public bool Status { get; set; } // False, ej signerad = True = signerad.
        public string? Filepath { get; set; } // Nullable attribut. Ej använd, men kan Demo:a denna i framtiden.
    }
    public enum BilType { Stadsbil, Liten, Mellan, Kombi, Minibuss, Transportbil };
    #endregion

}
