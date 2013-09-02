using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using System.Web;

namespace Didox.DataBase.Generics
{
    public class BasicType : IType
	{
        private DbTransaction _transaction;
        private bool _isTransaction;
        private bool _isFull;

        /// <summary>
        /// Seta ou Repassa a trasação para os outros objetos
        /// </summary>
        public DbTransaction Transaction
        {
            get { return this._transaction; }
            set { this._transaction = value; }
        }
        /// <summary>
        /// Inicia a transação no objeto, ao utilizar sempre finalize com Transaction.Commit ou Transaction.Rollback
        /// </summary>
        public bool IsTransaction
        {
            get { return this._isTransaction; }
            set { this._isTransaction = value; }
        }
        /// <summary>
        /// Verifica se o objeto está preenchido, false: preenche o objeto com os dados da base, true: Utiliza o objeto atual
        /// </summary>
        public bool IsFull
        {
            get { return this._isFull; }
            set { this._isFull = value; }
        }
        
        /// <summary>
        /// Metodo que libera o objeto da memória
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public string InstanceConectionString { get; set; }

        protected void CarregarConectionString(IType iType)
        {
            if (iType == null) return;
            InstanceConectionString = iType.GetType().GetMethod("ConectionString").Invoke(iType, null).ToString();
        }

        /// <summary>
        /// Seta seta o valor para a propriedade
        /// </summary>
        /// <param name="object">valor do banco de dados</param>
        /// <param name="iType">interface do objeto</param>      
        public object PrepareValueProperty(object value, Type type)
        {
            if (value == null && type.IsGenericType) return Activator.CreateInstance(type);
            if (value == null || value == DBNull.Value) return null;
            if (type == value.GetType()) return value;

            if (type.IsEnum || value is Enum)
            {
                if (value is string)
                    return Enum.Parse(type, value as string);
                else
                    return Enum.ToObject(type, value);
            }

            if (!type.IsInterface && type.IsGenericType)
            {
                Type innerType = type.GetGenericArguments()[0];
                object innerValue = PrepareValueProperty(value, innerType);
                return Activator.CreateInstance(type, new object[] { innerValue });
            }

            if (value is string && type == typeof(Guid)) return new Guid(value as string);
            if (type == typeof(Boolean)) return Convert.ToBoolean(int.Parse(value.ToString()));
            if (value is string && type == typeof(Version)) return new Version(value as string);
            if (type == typeof(Decimal))
            {
                var style = NumberStyles.Number | NumberStyles.AllowCurrencySymbol;
                var provider = new CultureInfo("en-US");
                return Decimal.Parse(value.ToString(), style, provider);
            }
            if (!(value is IConvertible)) return value;

            return Convert.ChangeType(value, type);
        }


        public int? Id
        {
            get
            {
                foreach (PropertyInfo pi in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    PropertyAttribute[] pAttProperty = (PropertyAttribute[])pi.GetCustomAttributes(typeof(PropertyAttribute), false);
                    if (pAttProperty != null && pAttProperty.Length > 0)
                    {
                        if (pAttProperty[0].IsPk) return (int?)pi.GetValue(this, null);
                    }
                }
                return null;
            }
            set
            {
                foreach (PropertyInfo pi in this.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    PropertyAttribute[] pAttProperty = (PropertyAttribute[])pi.GetCustomAttributes(typeof(PropertyAttribute), false);
                    if (pAttProperty != null && pAttProperty.Length > 0)
                    {
                        if (pAttProperty[0].IsPk)
                        {
                            pi.SetValue(this, this.PrepareValueProperty(value, pi.PropertyType), null);
                            break;
                        }
                    }
                }
            }
        }
        
	}
}
