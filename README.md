# D0004N Datamodellering - Uppgift 1
Elias T�yr� <br>
En simpel SQL-klient.<br><br>
Anv�nder OS tid f�r att se tillg�ngligheten p� bilar, samt f�r att ber�kna kostnad p� fakturor etc. <br>
Om en bil "l�mnas in" i "framtiden", s� kommer den fortsatt att anses vara "uthyrd" (�ven om en faktura har skapats)
<br><br>
Inga funktioner f�r kontroller eller skador etc..

#### F�r att bygga skiten
    dotnet restore
    dotnet build

#### K�r programmet
    dotnet run

Se till att s�tta upp tables och �ndra <u><b>Transactor.DB</b></u> till att peka p� din DB.

#### Exempel p� Transaktion
    Green Rental DB - Huvudmeny
    Steg 1 - 3 kr�vs f�r att hyra en bil.
    1. Registrera bil
    2. Registrera station
    3. Registrera Personal
    4. Visa alla bilar + status
    5. Visa stationer
    6. Hyr ut Bil
    7. Inl�mning av bil
    q. Avsluta

    6
    �r det en f�retagskund? (J/N):
    N
    Personnummer: 200107304444
    Bokning skapad med BokningsId = 5.
    Vill du ange slutdatum (S), eller hyra l�pande fr.o.m. nu (L)?
    L
    Ange RegNr (eller l�mna tomt f�r att avsluta): LYE715
    Bil LYE715 lades till bokningen.
    Ange RegNr (eller l�mna tomt f�r att avsluta): DEX123
    Bil DEX123 lades till bokningen.
    Ange RegNr (eller l�mna tomt f�r att avsluta):
    Samtliga valda bilar lades till bokningen.
    Anst�llningsId f�r den som skriver avtal: 1
    Avtal signerat.


# Logisk Datamodell
![alt text](D0004N_Logisk.png)

# Konceptuell Datamodell
![alt text](D0004N_Konceptuell.png)