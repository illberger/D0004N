namespace D0004N
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Green Rental DB\n" +
                "1. \n" +
                "2. \n" +
                "3. \n" +
                "4. \n" +
                "5. \n" +
                "6. \n" +
                "7. \n" +
                "8. \n" +
                "q. QUIT\n");
        }
    }
}





static class Transactor
{
    const string DB = "Server=localhost;Database=D0004N;Trusted_Connection=True;TrustServerCertificate=True";

   
    public static async Task<List<Författare>> QueryNr1()
    {
        List<Författare> result = new List<Författare>();
        try
        {
            using (var conn = new SqlConnection(DB))
            {
                await conn.OpenAsync();
                string transaction = @"
                SELECT Pnr, EfterNamn, FörNamn
                FROM dbo.Författare
                WHERE (2003 - FödelseÅr) = 90
                AND Kön = 'M';
                ";

                using (var cmd = new SqlCommand(transaction, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var f = new Författare
                            {
                                Pnr = reader["Pnr"].ToString(),
                                EfterNamn = reader["EfterNamn"].ToString(),
                                FörNamn = reader["FörNamn"].ToString()
                            };
                            result.Add(f);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"{e.Message}");
            return null;
        }
        return result;
    }

    public static async Task<List<Författare>> QueryNr2()
    {
        List<Författare> result = new List<Författare>();
        try
        {
            using (var conn = new SqlConnection(DB))
            {
                await conn.OpenAsync();
                string transaction = @"
                SELECT DISTINCT f.Pnr, f.EfterNamn
                FROM dbo.Författare AS f
                JOIN dbo.Bok as b
                ON b.Författare = f.Pnr
                WHERE b.AntalSidor > 1000;
                ";

                using (var cmd = new SqlCommand(transaction, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var f = new Författare
                            {
                                Pnr = reader["Pnr"].ToString(),
                                EfterNamn = reader["EfterNamn"].ToString(),
                            };
                            result.Add(f);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"{e.Message}");
            return null;
        }
        return result;

    }


    public static async Task<List<Bok>> QueryNr3()
    {
        List<Bok> result = new List<Bok>();
        try
        {
            using (var conn = new SqlConnection(DB))
            {
                await conn.OpenAsync();
                string transaction = @"
                SELECT b.IsbnNr, b.Titel
                FROM dbo.Bok AS b
                JOIN dbo.Ämne as ä
                    ON b.Ämne = ä.Ämnenr
                WHERE ä.ÄmneText = 'GIS'
                    AND b.UtgivningsÅr = 2001;
                ";

                using (var cmd = new SqlCommand(transaction, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var b = new Bok
                            {
                                IsbnNr = reader["IsbnrNr"].ToString(),
                                Titel = reader["Titel"].ToString(),
                            };
                            result.Add(b);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"{e.Message}");
            return null;
        }
        return result;

    }


    public static async Task<List<Ämne>> QueryNr4()
    {
        List<Ämne> result = new List<Ämne>();
        try
        {
            using (var conn = new SqlConnection(DB))
            {
                await conn.OpenAsync();
                string transaction = @"
                SELECT ä.ÄmneText
                FROM dbo.Ämne AS ä
                LEFT JOIN dbo.Bok as b
                    ON ä.Ämnenr = b.Ämne
                WHERE b.Ämne IS NULL;
                ";

                using (var cmd = new SqlCommand(transaction, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var ä = new Ämne
                            {
                                ÄmneText = reader["Ämnetext"].ToString()
                            };
                            result.Add(ä);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"{e.Message}");
            return null;
        }
        return result;

    }


    public static async Task<List<Bok>> QueryNr5()
    {
        List<Bok> result = new List<Bok>();
        try
        {
            using (var conn = new SqlConnection(DB))
            {
                await conn.OpenAsync();
                string transaction = @"
                SELECT *
                FROM dbo.Bok
                WHERE UtgivningsÅr < 1960;
                ";

                string returneraEttHeltalFrånDBMS = @" 
                SELECT COUNT(*) AS n
                FROM dbo.Bok
                WHERE UtgivningsÅr < 1960;
                ";

                using (var cmd = new SqlCommand(transaction, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var b = new Bok
                            {
                                Titel = reader["Titel"].ToString(),
                                IsbnNr = reader["IsbnNr"].ToString()
                            };
                            result.Add(b);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"{e.Message}");
            return null;
        }
        return result;

    }



    public static async Task<List<Författare>> QueryNr6()
    {
        List<Författare> result = new List<Författare>();
        try
        {
            using (var conn = new SqlConnection(DB))
            {
                await conn.OpenAsync();
                string transaction = @"
                SELECT f.Pnr,
                       f.FörNamn,
                       f.EfterNamn
                FROM dbo.Författare AS f
                JOIN dbo.Bok AS b
                    ON f.Pnr = b.Författare
                GROUP BY f.Pnr, f.FörNamn, f.EfterNamn
                HAVING COUNT(*) > 3;
                ";

                using (var cmd = new SqlCommand(transaction, conn))
                {
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var f = new Författare
                            {
                                Pnr = reader["Pnr"].ToString(),
                                EfterNamn = reader["EfterNamn"].ToString(),
                                FörNamn = reader["FörNamn"].ToString()
                            };
                            result.Add(f);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"{e.Message}");
            return null;
        }
        return result;

    }
}