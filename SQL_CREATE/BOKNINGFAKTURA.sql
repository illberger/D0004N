USE [D0004N]
GO

/****** Object:  Table [dbo].[BokningFaktura]    Script Date: 2025-03-12 13:45:10 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[BokningFaktura](
	[BokningsId] [int] NOT NULL,
	[FakturaNr] [bigint] NOT NULL,
 CONSTRAINT [PK__BokningF__96C0A3F8E4E8F694] PRIMARY KEY CLUSTERED 
(
	[BokningsId] ASC,
	[FakturaNr] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[BokningFaktura]  WITH CHECK ADD  CONSTRAINT [FK_BokningFaktura_BokningKund] FOREIGN KEY([BokningsId])
REFERENCES [dbo].[BokningKund] ([BokningsId])
GO

ALTER TABLE [dbo].[BokningFaktura] CHECK CONSTRAINT [FK_BokningFaktura_BokningKund]
GO

ALTER TABLE [dbo].[BokningFaktura]  WITH CHECK ADD  CONSTRAINT [FK_BokningFaktura_Faktura] FOREIGN KEY([FakturaNr])
REFERENCES [dbo].[Faktura] ([FakturaNr])
GO

ALTER TABLE [dbo].[BokningFaktura] CHECK CONSTRAINT [FK_BokningFaktura_Faktura]
GO

