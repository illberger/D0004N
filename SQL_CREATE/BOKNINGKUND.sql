USE [D0004N]
GO

/****** Object:  Table [dbo].[BokningKund]    Script Date: 2025-03-17 18:35:50 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BokningKund](
	[BokningsId] [int] NOT NULL,
	[KundId] [int] NOT NULL,
 CONSTRAINT [PK_BokningKund] PRIMARY KEY CLUSTERED 
(
	[BokningsId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[BokningKund]  WITH CHECK ADD  CONSTRAINT [FK_BokningKund_Kunder] FOREIGN KEY([KundId])
REFERENCES [dbo].[Kunder] ([KundId])
GO

ALTER TABLE [dbo].[BokningKund] CHECK CONSTRAINT [FK_BokningKund_Kunder]
GO

