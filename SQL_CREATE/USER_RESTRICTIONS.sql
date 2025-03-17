-- ÄNDRA DB-variabeln vid behov
USE [D0004N]
GO

CREATE LOGIN [KontrollLogin] WITH PASSWORD='Kontroll123!', DEFAULT_DATABASE=[D0004N], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=ON
GO

ALTER LOGIN [KontrollLogin] DISABLE
GO

CREATE LOGIN [AvtalLogin] WITH PASSWORD='Avtal123!', DEFAULT_DATABASE=[D0004N], DEFAULT_LANGUAGE=[us_english], CHECK_EXPIRATION=OFF, CHECK_POLICY=ON
GO

ALTER LOGIN [AvtalLogin] DISABLE
GO

CREATE USER KontrollUser FOR LOGIN KontrollLogin;
CREATE USER AvtalUser FOR LOGIN AvtalLogin;
GO

-- Se till att du har lagt upp tables redan
-- Kontrolluser (underhållspersonal)
GRANT SELECT ON dbo.BokningBil TO KontrollUser;
GRANT SELECT ON dbo.BilStation TO KontrollUser;
GRANT SELECT ON dbo.Station TO KontrollUser;
GRANT SELECT ON dbo.Bil TO KontrollUser;
GRANT SELECT ON dbo.BilTyp TO KontrollUser;
GRANT SELECT, INSERT, UPDATE ON dbo.Kontroll TO KontrollUser;
GRANT SELECT, INSERT, UPDATE ON dbo.Skada TO KontrollUser;

REVOKE INSERT, SELECT ON dbo.BokningKund FROM KontrollUser;
REVOKE INSERT, SELECT ON dbo.Kund FROM KontrollUser;
REVOKE INSERT, SELECT ON dbo.Foretag FROM KontrollUser;
REVOKE INSERT, SELECT ON dbo.Kunder FROM KontrollUser;
REVOKE INSERT, SELECT ON dbo.Faktura FROM KontrollUser;
REVOKE INSERT, SELECT ON dbo.Avtal FROM KontrollUser;
REVOKE INSERT, SELECT ON dbo.Anstalld FROM KontrollUser;
REVOKE INSERT, SELECT ON dbo.BokningFaktura FROM KontrollUser;

-- Avtaluser (Uthyrningspersonal)
GRANT SELECT, INSERT, UPDATE ON dbo.Kunder TO AvtalUser;
GRANT SELECT, INSERT, UPDATE ON dbo.Kund TO AvtalUser;
GRANT SELECT, INSERT, UPDATE ON dbo.Foretag TO AvtalUser;
GRANT SELECT, INSERT, UPDATE ON dbo.Bil TO AvtalUser;
GRANT SELECT, INSERT, UPDATE ON dbo.BokningBil TO AvtalUser;
GRANT SELECT, INSERT, UPDATE ON dbo.BokningKund TO AvtalUser;
GRANT SELECT, INSERT, UPDATE ON dbo.Faktura TO AvtalUser;
GRANT SELECT, INSERT, UPDATE ON dbo.BilStation TO AvtalUser;
GRANT SELECT, INSERT, UPDATE ON dbo.Station TO AvtalUser;
GRANT SELECT, INSERT, UPDATE ON dbo.Avtal TO AvtalUser;
GRANT SELECT, INSERT, UPDATE ON dbo.BilTyp TO AvtalUser;
GRANT SELECT, INSERT, UPDATE ON dbo.BokningFaktura TO AvtalUser;
GRANT SELECT ON dbo.Anstalld TO AvtalUser;

REVOKE INSERT, UPDATE ON dbo.Anstalld FROM AvtalUser;
