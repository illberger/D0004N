USE [D0004N]
GO

/****** Object:  Table [dbo].[BokningBil]    Script Date: 2025-03-17 18:36:14 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BokningBil](
	[BokningsId] [int] NOT NULL,
	[RegNr] [nchar](6) NOT NULL,
	[StartDatum] [date] NOT NULL,
	[SlutDatum] [date] NULL,
 CONSTRAINT [PK_BokningBil] PRIMARY KEY CLUSTERED 
(
	[BokningsId] ASC,
	[RegNr] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[BokningBil]  WITH CHECK ADD  CONSTRAINT [FK_BokningBil_Bil] FOREIGN KEY([RegNr])
REFERENCES [dbo].[Bil] ([RegNr])
ON DELETE CASCADE
GO

ALTER TABLE [dbo].[BokningBil] CHECK CONSTRAINT [FK_BokningBil_Bil]
GO

ALTER TABLE [dbo].[BokningBil]  WITH CHECK ADD  CONSTRAINT [FK_BokningBil_BokningKund] FOREIGN KEY([BokningsId])
REFERENCES [dbo].[BokningKund] ([BokningsId])
GO

ALTER TABLE [dbo].[BokningBil] CHECK CONSTRAINT [FK_BokningBil_BokningKund]
GO

