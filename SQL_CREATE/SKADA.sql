USE [D0004N]
GO

/****** Object:  Table [dbo].[Skada]    Script Date: 2025-03-17 18:34:47 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Skada](
	[SkadaId] [int] NOT NULL,
	[BokningsId] [int] NOT NULL,
	[RegNr] [nchar](6) NOT NULL,
	[DatumFixad] [date] NULL,
	[Beskrivning] [nvarchar](max) NULL,
 CONSTRAINT [PK_Skada] PRIMARY KEY CLUSTERED 
(
	[SkadaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Skada]  WITH CHECK ADD  CONSTRAINT [FK_Skada_Kontroll] FOREIGN KEY([RegNr], [BokningsId])
REFERENCES [dbo].[Kontroll] ([RegNr], [BokningsId])
GO

ALTER TABLE [dbo].[Skada] CHECK CONSTRAINT [FK_Skada_Kontroll]
GO

