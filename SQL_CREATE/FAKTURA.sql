USE [D0004N]
GO

/****** Object:  Table [dbo].[Faktura]    Script Date: 2025-03-12 14:43:11 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Faktura](
	[FakturaNr] [bigint] NOT NULL,
	[FakturaDatum] [date] NOT NULL,
	[Belopp] [decimal](18, 2) NOT NULL,
	[ForfalloDatum] [date] NOT NULL,
	[Filepath] [nvarchar](100) NULL,
	[Status] [bit] NOT NULL,
 CONSTRAINT [PK_Faktura] PRIMARY KEY CLUSTERED 
(
	[FakturaNr] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

