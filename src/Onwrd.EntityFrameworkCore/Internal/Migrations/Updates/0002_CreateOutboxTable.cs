using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onwrd.EntityFrameworkCore.Internal.Migrations.Updates
{
    internal class CreateOutboxTable : IOnwrdMigrationUpdate
    {
        public string GetSqlForMsSqlServer()
        {
            return @"
                CREATE TABLE Onwrd.Outbox
                (
	                Id UNIQUEIDENTIFIER NOT NULL,
	                CreatedOn datetime2 NOT NULL,
	                DispatchedOn datetime2 NULL,
	                TypeId NVARCHAR(MAX) NOT NULL,
	                Contents NVARCHAR(MAX) NOT NULL,
	                CONSTRAINT PK_MyTable PRIMARY KEY CLUSTERED (Id),
                    INDEX IX_Onwrd_Outbox_DispatchedOn NONCLUSTERED(DispatchedOn)
                )

                GO
            ";
        }

        public string GetSqlForPostgreSql()
        {
            return
                @"CREATE TABLE Onwrd.Outbox
                (
	                Id CHAR(36) NOT NULL,
	                CreatedOn timestamp(6) NOT NULL,
	                DispatchedOn timestamp(6) NULL,
	                TypeId TEXT NOT NULL,
	                Contents TEXT NOT NULL,
	                CONSTRAINT PK_Onwrd_Outbox PRIMARY KEY (Id)
                );

                CREATE INDEX IX_Onwrd_Outbox_DispatchedOn ON Onwrd.Outbox USING btree (DispatchedOn);
            ";
        }
    }
}