using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace WeihanLi.EntityFramework.SqlServer
{
    public static class SqlDbFunctionExtensions
    {
        [DbFunction("JSON_VALUE", "")]
        public static string JsonValue(string column, [NotParameterized] string path)
        {
            throw new NotSupportedException();
        }
    }
}
