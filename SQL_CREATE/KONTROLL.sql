USE [D0004N]
GO

/****** Object:  Table [dbo].[Kontroll]    Script Date: 2025-03-17 18:35:16 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Kontroll](
	[RegNr] [nchar](6) NOT NULL,
	[BokningsId] [int] NOT NULL,
	[AntallningsId] [int] NOT NULL,
	[MatarStallning] [int] NOT NULL,
	[Filepath] [nvarchar](100) NULL,
 CONSTRAINT [PK_Kontroll] PRIMARY KEY CLUSTERED 
(
	[RegNr] ASC,
	[BokningsId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Kontroll]  WITH CHECK ADD  CONSTRAINT [FK_Kontroll_Anstalld] FOREIGN KEY([AntallningsId])
REFERENCES [dbo].[Anstalld] ([AnstallningsId])
GO

ALTER TABLE [dbo].[Kontroll] CHECK CONSTRAINT [FK_Kontroll_Anstalld]
GO

ALTER TABLE [dbo].[Kontroll]  WITH CHECK ADD  CONSTRAINT [FK_Kontroll_BokningBil] FOREIGN KEY([BokningsId], [RegNr])
REFERENCES [dbo].[BokningBil] ([BokningsId], [RegNr])
GO

ALTER TABLE [dbo].[Kontroll] CHECK CONSTRAINT [FK_Kontroll_BokningBil]
GO

