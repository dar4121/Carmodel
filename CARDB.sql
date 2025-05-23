USE [CarDB]
GO
/****** Object:  Table [dbo].[tblBrand]    Script Date: 5/4/2025 1:56:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblBrand](
	[BrandID] [int] IDENTITY(1,1) NOT NULL,
	[BrandName] [nvarchar](50) NULL,
 CONSTRAINT [PK_tblBrand] PRIMARY KEY CLUSTERED 
(
	[BrandID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblCarModel]    Script Date: 5/4/2025 1:56:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblCarModel](
	[ModelID] [int] IDENTITY(1,1) NOT NULL,
	[BrandID] [int] NULL,
	[ClassID] [int] NULL,
	[ModelName] [nvarchar](50) NULL,
	[ModelCode] [nvarchar](50) NULL,
	[Description] [nvarchar](max) NULL,
	[Features] [nvarchar](max) NULL,
	[Price] [decimal](18, 2) NULL,
	[DateofManufacturing] [datetime] NULL,
	[IsActive] [bit] NULL,
	[IsDelete] [bit] NULL,
	[Sortorder] [int] NULL,
 CONSTRAINT [PK_tblCarModel] PRIMARY KEY CLUSTERED 
(
	[ModelID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblClass]    Script Date: 5/4/2025 1:56:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblClass](
	[ClassID] [int] IDENTITY(1,1) NOT NULL,
	[ClassName] [nvarchar](50) NULL,
 CONSTRAINT [PK_tblClass] PRIMARY KEY CLUSTERED 
(
	[ClassID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[tblImages]    Script Date: 5/4/2025 1:56:46 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tblImages](
	[ImageID] [int] IDENTITY(1,1) NOT NULL,
	[ModelID] [int] NULL,
	[ImageName] [nvarchar](max) NULL,
	[IsDefault] [bit] NULL,
 CONSTRAINT [PK_tblImages] PRIMARY KEY CLUSTERED 
(
	[ImageID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO


INSERT INTO [dbo].[tblBrand] ([BrandName]) VALUES 
('Audi'),
('Jaguar'),
('Land Rover'),
('Renault');

INSERT INTO [dbo].[tblClass] ([ClassName]) VALUES 
('A-Class'),
('B-Class'),
('C-Class');


Scaffold-DbContext "Server= LAPTOP-T4723NN2;Database=EmployeeDB;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Context "EmpContext" -force