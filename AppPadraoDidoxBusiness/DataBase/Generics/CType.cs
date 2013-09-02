using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using System.Web;

namespace Didox.DataBase.Generics
{
    public class CType : BasicType
	{
        /// <summary>
        /// Usar este metodo somente quando estiver usando transação
        /// </summary>
        public void CloseTransaction()
        {
            if (this.Transaction != null)
            {
                if (this.Transaction.Connection != null)
                    this.Transaction.Connection.Close();
                this.Transaction.Dispose();
            }
        }

        /// <summary>
        /// Salva o registro através de uma referência do objeto
        /// Ex: usar no ObjectDataSource
        /// </summary>
        /// <param name="objGrupo">objeto grupo</param>
        public static IType Save(IType objiType)
        {
            //if (objiType.Id == null) objiType.DataCriacao = DateTime.Now;
            validateFields(objiType);
            new CData().Save(ref objiType);
            Cache.InvalidateByClass(objiType);
            return objiType;
        }

        private static void validateFields(IType objiType)
        {
            string invalidFields = "";
            foreach (PropertyInfo pi in objiType.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {                
                ValidateAttribute[] pValProperty = (ValidateAttribute[])pi.GetCustomAttributes(typeof(ValidateAttribute), false);
                PropertyAttribute[] pAttrProperty = (PropertyAttribute[])pi.GetCustomAttributes(typeof(PropertyAttribute), false);
                if (pAttrProperty != null && pAttrProperty.Length > 0)
                {
                    if (pAttrProperty[0].IsFieldBase)
                    {
                        var value = pi.GetValue(objiType, null);

                        if (pValProperty != null && pValProperty.Length > 0)
                        {
                            if (value == null) setDefaultValue(objiType, pAttrProperty, pi, ref invalidFields, ref value);
                            else if (value is string)
                                if (value.ToString() == "") setDefaultValue(objiType, pAttrProperty, pi, ref invalidFields, ref value);
                        }

                        if (!pAttrProperty[0].IsText && (value != null && value.ToString().Length > pAttrProperty[0].Size))
                            invalidFields += "O campo " + pi.Name + " está com o tamanho maior que o permitido. \n";                                                
                    }
                }
            }
            
            if (invalidFields.Length > 0)
                throw new DidoxFrameworkError("Por favor corrija os campos abaixo:\n" + invalidFields);
        }

        private static void setDefaultValue(IType objiType, PropertyAttribute[] pAttrProperty, PropertyInfo pi, ref string invalidFields, ref object value)
        {
            if (!string.IsNullOrEmpty(pAttrProperty[0].DefaultValue))
            {
                pi.SetValue(objiType, objiType.PrepareValueProperty(pAttrProperty[0].DefaultValue, pi.PropertyType), null);
                value = pi.GetValue(objiType, null);
            }
            else invalidFields += pi.Name + " Não pode ser vazio \n";
        }

        /// <summary>
        /// Exclui o registro através de uma referência do objeto
        /// Ex: usar no ObjectDataSource
        /// </summary>
        /// <param name="objGrupo">objeto grupo</param>
        public static void Delete(IType objiType)
        {
            new CData().Delete(ref objiType);
            //objiType = GetByObject(objiType);
            //objiType.DataExclusao = DateTime.Now;
            //new CData().Save(ref objiType);
            Cache.InvalidateByClass(objiType);
        }

        /// <summary>
        /// Retorna lista de grupo filtrando pelo objeto
        /// </summary>
        public static LIType Find(IType objiType)
        {
            if(objiType.Transaction != null)
                return new CData().Find(ref objiType);

            string key = CacheKey(objiType);
            var objCache = Cache.Get(key);
            if (objCache != null && !(objCache is LIType)) Cache.Invalidate(key);
            LIType listWithCache = (LIType)Cache.Get(key);
            if (listWithCache != null) return listWithCache;

            LIType list = new CData().Find(ref objiType);
            Cache.Add(key, list, DateTime.Now.AddMinutes(Cache.MinutesOfEspirationCache()));

            return list;
        }
        /// <summary>
        /// Retorna lista de grupo filtrando pelo objeto por condição
        /// </summary>
        public static LIType FindByConditions(IType objiType, string conditions)
        {
            if(objiType.Transaction != null)
                return new CData().FindByConditions(ref objiType, conditions);

            string key = CacheKey(objiType, conditions);
            var objCache = Cache.Get(key);
            if (objCache != null && !(objCache is LIType)) Cache.Invalidate(key);
            LIType listWithCache = (LIType)Cache.Get(key);
            if (listWithCache != null) return listWithCache;

            LIType list = new CData().FindByConditions(ref objiType, conditions);
            Cache.Add(key, list, DateTime.Now.AddMinutes(Cache.MinutesOfEspirationCache()));

            return list;
        }

        /// <summary>
        /// Retorna lista de grupo filtrando pelo objeto por SQL query
        /// </summary>
        public static LIType FindBySql(IType objiType, string sql)
        {
            sql = string.Format(sql, GetTableName(objiType));
            if (objiType.Transaction != null)
                return new CData().FindBySql(ref objiType, sql);

            string key = CacheKeySql(sql);
            var objCache = Cache.Get(key);
            if (objCache != null && !(objCache is LIType)) Cache.Invalidate(key);
            LIType listWithCache = (LIType)Cache.Get(key);
            if (listWithCache != null) return listWithCache;

            LIType list = new CData().FindBySql(ref objiType, sql);
            Cache.Add(key, list, DateTime.Now.AddMinutes(Cache.MinutesOfEspirationCache()));

            return list;
        }

        /// <summary>
        /// Executa codigo SQL
        /// </summary>
        public static void ExecSql(IType objiType, string sql)
        {
            sql = string.Format(sql, GetTableName(objiType));
            new CData().ExecSql(ref objiType, sql);
        }  
        
        /// <summary>
        /// Metodo interno para setar e retorna a referencia
        /// </summary>
        /// <param name="objGrupo">Referência para o objeto</param>
        protected internal static IType GetByObject(IType objiType)
        {
            if (fieldsNull(objiType))
            {
                objiType.Id = null;
                return objiType;
            }

            if (objiType.Transaction != null)
            {
                new CData().Get(ref objiType);
                return objiType;
            }

            string key = CacheKey(objiType);
            var objCache = Cache.Get(key);
            if (objCache != null)
            {
                if (objCache is CType) return (CType)objCache;
                else Cache.Invalidate(key);
            }

            new CData().Get(ref objiType);
            Cache.Add(key, objiType, DateTime.Now.AddMinutes(Cache.MinutesOfEspirationCache()));
            
            return objiType;                
        }

        private static bool fieldsNull(IType objiType)
        {
            bool idNull = true;
            foreach (PropertyInfo pi in objiType.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                OperationsAttribute[] pOperationProperty = (OperationsAttribute[])pi.GetCustomAttributes(typeof(OperationsAttribute), false);
                PropertyAttribute[] pAttrProperty = (PropertyAttribute[])pi.GetCustomAttributes(typeof(PropertyAttribute), false);
                if (pAttrProperty != null && pAttrProperty.Length > 0)
                {
                    if (pAttrProperty[0].IsFieldBase)
                    {
                        if (pOperationProperty != null && pOperationProperty.Length > 0 && pOperationProperty[0].UseGet)
                        {
                            if (pi.GetValue(objiType, null) != null)
                            {
                                idNull = false;
                                break;
                            }
                        }
                    }
                }
            }

            return idNull;
        }

        public static string CacheKey(IType cType)
        {
            return CacheKey(cType, string.Empty);
        }

        public static string CacheKey(IType cType, string conditions)
        {
            string key = Cache.KeyForClass(cType) + "[" + HttpUtility.UrlEncode(conditions) + "]";
            foreach (PropertyInfo pi in cType.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                PropertyAttribute[] pAttProperty = (PropertyAttribute[])pi.GetCustomAttributes(typeof(PropertyAttribute), false);
                if (pAttProperty != null && pAttProperty.Length > 0)
                {
                    if (pAttProperty[0].IsFieldBase)
                    {
                        object value = pi.GetValue(cType, null);
                        value = value is DateTime ? ((DateTime)value).ToString("dd/MM/yyy") : value;
                        key += "[Param[" + pi.Name + "]Value[" + value + "]]";
                    }
                }
            }
            return key;
        }

        public static string CacheKeySql(string conditions)
        {
            return "SQL[" + HttpUtility.UrlEncode(conditions) + "]";
        }

        public virtual void Commit()
        {
            if (this.Transaction == null) return;
            if (this.Transaction.Connection == null)
            {
                this.Transaction.Dispose();
                return;
            }
            if (this.Transaction != null)
                this.Transaction.Commit();
            CloseTransaction();
        }

        public virtual void Rollback()
        {
            if (this.Transaction == null) return;
            if (this.Transaction.Connection == null)
            {
                this.Transaction.Dispose();
                return;
            }

            if (this.Transaction != null)
                this.Transaction.Rollback();
            CloseTransaction();
        }

        public static string GetPropertyName(PropertyInfo pi)
        {
            PropertyAttribute[] pAttProperty = (PropertyAttribute[])pi.GetCustomAttributes(typeof(PropertyAttribute), false);
            if (pAttProperty != null && pAttProperty.Length > 0 && !string.IsNullOrEmpty(pAttProperty[0].Name))
                return pAttProperty[0].Name;
            return pi.Name;
        }

        public static string GetPropertyGenericTipo(IType iType)
        {
            foreach (PropertyInfo pi in iType.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                ColumnTypeAttribute[] pColumnTypeProperty = (ColumnTypeAttribute[])pi.GetCustomAttributes(typeof(ColumnTypeAttribute), false);
                if (pColumnTypeProperty != null && pColumnTypeProperty.Length > 0)
                    return GetPropertyName(pi);
            }
            return null;
        }

        public static string GetViewPath(IType iType)
        {
            MemberInfo info = iType.GetType();
            var tableAttributes = info.GetCustomAttributes(typeof(ViewAttribute), true);
            if (tableAttributes.Length > 0)
            {
                ViewAttribute pAttView = (ViewAttribute)tableAttributes[0];
                if (pAttView != null && !string.IsNullOrEmpty(pAttView.Path))
                    return pAttView.Path;
            }
            throw new DidoxFrameworkError("Path view não encontrado.");
        }

        public static string GetTableName(IType iType)
        {
            MemberInfo info = iType.GetType();
            var tableAttributes = info.GetCustomAttributes(typeof(TableAttribute), true);
            if (tableAttributes.Length > 0)
            {
                TableAttribute pAttTable = (TableAttribute)tableAttributes[0];
                if (pAttTable != null && !string.IsNullOrEmpty(pAttTable.Name))
                    return pAttTable.Name;
            }
            return "tb" + iType.GetType().Name;
        }

        public virtual void Get() { setProperty(GetByObject(this)); }
        public virtual LIType Find() { return Find(this); }
        public virtual LIType FindByConditions(string conditions) { return FindByConditions(this, conditions); }
        public virtual LIType FindBySql(string sql) { return FindBySql(this, sql); }
        public virtual void ExecSql(string sql) { ExecSql(this, sql); }
        public virtual void Save() { setProperty(Save(this)); }
        public virtual void Delete() { Delete(this); }
        public void CreateProcs() { CreateProcs(this); }
        public void CreateViews() { CreateViews(this); }
        public void CreateForeignKeys() { CreateForeignKeys(this); }
        public void DeleteForeignKeys() { DeleteForeignKeys(this); }
        public void CreateTable() { DeleteForeignKeys(this); CreateTable(this); }

        public void Scaffold()
        {
            CreateTable(this);
            CreateProcs(this);
            CreateForeignKeys(this);
            CreateViews(this);
        }

        private void setProperty(IType iType)
        {
            foreach (PropertyInfo pi in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                PropertyAttribute[] pAttProperty = (PropertyAttribute[])pi.GetCustomAttributes(typeof(PropertyAttribute), false);
                if (pAttProperty != null && pAttProperty.Length > 0)
                {
                    if (pAttProperty[0].IsPk && pi.GetValue(iType, null) == null) return;
                    if (pAttProperty[0].IsFieldBase)
                        pi.SetValue(this, iType.PrepareValueProperty(pi.GetValue(iType, null), pi.PropertyType), null);
                }
            }
        }
        
        public static void CreateTable(IType objiType)
        {
            new CData().CreateTable(objiType);
            Cache.InvalidateByClass(objiType);
        }

        public static void CreateProcs(IType objiType)
        {
            new CData().CreateProcs(objiType);
            Cache.InvalidateByClass(objiType);
        }

        public static void CreateViews(IType objiType)
        {
            new CData().CreateViews(objiType);
        }

        public static void CreateForeignKeys(IType objiType)
        {
            new CData().CreateForeignKeys(objiType);
        }

        public static void DeleteForeignKeys(IType objiType)
        {
            new CData().DeleteForeignKeys(objiType);
        }

        [Property(IsField = true, Name = "DataCriacao")]
        internal DateTime? DataCriacaoInterno { get; set; }
        public DateTime? DataCriacao { get { return DataCriacaoInterno; } }

        [Property(IsField = true, Name = "DataAtualizacao")]
        internal DateTime? DataAtualizacaoInterno { get; set; }
        public DateTime? DataAtualizacao { get { return DataAtualizacaoInterno; } }
	}
}
