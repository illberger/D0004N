USE [D0004N]
GO

/****** Object:  Table [dbo].[Kunder]    Script Date: 2025-03-17 18:34:57 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Kunder](
	[KundId] [int] NOT NULL,
	[Personnummer] [nchar](12) NOT NULL,
 CONSTRAINT [PK_Kunder] PRIMARY KEY CLUSTERED 
(
	[KundId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY],
 CONSTRAINT [UQ_Kunder_Personnummer] UNIQUE NONCLUSTERED 
(
	[Personnummer] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Kunder]  WITH CHECK ADD  CONSTRAINT [FK_Kunder_Kund] FOREIGN KEY([Personnummer])
REFERENCES [dbo].[Kund] ([Personnummer])
GO

ALTER TABLE [dbo].[Kunder] CHECK CONSTRAINT [FK_Kunder_Kund]
GO

