using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Onwrd.EntityFrameworkCore.Internal.Migrations
{
    internal interface IOnwrdMigration
    {
        MigrationContext Context { get; set; }
    }
}
