USE [D0004N]
GO

/****** Object:  Table [dbo].[Anstalld]    Script Date: 2025-03-17 15:42:52 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Anstalld](
	[AnstallningsId] [int] NOT NULL,
	[Behorighet] [int] NOT NULL,
	[Fornamn] [nvarchar](50) NOT NULL,
	[Efternamn] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Anstalld] PRIMARY KEY CLUSTERED 
(
	[AnstallningsId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

INSERT INTO [dbo].[Anstalld] 
    (AnstallningsId, Behorighet, Fornamn, Efternamn)
VALUES
    (0, 0, 'UnderHåll', 'Persson'),  -- UnderHåll = behörighet 0
    (1, 1, 'Uthyrning', 'Jansson');  -- Uthyrning = behörighet 1
GO