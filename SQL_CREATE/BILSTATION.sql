USE [D0004N]
GO

/****** Object:  Table [dbo].[BilStation]    Script Date: 2025-03-12 13:44:23 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BilStation](
	[RegNr] [nchar](6) NOT NULL,
	[StationId] [int] NOT NULL,
 CONSTRAINT [PK_BilStation] PRIMARY KEY CLUSTERED 
(
	[RegNr] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[BilStation]  WITH CHECK ADD  CONSTRAINT [FK_BilStation_Station] FOREIGN KEY([StationId])
REFERENCES [dbo].[Station] ([StationId])
GO

ALTER TABLE [dbo].[BilStation] CHECK CONSTRAINT [FK_BilStation_Station]
GO

