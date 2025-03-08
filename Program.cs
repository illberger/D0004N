using System.Data.SqlClient;

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
        /// Registrera Station (En post i Bil kan skapas utan en relation till "BilStation", vilket betyder att en bil inte är kopplad 1:1 till en station
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
        /// Exempel: Registrera en ny bil i dbo.Bil (och koppla bil till en station).
        /// </summary>
        private static async Task RegistreraBil()
        {
            Console.WriteLine("Registrera bil.\n");

            Console.Write("RegNr: ");
            var regNr = Console.ReadLine() ?? "";

            Console.Write("Biltyp (0-5): ");
            var bilTypString = Console.ReadLine() ?? "";
            if (!int.TryParse(bilTypString, out int bilTyp))
            {
                Console.WriteLine("Felaktig inmatning av Biltyp (0-5). Avbryter...");
                return;
            }

            var success = await Transactor.NonQueryBil(regNr, bilTyp);
            if (!success)
            {
                Console.WriteLine("Fel vid registrering av bil (INSERT).");
                return;
            }


            Console.Write("StationId (ex. 1): ");
            var stationIdStr = Console.ReadLine() ?? "";
            if (!int.TryParse(stationIdStr, out int stationId))
            {
                Console.WriteLine("Felaktig inmatning av StationID. Avbryter...");
                return;
            }

            success = await Transactor.NonQueryBilStation(regNr, stationId);
            if (!success)
            {
                Console.WriteLine("Fel vid registrering av BilStation (INSERT).");
                return;
            }

            Console.WriteLine("Bilen registrerades och kopplades till station.");
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
            Console.WriteLine("Är det en företagskund? (J/N)");
            var corp = Console.ReadLine()?.ToLower();
            bool isFöretag = corp == "j";

            if (isFöretag)
            {
                Console.Write("OrgNr: ");
                var org = Console.ReadLine() ?? "";
                Console.Write("Företagsnamn: ");
                var fname = Console.ReadLine() ?? "";
                Console.Write("Företagsadress: ");
                var faddr = Console.ReadLine() ?? "";
                Console.Write("Förnamn kontaktperson: ");
                var fn = Console.ReadLine() ?? "";
                Console.Write("Efternamn kontaktperson: ");
                var ln = Console.ReadLine() ?? "";
                var ex = await Transactor.CheckIfForetagExists(org, fn, ln);
                if (!ex)
                {
                    var cF = await Transactor.NonQueryForetag(org, fname, fn, ln, faddr);
                    if (!cF) return;
                }
                Console.WriteLine("Skapar bokning...");
                var bokId = await SkapaBokning();
                if (bokId < 0) return;

                Console.WriteLine("Ange AnställningsId: ");
                if  (!int.TryParse(Console.ReadLine() ?? "", out var aid))
                {
                    Console.WriteLine("Fel vid inmatningstolkning av anställningsid....");
                    Console.ReadKey();
                    return;
                }

                var avtalSuccess = await Transactor.NonQueryAvtal(aid, bokId, true, null);

                if (!avtalSuccess)
                {
                    Console.WriteLine("Fel vid signering av avtal...");
                    Console.ReadKey();
                    return;
                }

                var cBK = await Transactor.NonQueryBokningKund(bokId, null, org, fn, ln);
                Console.WriteLine(cBK ? "Bokning gjord." : "Fel vid bokning.");
                Console.ReadKey();
            }
            else
            {
                Console.Write("Personnummer: ");
                var pnr = Console.ReadLine() ?? "";
                var ex = await Transactor.CheckIfKundExists(pnr);
                if (!ex)
                {
                    Console.Write("Förnamn: ");
                    var fNamn = Console.ReadLine() ?? "";
                    Console.Write("Efternamn: ");
                    var eNamn = Console.ReadLine() ?? "";
                    var cK = await Transactor.NonQueryKund(pnr, fNamn, eNamn);
                    if (!cK) return;
                }
                var bokId = await SkapaBokning();
                if (bokId <= 0) return;
                var cBK = await Transactor.NonQueryBokningKund(bokId, pnr, null, null, null);
                Console.WriteLine(cBK ? "Bokning gjord." : "Fel vid bokning.");
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
            var sd = Console.ReadLine() ?? "";
            if (!DateTime.TryParse(sd, out var start)) return -1;
            Console.Write("Slutdatum (yyyy-mm-dd): ");
            var ed = Console.ReadLine() ?? "";
            if (!DateTime.TryParse(ed, out var slut)) return -1;
            var bokId = await Transactor.NonQueryBokning(reg, start, slut);
            return bokId;
        }


        /// <summary>
        /// Skapa en enkel faktura för en bokning.
        /// </summary>
        private static async Task SkapaFaktura()
        {
            Console.Write("BokningsId: ");
            var bokIdStr = Console.ReadLine() ?? "";
            if (!int.TryParse(bokIdStr, out int bokningsId))
            {
                Console.WriteLine("Ogiltig BokningsID.");
                Console.ReadKey();
                return;
            }
            Console.Write("Totalt belopp: ");
            var beloppStr = Console.ReadLine() ?? "";
            if (!decimal.TryParse(beloppStr, out decimal belopp))
            {
                Console.WriteLine("Ogiltigt belopp.");
                Console.ReadKey();
                return;
            }

            var fakturaSuccess = await Transactor.NonQueryFaktura(bokningsId, belopp);
            if (!fakturaSuccess)
            {
                Console.WriteLine("Fel vid skapande av faktura.");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("Faktura skapad!");
            Console.WriteLine("Tryck valfri tangent för att fortsätta...");
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

        public static async Task<bool> NonQueryKund(string fNamn, string eNamn, string pnr)
        {
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = @"INSERT INTO Kund (ForNamn, EfterNamn, Personnummer)
                           VALUES (@F, @E, @P);";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@F", fNamn);
                cmd.Parameters.AddWithValue("@E", eNamn);
                cmd.Parameters.AddWithValue("@P", pnr);


                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        // ============= FÖRETAG =============
        public static async Task<bool> CheckIfForetagExists(string orgNr, string fornamn, string efternamn)
        {
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = @"SELECT COUNT(*) FROM Företag WHERE OrgNr = @O AND Efternamn = @EN AND Fornamn = @FN;";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@O", orgNr);
                cmd.Parameters.AddWithValue("@EN", efternamn);
                cmd.Parameters.AddWithValue("@FN", fornamn);

                int count = (int)await cmd.ExecuteScalarAsync();
                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public static async Task<bool> NonQueryForetag(string orgNr, string namn, string fornamn, string efternamn, string adr)
        {
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = @"INSERT INTO Företag (OrgNr, FöretagsNamn, Adress, Fornamn, Efternamn)
                           VALUES (@O, @N, @A, @FN, @EN);";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@O", orgNr);
                cmd.Parameters.AddWithValue("@N", namn);
                cmd.Parameters.AddWithValue("@A", adr);
                cmd.Parameters.AddWithValue("@FN", fornamn);
                cmd.Parameters.AddWithValue("@EN", efternamn);

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
            int lastId = 0; // Tvinga check constraint i DB:n att id är > 0.
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

        public static async Task<bool> NonQueryBokningKund(int bokningsId, string? pnr, string? orgNr, string? fornamn, string? efternamn)
        {
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = @"
            INSERT INTO BokningKund (BokningsId, Personnummer, OrgNr, Fornamn, Efternamn)
            VALUES (@BokId, @Pnr, @Org, @Fnamn, @Enamn);";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@BokId", bokningsId);

                if (pnr == null)
                {
                    cmd.Parameters.AddWithValue("@Pnr", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Fnamn", fornamn);
                    cmd.Parameters.AddWithValue("@Enamn", efternamn);
                    cmd.Parameters.AddWithValue("@Org", orgNr);
                }
                else
                {
                    cmd.Parameters.AddWithValue("@Org", DBNull.Value);
                    cmd.Parameters.AddWithValue("@Pnr", pnr);
                }
                

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
        public static async Task<bool> NonQueryFaktura(int bokningsId, decimal belopp)
        {
            try
            {
                using var conn = new SqlConnection(DB);
                await conn.OpenAsync();

                string sql = @"INSERT INTO Faktura (BokningsId, Totalbelopp)
                           VALUES (@B, @Belopp);";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@B", bokningsId);
                cmd.Parameters.AddWithValue("@Belopp", belopp);

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

                string sql = @"INSERT INTO Avtal (AnställningsId, BokningsId, Status, Filepath)
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



    public class Kund
    {
        public string ForNamn { get; set; } = "";
        public string EfterNamn { get; set; } = "";
        public string Personnummer { get; set; } = ""; // PK

    }

    public class Företag
    {
        public string OrgNr { get; set; } = ""; // PK
        public string FöretagsNamn { get; set; } = "";
        public string Adress { get; set; } = "";
        public string Fornamn { get; set; } = ""; // PK
        public string EfterNamn { get; set; } = ""; // PK 
    }

    public class Bil
    {
        public string RegNr { get; set; } = ""; // PK
        public int BilTyp { get; set; }
    }

    public class Personal // En personal är inte kopplad till en station
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

    // En bil är ALLTID kopplad till en station (uppdateras vid inlämning till annan station).
    public class BilStation
    {
        public string RegNr { get; set; } = ""; // Composite PK [0]
        public int StationId { get; set; } // Composite PK [1]
    }

    public class Bokning {
        public int BokningsId { get; set; } // PK
        public string RegNr { get; set; } = ""; // FK
        public DateTime StartDatum { get; set; }
        public DateTime SlutDatum { get; set; }
    }

    public class Kontroll
    {
        public string RegNr { get; set; } = ""; // Composite PK [0]
        public DateTime Datum { get; set; } // Composite PK [1]
        public int AnställningsId { get; set; } // FK 
        public bool Status { get; set; } // False = OK, True = Skada. Default False.
        public string Dokument { get; set; } = ""; // Sökväg/länk till de facto dokumentationen.
    }

    public class Skada
    {
        public int SkadaId { get; set; } // PK
        public string RegNr { get; set; } = "";  // FK -> Bil
        public DateTime UpptäcktDatum { get; set; }
        public string? Beskrivning { get; set; } = ""; // Valfri
        public decimal Kostnad { get; set; }
    }


    public class Faktura
    {
        public int FakturaId { get; set; } // PK
        public int BokningsId { get; set; } // FK -> Bokning
        public DateTime Fakturadatum { get; set; } = DateTime.Now;
        public DateTime Förfallodatum { get; set; } = DateTime.Now.AddDays(30);
        public decimal Totalbelopp { get; set; }
        public bool Status { get; set; } = false; // False = ej betald, True = Betald.
    }

    public class Avtal
    {
        public int AnställningsId { get; set; }
        public int BokningsId { get; set; }
        public bool Status { get; set; } // False, ej signerad = True = signerad.
        public string Filepath { get; set; } = "";// Nullable attribut. Ej använd, men kan Demo:a denna i framtiden.
    }
    public enum BilType { Stadsbil, Liten, Mellan, Kombi, Minibuss, Transportbil };

}
