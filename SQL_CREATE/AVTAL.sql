USE [D0004N]
GO

/****** Object:  Table [dbo].[Avtal]    Script Date: 2025-03-12 13:44:06 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Avtal](
	[AnstallningsId] [int] NOT NULL,
	[BokningsId] [int] NOT NULL,
	[Filepath] [nvarchar](100) NULL,
 CONSTRAINT [PK_Avtal] PRIMARY KEY CLUSTERED 
(
	[BokningsId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Avtal]  WITH CHECK ADD  CONSTRAINT [FK_Avtal_Anstalld] FOREIGN KEY([AnstallningsId])
REFERENCES [dbo].[Anstalld] ([AnstallningsId])
GO

ALTER TABLE [dbo].[Avtal] CHECK CONSTRAINT [FK_Avtal_Anstalld]
GO

ALTER TABLE [dbo].[Avtal]  WITH CHECK ADD  CONSTRAINT [FK_Avtal_BokningKund] FOREIGN KEY([BokningsId])
REFERENCES [dbo].[BokningKund] ([BokningsId])
GO

ALTER TABLE [dbo].[Avtal] CHECK CONSTRAINT [FK_Avtal_BokningKund]
GO

