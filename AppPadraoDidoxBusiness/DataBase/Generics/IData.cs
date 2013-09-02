using System;
using System.Data.Common;

namespace Didox.DataBase.Generics
{
	public interface IData
	{
        IType SetObject(DbDataReader dr, IType iType);
        void Save(ref IType iType);
        void Delete(ref IType iType);
        LIType Find(ref IType iType);
        void Get(ref IType iType);
        //void CreateTable(IType iType);
        void CreateProcs(IType iType);
	}
}
