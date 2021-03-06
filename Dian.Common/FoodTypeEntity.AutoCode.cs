using System;
using System.Data;
using System.Data.Common;
using CSN.DotNetLibrary.Common;

namespace Dian.Common.Entity
{
    partial class FoodTypeEntity
    {

        [Field("FOOD_TYPE_ID", FieldDBType = DbType.Int32, FieldDesc = "", IsIdentityField = true, IsPrimaryKey = true)]
        public int? FOOD_TYPE_ID { get; set; }

        [Field("FOOD_TYPE_NAME", FieldDBType = DbType.AnsiString, FieldDesc = "", IsIdentityField = false, IsPrimaryKey = false)]
        public string FOOD_TYPE_NAME { get; set; }

        //[Field("RESTAURANT_ID", FieldDBType = DbType.Int32, FieldDesc = "", IsIdentityField = false, IsPrimaryKey = false)]
        //public int? RESTAURANT_ID { get; set; }

    }
}
