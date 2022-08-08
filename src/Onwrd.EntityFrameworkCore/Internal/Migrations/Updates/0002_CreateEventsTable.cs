using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onwrd.EntityFrameworkCore.Internal.Migrations.Updates
{
    internal class CreateEventsTable : IOnwrdMigrationUpdate
    {
        public string GetSqlForMsSqlServer()
        {
            return @"
                CREATE TABLE Onwrd.Events
                (
	                Id UNIQUEIDENTIFIER NOT NULL,
	                CreatedOn datetime2 NOT NULL,
	                DispatchedOn datetime2 NULL,
	                TypeId NVARCHAR(MAX) NOT NULL,
	                Contents NVARCHAR(MAX) NOT NULL,
	                CONSTRAINT PK_Onwrd_Events PRIMARY KEY CLUSTERED (Id),
                    INDEX IX_Onwrd_Events_DispatchedOn NONCLUSTERED(DispatchedOn)
                )

                GO
            ";
        }

        public string GetSqlForPostgreSql()
        {
            return
                @"CREATE TABLE Onwrd.Events
                (
	                Id CHAR(36) NOT NULL,
	                CreatedOn timestamp(6) NOT NULL,
	                DispatchedOn timestamp(6) NULL,
	                TypeId TEXT NOT NULL,
	                Contents TEXT NOT NULL,
	                CONSTRAINT PK_Onwrd_Events PRIMARY KEY (Id)
                );

                CREATE INDEX IX_Onwrd_Events_DispatchedOn ON Onwrd.Events USING btree (DispatchedOn);
            ";
        }
    }
}