USE [master]
GO

IF db_id('MoviesDb') IS NULL
  CREATE DATABASE MoviesDb
GO

USE [MoviesDb]
GO


DROP TABLE IF EXISTS [Movie];
DROP TABLE IF EXISTS [Genre];


CREATE TABLE [Genre] (
    [Id] INTEGER IDENTITY PRIMARY KEY NOT NULL,
    [Name] NVARCHAR(20) NOT NULL
)

CREATE TABLE [Movie] (
    [Id] INTEGER IDENTITY NOT NULL,
    [Title] NVARCHAR(255) NOT NULL,
    [Year] INTEGER NOT NULL,
    [Url] NVARCHAR(255) NOT NULL,
    [Rating] DECIMAL NOT NULL,
    [GenreId] INTEGER NOT NULL,

    CONSTRAINT Fk_Movie_Genre FOREIGN KEY (GenreId) REFERENCES Genre(Id)
)


SET IDENTITY_INSERT [Genre] ON
INSERT INTO [Genre] 
    ([Id], [Name])
VALUES
--GENRE_INSERTS
SET IDENTITY_INSERT [Genre] OFF

SET IDENTITY_INSERT [Movie] ON
INSERT INTO [Movie]
    ([Id], [Title], [Year], [Url], [Rating], [GenreId])
VALUES
--MOVIE_INSERTS
SET IDENTITY_INSERT [Movie] OFF

