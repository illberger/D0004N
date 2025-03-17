using System.Data.SqlClient;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using static D0004N.Schema;

namespace D0004N
{

    /// <summary>
    /// Exempel på enkel användning av datamodellen
    /// - Elias Töyrä
    /// </summary>
    public class Program
    {
        public static async Task Main(string[] args)
        {

            while (true) { 
                int behorighet = await Login();
                if (behorighet < 0 || behorighet > 1)
                {
                    Console.WriteLine("Inloggningen misslyckades. Avslutar...");
                    Console.ReadKey();
                    return;
                }

                await Meny(behorighet);
                Console.WriteLine("Vill du Avsluta (J/N)?");
                var input = Console.ReadLine()?.Trim().ToLower();
                bool yes = input == "j";
                if (yes) break;
            }   
        }


        private static async Task<int> Login()
        {
            var allPersonal = await Transactor.QueryAllPersonal();
            if (allPersonal == null || allPersonal.Count == 0)
            {
                Console.WriteLine("Inga poster i tabellen [Anstalld]. Går ej att logga in.");
                return -1;
            }

            Console.WriteLine("Välj vilken användare du vill logga in som:");
            Console.WriteLine("AnstallningsId | Behorighet | Fornamn      | Efternamn");
            Console.WriteLine("-----------------------------------------------------");
            foreach (var p in allPersonal)
            {
                Console.WriteLine($"{p.AnstallningsId}            | {p.Behorighet}           | {p.Fornamn,-12} | {p.Efternamn}");
            }

            Console.Write("\nAnge AnstallningsId för inloggning: ");
            var input = Console.ReadLine()?.Trim();
            if (!int.TryParse(input, out int anstId))
            {
                Console.WriteLine("Felaktig inmatning.");
                return -1;
            }

            var chosen = allPersonal.Find(x => x.AnstallningsId == anstId);
            if (chosen == null)
            {
                Console.WriteLine("Ingen personal med det id:t.");
                return -1;
            }

            return chosen.Behorighet;
        }





        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static async Task Meny(int behorighet)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Green Rental DB - Huvudmeny");
                Console.WriteLine($"Inloggad med behörighet: {behorighet}");
                Console.WriteLine("1. Registrera bil (Behörighet 1++)"); // Insert till Bilstation och Bil
                Console.WriteLine("2. Registrera station (Behörighet 1++)"); // Insert till Station
                Console.WriteLine("3. Visa alla bilar + status (Behörighet 0++)"); // Select från BilStation + BokningBil
                Console.WriteLine("4. Visa stationer (Behörighet 0++)"); // Select från Station
                Console.WriteLine("5. Hyr ut Bil (Behörighet 1++)"); // Skriva avtal samt eventuell registrera kunduppgifter
                Console.WriteLine("6. Inlämning av bil (Behörighet 1++)."); // En update till BokningBil
                Console.WriteLine("7. Gör skadekontroll av inlämnad bil (Behörighet 0++)."); // En Insert till Kontroll samt eventuellt Skada
                Console.WriteLine("q. 'Logga ut'\n");

                var input = Console.ReadLine()?.Trim();

                switch (input)
                {
                    case "1":
                        await RegistreraBil(behorighet);
                        break;
                    case "2":
                        await RegistreraStation(behorighet);
                        break;
                    case "3":
                        await VisaAllaBilarMedStatus(behorighet);
                        break;
                    case "4":
                        await VisaStationer(behorighet);
                        break;
                    case "5":
                        await BokaHyrning(behorighet); ;
                        break;
                    case "6":
                        await AvslutaHyrning(behorighet);
                        break;
                    case "7":
                        await SkadeKontroll(behorighet);
                        break;
                    case "q":
                        return;
                    default:
                        Console.WriteLine("Felaktig inmatning.");
                        break;
                }
            }
        }

        /// <summary>
        /// Registrera Station
        /// strikt enligt denna modell
        /// </summary>
        /// <returns></returns>
        private static async Task RegistreraStation(int behorighet)
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

            var success = await Transactor.NonQueryStation(stationId, adress, behorighet);
            if (!success)
            {
                Console.WriteLine("Fel vid registrering av station (INSERT).");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Station registrerad!");
            Console.ReadKey();
        }

        private static async Task VisaStationer(int behorighet)
        {
            var stationer = await Transactor.QueryStation(behorighet);
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
        /// Registrera av en bil.
        /// </summary>
        private static async Task RegistreraBil(int behorighet)
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

            var bilTypReturn = await Transactor.QueryBilTyp(bilTyp, behorighet);

            if (!bilTypReturn) // "< 0 av "bilTyp" i dbo.BilTyp"
            {
                Console.WriteLine($"Kunde ej hitta BilTyp, vill du registrera BilTypen {bilTyp}? (J/N)");
                var input = Console.ReadLine()?.Trim().ToLower();
                bool fortsatt = input == "j";
                if (!fortsatt) return;

                Console.WriteLine($"Ange SEK/Dygn för biltypen {bilTyp}:");
                var hyrSats = Console.ReadLine()?.Trim();
                if (!decimal.TryParse(hyrSats, System.Globalization.NumberStyles.Number, CultureInfo.InvariantCulture, out var krDygn))
                {
                    Console.WriteLine($"Fel vid tolkning");
                    Console.ReadKey();
                    return;
                }

                bool nonQuerySuccess = await Transactor.NonQueryBilTyp(bilTyp, krDygn, behorighet);
                if (!nonQuerySuccess)
                {
                    Console.WriteLine("Fel vid transakation till BilTyp");
                    Console.ReadKey();
                    return;
                }
                // Biltyp är nu skapad 
            }

            var successStation = await Transactor.NonQueryBilStation(regNr, stationId, behorighet);
            if (!successStation)
            {
                Console.WriteLine("Fel vid registrering av BilStation (INSERT).");
                Console.ReadKey();
                return;
            }

            var successBil = await Transactor.NonQueryBil(regNr, bilTyp, behorighet);
            if (!successBil)
            {
                Console.WriteLine("Fel vid registrering av bil (INSERT).");
                Console.ReadKey();
                return; // Hängande post i "BilStation", ta detta i hänsyn vid nästa query.
            }

            Console.WriteLine("BilStation och Bil registrerades korrekt.");
            Console.ReadKey();
        }



        /// <summary>
        /// Enkel variant
        /// </summary>
        private static async Task VisaAllaBilarMedStatus(int behorighet)
        {
            var bilLista = await Transactor.QueryBilWithStatus(behorighet);
            if (bilLista == null || bilLista.Count == 0)
            {
                Console.WriteLine("Inga bilar hittades i databasen.");
                Console.ReadKey();
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
            Console.ReadKey();
        }

        /// <summary>
        /// <b>Boka en bil.</b>
        /// </summary>
        private static async Task BokaHyrning(int behorighet)
        {
            Console.WriteLine("Är det en företagskund? (J/N):");
            var input = Console.ReadLine()?.Trim().ToLower();
            bool isFöretag = input == "j";
            int kundId = 0;

            if (isFöretag)
            {
                Console.Write("OrgNr: ");
                var orgNr = Console.ReadLine() ?? "";
                Console.Write("Företagsnamn: ");
                var fNamn = Console.ReadLine() ?? "";
                Console.Write("Företagsadress: ");
                var fAdress = Console.ReadLine() ?? "";

                var foretagExists = await Transactor.CheckIfForetagExists(orgNr, behorighet);
                if (!foretagExists)
                {
                    var createdF = await Transactor.NonQueryForetag(orgNr, fNamn, fAdress, behorighet);
                    if (!createdF)
                    {
                        Console.WriteLine("Fel vid registrering av Företag.");
                        Console.ReadKey();
                        return;
                    }
                }

                Console.Write("Kontaktpersonens personnummer: ");
                var pnr = Console.ReadLine() ?? "";

                var kundExists = await Transactor.CheckIfKundExists(pnr, behorighet);
                if (!kundExists)
                {
                    Console.Write("Kontaktpersonens förnamn: ");
                    var kFnamn = Console.ReadLine() ?? "";
                    Console.Write("Kontaktpersonens efternamn: ");
                    var kEnamn = Console.ReadLine() ?? "";
                    var createdKund = await Transactor.NonQueryKund(kFnamn, kEnamn, pnr, orgNr, behorighet);
                    if (!createdKund)
                    {
                        Console.WriteLine("Fel vid registrering av kund.");
                        Console.ReadKey();
                        return;
                    }
                    kundId = await Transactor.NonQueryKunder(pnr, behorighet); // --------- Skapa KundId -> INSERT
                    if (kundId <= 0)
                    {
                        Console.WriteLine("Fel vid skapande av Kunder-rad.");
                        Console.ReadKey();
                        return;
                    }
                } else
                {
                    kundId = await Transactor.QueryKunder(pnr, behorighet);
                }

                

                int bokId = await SkapaBokning(kundId, behorighet); // ------ Skapa BoKId -> Insert

                if (bokId <= 0)
                {
                    Console.WriteLine("Fel vid bokning.");
                    Console.ReadKey();
                    return;
                }

                bool okBK = bokId > 1;
                Console.WriteLine(okBK ? "BokningKund skapad." : "Fel i BokningKund.");

                Console.Write("AnställningsId för den som skriver avtal: ");
                var anstStr = Console.ReadLine() ?? "";
                if (!int.TryParse(anstStr, out int anstallningsId))
                {
                    Console.WriteLine("Felaktigt AnställningsId.");
                    Console.ReadKey();
                    return;
                }

                var avtalOk = await Transactor.NonQueryAvtal(anstallningsId, bokId, true, null, behorighet);
                Console.WriteLine(avtalOk ? "Avtal signerat." : "Fel vid avtalssignering.");
            }
            else
            {
                Console.Write("Personnummer: ");
                var pnr = Console.ReadLine() ?? "";
                var kundExists = await Transactor.CheckIfKundExists(pnr, behorighet);
                if (!kundExists)
                {
                    Console.Write("Förnamn: ");
                    var fNamn = Console.ReadLine() ?? "";
                    Console.Write("Efternamn: ");
                    var eNamn = Console.ReadLine() ?? "";
                    var createdKund = await Transactor.NonQueryKund(fNamn, eNamn, pnr, null, behorighet);
                    if (!createdKund)
                    {
                        Console.WriteLine("Fel vid registrering av kund.");
                        Console.ReadKey();
                        return;
                    }
                    kundId = await Transactor.NonQueryKunder(pnr, behorighet);
                    if (kundId <= 0)
                    {
                        Console.WriteLine("Fel vid skapande av Kunder-rad.");
                        Console.ReadKey();
                        return;
                    }
                }
                else
                {
                    kundId = await Transactor.QueryKunder(pnr, behorighet);
                }



                int bokId = await SkapaBokning(kundId, behorighet);

                if (bokId <= 0)
                {
                    Console.WriteLine("Fel vid bokning.");
                    Console.ReadKey();
                    return;
                }

                Console.Write("AnställningsId för den som skriver avtal: ");
                var anstStr = Console.ReadLine() ?? "";
                if (!int.TryParse(anstStr, out int anstallningsId))
                {
                    Console.WriteLine("Felaktigt AnställningsId.");
                    Console.ReadKey();
                    return;
                }

                var avtalOk = await Transactor.NonQueryAvtal(anstallningsId, bokId, true, null, behorighet);
                Console.WriteLine(avtalOk ? "Avtal signerat." : "Fel vid avtalssignering.");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Ny Bokning Agnostic.
        /// </summary>
        /// <returns>Boknings ID:t</returns>
        private static async Task<int> SkapaBokning(int kundId, int behorighet)
        {

            int bokId = await Transactor.NonQueryBokningKund(kundId, behorighet);
            if (bokId <= 0)
            {
                Console.WriteLine("Kunde ej skapa Bokning i BokningKund.");
                Console.ReadKey();
                return -1;
            }
            Console.WriteLine($"Bokning skapad med BokningsId = {bokId}.");

            Console.WriteLine("Vill du ange slutdatum (S), eller hyra löpande fr.o.m. nu (L)?");
            var input = Console.ReadLine()?.Trim().ToLower();
            bool lopandeHyrning = (input == "l");

            DateTime? slutdatum = null;
            DateTime start = DateTime.Now;
            if (!lopandeHyrning)
            {
                Console.Write("Slutdatum (yyyy-mm-dd): ");
                var endStr = Console.ReadLine() ?? "";
                if (!DateTime.TryParse(endStr, out DateTime slutParsed))
                {
                    Console.WriteLine("Felaktigt datumformat för slutdatum.");
                    Console.ReadKey();
                    return -1;
                }
                slutdatum = slutParsed;
            }

            var regNrList = new List<string>();

            while (true)
            {
                Console.Write("Ange RegNr (eller lämna tomt för att avsluta): ");
                var reg = Console.ReadLine()?.Trim();
                if (string.IsNullOrEmpty(reg)) break;

                bool avbl = await Transactor.QueryRegNrWithStatus(reg, behorighet);
                if (!avbl)
                {
                    Console.WriteLine($"Bil {reg} är inte tillgänglig just nu.");
                    continue;
                }

                regNrList.Add(reg);
                Console.WriteLine($"Bil {reg} lades till bokningen.");
            }

            if (regNrList.Count == 0)
            {
                Console.WriteLine("Ingen bil angavs. Avbryter bokning...");
                Console.ReadKey();
                return -1;
            }



            bool success = await Transactor.NonQueryBokningBil(regNrList, bokId, start, slutdatum, behorighet);
            if (!success)
            {
                Console.WriteLine("Fel vid skapande av poster i BokningBil.");
                Console.ReadKey();
                return -1;
            }
            Console.WriteLine("Samtliga valda bilar lades till bokningen.");

            if (!lopandeHyrning)
            {
                decimal belopp = 0;
                foreach (string reg in regNrList)
                {
                    var bilInfo = await Transactor.QueryBiltypByRegNr(reg, behorighet);
                    if (bilInfo == null)
                    {
                        Console.WriteLine("Kunde inte hitta Biltyp-info för bil: " + reg);
                        Console.ReadKey();
                        return -1;
                    }

                    TimeSpan diff = slutdatum.Value - start;
                    int antalDygn = (int)diff.TotalDays;
                    DateTime tempDate = start;
                    for (int i = 0; i <= antalDygn; i++)
                    {
                        var dayOfWeek = tempDate.DayOfWeek;
                        if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
                        {
                            belopp += bilInfo.KrHelg;
                        }
                        else
                        {
                            belopp += bilInfo.KrDygn;
                        }
                        tempDate = tempDate.AddDays(1);
                    }
                }
                bool facturaSuccess = await SkapaFaktura(bokId, belopp, behorighet);
                if (!facturaSuccess) return -1;
            }

            if (lopandeHyrning)
            {
                Console.WriteLine($"Följande bilar hyrs fr.o.m {start}: ");
                if (regNrList.Count > 0)
                {
                    foreach (var regNr in regNrList)
                    {
                        Console.WriteLine($"{regNr}");
                    }
                } else
                {
                    Console.WriteLine("Tyvärr så fanns inte angivna bilar tillgängliga.");
                }
                Console.ReadKey();
            }
            return bokId;
        }



        /// <summary>
        /// <b>Agnostic Faktura NonQuery.</b><br></br>
        /// Skapa en enkel faktura för en bokning.
        /// </summary>
        private static async Task<bool> SkapaFaktura(int bokningsId, decimal belopp, int behorighet)
        {
            var fakturaId = DateTime.Now.ToBinary(); // -2^64 i nutid
            DateTime datum = DateTime.Now;
            DateTime forfDatum = datum.AddDays(30);
            bool status = false;
            var success = await Transactor.NonQueryFaktura(fakturaId, bokningsId, belopp, datum, forfDatum, status, behorighet);
            Console.WriteLine(success ? $"Skapade faktura med beloppet: {belopp}" : "Fel vid skapande av faktura.");
            Console.ReadKey();
            return success;
        }

        /// <summary>
        /// Uppdatera en eller flera poster i <b>BokningBil</b> som inte har ett slutDatum.
        /// </summary>
        /// <returns></returns>
        private static async Task AvslutaHyrning(int behorighet)
        {
            var kunderObject = await Transactor.QueryAllKunder(behorighet);
            Console.WriteLine("Registrerade kunder:\n");

            foreach (var kunder in kunderObject)
            {
                if (kunder != null)
                {
                    Console.WriteLine($"KundId: {kunder.KundId}, Pnr: {kunder.Personnummer}\n");
                }
            }
            Console.Write("Ange KundID: ");
            var kundIdStr = Console.ReadLine() ?? "";
            if (!int.TryParse(kundIdStr, out int kundId))
            {
                Console.WriteLine("Felaktigt KundID.");
                Console.ReadKey();
                return;
            }

            var bokningsList = await Transactor.QueryBokningarForKund(kundId, behorighet);
            if (bokningsList == null || bokningsList.Count == 0)
            {
                Console.WriteLine("Hittade inga bokningar för KundID = " + kundId);
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Bokningar för kund " + kundId + ":");
            foreach (var b in bokningsList)
            {
                Console.WriteLine($"- BokningsId: {b}");
            }

            Console.Write("Vilken BokningsId vill du avsluta?: ");
            var bokIdStr = Console.ReadLine() ?? "";
            if (!int.TryParse(bokIdStr, out int bokId))
            {
                Console.WriteLine("Felaktigt BokningsId.");
                Console.ReadKey();
                return;
            }

            var bokningBilList = await Transactor.QueryBokningBil(bokId, behorighet);
            if (bokningBilList == null || bokningBilList.Count == 0)
            {
                Console.WriteLine("Ingen bil hittades för BokningsId = " + bokId);
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Följande bilar är kopplade till bokningen:");
            foreach (var bb in bokningBilList)
            {
                Console.WriteLine($"- RegNr: {bb.RegNr}, Start: {bb.StartDatum}, Slut: {(bb.SlutDatum.HasValue ? bb.SlutDatum.Value.ToString() : "NULL")}");
            }

            Console.Write("Vill du avsluta samtliga bilar i denna bokning? (J/N): ");
            var jn = Console.ReadLine()?.Trim().ToLower();
            bool avslutaAlla = (jn == "j");

      
            var regNrToClose = new List<string>();
            if (avslutaAlla)
            {
         
                foreach (var item in bokningBilList)
                {
                    if (!item.SlutDatum.HasValue)
                        regNrToClose.Add(item.RegNr);
                }
            }
            else
            {
                while (true)
                {
                    Console.Write("Ange RegNr du vill avsluta (tom rad för klart): ");
                    var reg = Console.ReadLine()?.Trim();
                    if (string.IsNullOrEmpty(reg)) break;
                    regNrToClose.Add(reg);
                }
            }

            if (regNrToClose.Count == 0)
            {
                Console.WriteLine("Inga bilar att avsluta. Avbryter...");
                Console.ReadKey();
                return;
            }

            Console.Write("Ange slutdatum (yyyy-mm-dd): ");
            var slutStr = Console.ReadLine() ?? "";
            if (!DateTime.TryParse(slutStr, out DateTime newSlut))
            {
                Console.WriteLine("Felaktigt datumformat.");
                Console.ReadKey();
                return;
            }

            decimal totalBelopp = 0;

            foreach (var regNr in regNrToClose)
            {
                var row = bokningBilList.Find(x => x.RegNr == regNr);
                if (row == null || row.SlutDatum.HasValue)
                {
                    continue;
                }
                DateTime start = row.StartDatum;
                var bilInfo = await Transactor.QueryBiltypByRegNr(regNr, behorighet);
                if (bilInfo == null)
                {
                    Console.WriteLine($"Kunde inte hämta Biltyp för {regNr} - hoppar över.");
                    continue;
                }

                TimeSpan diff = newSlut - start; // (2025-03-13) - (2025-03-12) = TimeSpan(1)
                int antalDygn = (int)diff.TotalDays; // (int)1
                decimal belopp = 0;

                DateTime tempDate = start; // 2025-03-12
                for (int i = 0; i <= antalDygn; i++) // n = 1, i = 1. Z: {0 + kr ? 0.5}
                {
                    bool helg = (tempDate.DayOfWeek == DayOfWeek.Saturday
                              || tempDate.DayOfWeek == DayOfWeek.Sunday);
                    belopp += helg ? bilInfo.KrHelg : bilInfo.KrDygn; // 0||1
                    tempDate = tempDate.AddDays(1); // 2025-03-12 = 2025-03-12 + Days(1) = 2025-03-13
                }

                totalBelopp += belopp;
            }

            bool updateOk = await Transactor.UpdateSlutDatum(bokId, regNrToClose, newSlut, behorighet);
            if (!updateOk)
            {
                Console.WriteLine("Fel vid uppdatering av slutdatum.");
                Console.ReadKey();
                return;
            }

            if (totalBelopp > 0)
            {
                Console.WriteLine($"Totalt belopp att faktureras: {totalBelopp}");
                bool facturaSuccess = await SkapaFaktura(bokId, totalBelopp, behorighet);
                if (!facturaSuccess)
                {
                    Console.WriteLine("Återställer bokning...");
                    bool reUpdateOk = await Transactor.UpdateSlutDatum(bokId, regNrToClose, null, behorighet);
                    if (!reUpdateOk)
                    {
                        Console.WriteLine("Allting sket sig totalt, återgår till huvudmenyn...");
                        Console.ReadKey();
                        return;
                    }
                }
            }
            else
            {
                Console.WriteLine("Ingen debitering behövs.");
                Console.ReadKey();
            }

            Console.WriteLine("Återgå till huvudmeny (click)");
            Console.ReadKey();
        }



        public static async Task SkadeKontroll(int behorighet)
        {
 
             List<BokningBil> bokningBilList = new List<BokningBil>();
             bokningBilList = await Transactor.QueryAllBokningBil(behorighet);

             if (bokningBilList == null || bokningBilList.Count == 0)
             {
                 Console.WriteLine("Inga bilar funna i BokningBil.");
                 Console.ReadKey();
                 return;
             }
            

            var bilarUtanKontroll = new List<(int BokningsId, string Reg)>();

            foreach (var row in bokningBilList)
            {
                int count = await Transactor.QueryBilarUtanKontroll(row.BokningsId, row.RegNr, behorighet);
                if (count == 0)
                {
                    bilarUtanKontroll.Add((row.BokningsId, row.RegNr));
                }
            }

            if (bilarUtanKontroll.Count == 0)
            {
                Console.WriteLine("Alla bilar i vald del av systemet har redan kontrollposter.");
                Console.ReadKey();
                return;
            }
            foreach (var (bokId, reg) in bilarUtanKontroll)
            {
                Console.Clear();
                Console.WriteLine($"Bil {reg}, Bokning {bokId} saknar kontroll.");
                Console.Write("Ange mätarställning: ");
                var input = Console.ReadLine()?.Trim();
                if (!int.TryParse(input, out int matar))
                {
                    Console.WriteLine("Felaktig inmatning, avbryter denna bil.");
                    Console.ReadKey();
                    continue;
                }

                bool kontrollOk = await Transactor.NonQueryKontroll(reg, bokId, 0, matar, behorighet);
                if (!kontrollOk)
                {
                    Console.WriteLine("Fel vid insättning av kontroll.");
                    Console.ReadKey();
                    continue;
                }

                Console.Write("Hur många skador vill du registrera? ");
                var skdInput = Console.ReadLine()?.Trim();
                if (!int.TryParse(skdInput, out int antalSkador))
                {
                    Console.WriteLine("Felaktig inmatning. Ingen skada registreras.");
                    Console.ReadKey();
                    continue;
                }
                for (int i = 0; i < antalSkador; i++)
                {
                    int skadaId = await Transactor.NonQuerySkada(reg, bokId, null, null, behorighet);
                    Console.WriteLine($"Skada nr {skadaId} insatt för bil {reg}.");
                }
                Console.WriteLine("Klicka för att fortsätta.");
                Console.ReadKey();
            }

            Console.WriteLine("Klart! Återgå till menyn...");
            Console.ReadKey();
        }
    }
}
