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
                @"CREATE TABLE onwrd.events
                (
	                id CHAR(36) NOT NULL,
	                created_on timestamp(6) NOT NULL,
	                dispatched_on timestamp(6) NULL,
	                type_id TEXT NOT NULL,
	                contents TEXT NOT NULL,
	                CONSTRAINT PK_onwrd_events PRIMARY KEY (Id)
                );

                CREATE INDEX IX_onwrd_events_dispatched_on ON onwrd.events USING btree (dispatched_on);
            ";
        }
    }
}