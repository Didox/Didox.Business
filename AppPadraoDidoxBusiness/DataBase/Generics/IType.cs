using System;
using System.Data.Common;

namespace Didox.DataBase.Generics
{
    public interface IType : IDisposable
    {
        DbTransaction Transaction { get;set;}
        bool IsTransaction { get;set;}
        bool IsFull { get;set;}
        object PrepareValueProperty(object value, Type type);
        int? Id { get; set; }
        string InstanceConectionString { get; set; }
    }
}
