USE [D0004N]
GO

/****** Object:  Table [dbo].[Foretag]    Script Date: 2025-03-17 18:35:25 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Foretag](
	[OrgNr] [nchar](10) NOT NULL,
	[ForetagsNamn] [nvarchar](50) NOT NULL,
	[Adress] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Företag] PRIMARY KEY CLUSTERED 
(
	[OrgNr] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

