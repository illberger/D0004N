﻿using System.Data.SqlClient;
using System.Globalization;
using System.Text.RegularExpressions;

namespace D0004N
{

    /// <summary>
    /// Exempel på enkel användning av datamodellen
    /// -Elias Töyrä
    /// </summary>
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
                Console.WriteLine("Steg 1 - 3 krävs för att hyra en bil.");
                Console.WriteLine("1. Registrera bil");
                Console.WriteLine("2. Registrera station");
                Console.WriteLine("3. Registrera Personal");
                Console.WriteLine("4. Visa alla bilar + status");
                Console.WriteLine("5. Visa stationer");
                Console.WriteLine("6. Hyr ut Bil");
                Console.WriteLine("7. Inlämning av bil");
                Console.WriteLine("q. Avsluta\n");

                var input = Console.ReadLine()?.Trim();

                switch (input)
                {
                    case "1":
                        await RegistreraBil();
                        break;
                    case "2":
                        await RegistreraStation();
                        break;
                    case "3":
                        await RegistreraPersonal();
                        break;
                    case "4":
                        await VisaAllaBilarMedStatus();
                        break;
                    case "5":
                        await VisaStationer();
                        break;
                    case "6":
                        await BokaHyrning();
                        break;
                    case "7":
                        await AvslutaHyrning();
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
        /// Registrera av en bil.
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

            var bilTypReturn = await Transactor.QueryBilTyp(bilTyp);

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

                bool nonQuerySuccess = await Transactor.NonQueryBilTyp(bilTyp, krDygn);
                if (!nonQuerySuccess)
                {
                    Console.WriteLine("Fel vid transakation till BilTyp");
                    Console.ReadKey();
                    return;
                }
                // Biltyp är nu skapad 
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
                return; // Hängande post i "BilStation", ta detta i hänsyn vid nästa query.
            }

            Console.WriteLine("BilStation och Bil registrerades korrekt.");
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

                int kundId = await Transactor.NonQueryKunder(pnr); // --------- Skapa KundId -> INSERT
                if (kundId <= 0)
                {
                    Console.WriteLine("Fel vid skapande av Kunder-rad.");
                    Console.ReadKey();
                    return;
                }

                int bokId = await SkapaBokning(kundId); // ------ Skapa BoKId -> Insert

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

                var avtalOk = await Transactor.NonQueryAvtal(anstallningsId, bokId, true, null);
                Console.WriteLine(avtalOk ? "Avtal signerat." : "Fel vid avtalssignering.");
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

                int kundId = await Transactor.NonQueryKunder(pnr);
                if (kundId <= 0)
                {
                    Console.WriteLine("Fel vid skapande av Kunder-rad.");
                    Console.ReadKey();
                    return;
                }

                int bokId = await SkapaBokning(kundId);

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

                var avtalOk = await Transactor.NonQueryAvtal(anstallningsId, bokId, true, null);
                Console.WriteLine(avtalOk ? "Avtal signerat." : "Fel vid avtalssignering.");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Ny Bokning Agnostic.
        /// </summary>
        /// <returns>Boknings ID:t</returns>
        private static async Task<int> SkapaBokning(int kundId)
        {

            int bokId = await Transactor.NonQueryBokningKund(kundId);
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

                regNrList.Add(reg);
                Console.WriteLine($"Bil {reg} lades till bokningen.");
            }

            if (regNrList.Count == 0)
            {
                Console.WriteLine("Ingen bil angavs. Avbryter bokning...");
                Console.ReadKey();
                return -1;
            }

            bool success = await Transactor.NonQueryBokningBil(regNrList, bokId, start, slutdatum);
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
                    var bilInfo = await Transactor.QueryBiltypByRegNr(reg);
                    if (bilInfo == null)
                    {
                        Console.WriteLine("Kunde inte hitta Biltyp-info för bil: " + reg);
                        Console.ReadKey();
                        return -1;
                    }

                    TimeSpan diff = slutdatum.Value - start;
                    int antalDygn = (int)diff.TotalDays;
                    DateTime tempDate = start;
                    for (int i = 0; i < antalDygn; i++)
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
                bool facturaSuccess = await SkapaFaktura(bokId, belopp);
                if (!facturaSuccess) return -1;
            }

            if (lopandeHyrning)
            {
                Console.WriteLine($"Följande bilar hyrs fr.o.m {start}:\n{regNrList.ToArray()}");
                Console.ReadKey();
            }
            return bokId;
        }



        /// <summary>
        /// <b>Agnostic Faktura NonQuery.</b><br></br>
        /// Skapa en enkel faktura för en bokning.
        /// </summary>
        private static async Task<bool> SkapaFaktura(int bokningsId, decimal belopp)
        {
            var fakturaId = DateTime.Now.ToBinary(); // -2^64 i nutid
            DateTime datum = DateTime.Now;
            DateTime forfDatum = datum.AddDays(30);
            bool status = false;
            var success = await Transactor.NonQueryFaktura(fakturaId, bokningsId, belopp, datum, forfDatum, status);
            Console.WriteLine(success ? $"Skapade faktura med beloppet: {belopp}" : "Fel vid skapande av faktura.");
            Console.ReadKey();
            return success;
        }

        /// <summary>
        /// Uppdatera en eller flera poster i <b>BokningBil</b> som inte har ett slutDatum.
        /// </summary>
        /// <returns></returns>
        private static async Task AvslutaHyrning()
        {
            Console.Write("Ange KundID: ");
            var kundIdStr = Console.ReadLine() ?? "";
            if (!int.TryParse(kundIdStr, out int kundId))
            {
                Console.WriteLine("Felaktigt KundID.");
                Console.ReadKey();
                return;
            }

            var bokningsList = await Transactor.QueryBokningarForKund(kundId);
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

            var bokningBilList = await Transactor.QueryBokningBil(bokId);
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
                var bilInfo = await Transactor.QueryBiltypByRegNr(regNr);
                if (bilInfo == null)
                {
                    Console.WriteLine($"Kunde inte hämta Biltyp för {regNr} - hoppar över.");
                    continue;
                }

                TimeSpan diff = newSlut - start;
                int antalDygn = (int)diff.TotalDays;
                decimal belopp = 0;

                DateTime tempDate = start;
                for (int i = 0; i < antalDygn; i++)
                {
                    bool helg = (tempDate.DayOfWeek == DayOfWeek.Saturday
                              || tempDate.DayOfWeek == DayOfWeek.Sunday);
                    belopp += helg ? bilInfo.KrHelg : bilInfo.KrDygn;
                    tempDate = tempDate.AddDays(1);
                }

                totalBelopp += belopp;
            }

            bool updateOk = await Transactor.UpdateSlutDatum(bokId, regNrToClose, newSlut);
            if (!updateOk)
            {
                Console.WriteLine("Fel vid uppdatering av slutdatum.");
                Console.ReadKey();
                return;
            }
            Console.WriteLine("Slutdatum uppdaterat för valda bilar.");

            if (totalBelopp > 0)
            {
                Console.WriteLine($"Totalt belopp att faktureras: {totalBelopp}");
                bool facturaSuccess = await SkapaFaktura(bokId, totalBelopp);
                if (!facturaSuccess)
                {
                    Console.WriteLine("Återställer bokning...");
                    bool reUpdateOk = await Transactor.UpdateSlutDatum(bokId, regNrToClose, null);
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
                Console.WriteLine("Ingen debitering behövs (0 kr).");
                Console.ReadKey();
            }

            Console.WriteLine("Återgå till huvudmeny (click)");
            Console.ReadKey();
        }

    }
}
