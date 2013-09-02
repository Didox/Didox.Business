using System;
using System.Data;
using System.Data.Common;
using System.Configuration;
using System.Reflection;
using Didox.DataBase.Generics.DataManager;
using System.Collections.Generic;
using System.IO;

namespace Didox.DataBase.Generics
{
	public class CData : IData
	{
        public CData() { }

        private const string EXCEPTIONMESSAGE = "Não foi possível criar uma Conexão. \r" +
                        "É necessário ter um <connectionString/> em seu App.Config ou Web.config com os seguintes valores.\r\r" +
                        "<connectionStrings>\r" +
                        "   <add name=\"connectionString\" connectionString=\"connectionString do Banco de Dados\" ProviderName=\"providerName\"/>\r" +
                        "</connectionStrings>\r\r" +
                        "Onde o atributo Name deve ser exatamente connectionString, o connectionString deve conter a string de conexão ao banco" +
                        " e ProviderName deve conter um dos seguintes nomes: \rSqlServer \rMsAccess.\r";

        private DbConnection _cn = null;
        private DbCommand _cmd = null;
        /// <summary>
        /// Controla se o campo primary key fou preenchido, usado para delete
        /// </summary>
        private bool pkValue = false;

        protected internal DbConnection Cn
        {
            get { return _cn; }
            set { _cn = value; }
        }

        protected internal DbCommand Cmd
        {
            get { return _cmd; }
            set { _cmd = value; }
        }
        /// <summary>
        /// Retorna a conexao com o banco
        /// </summary>
        protected internal string ConnectionString
        {
            get
            {
                try
                {
                    return ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
                }
                catch(Exception ex) { throw new DidoxFrameworkError(ex.Message); }
            }
        }
        /// <summary>
        /// Retorna o provider do web.config
        /// </summary>
        protected internal DataBaseType BaseType
        {
            get
            {
                try
                {
                    return (DataBaseType)Enum.Parse(typeof(DataBaseType),
                        ConfigurationManager.ConnectionStrings["connectionString"].ProviderName);
                }
                catch (Exception ex) { throw new DidoxFrameworkError(ex.Message); }
                
            }
        }

        /// <summary>
        /// Abre a conexão com o banco de dados
        /// </summary>
        protected internal void OpenConnection(IType iType)
        {
            string conectionString = ConnectionString;
            if (iType != null)
            {
                string iTypeConnection = getConectionStringFromIType(iType);
                if (iTypeConnection != null)
                    conectionString = iTypeConnection;
            }

            Cn = DataBaseGeneric.CreateConnection(BaseType, conectionString);
            Cn.Open();
            Cmd.Connection = Cn;
        }

        /// <summary>
        /// Fecha a conexão com o banco de dados
        /// </summary>
        protected internal void CloseConnection(IType iType)
        {
            if (Cn != null)
            {
                if (Cn.State == ConnectionState.Open && iType.Transaction == null)
                {
                    Cn.Close();
                    Cn.Dispose();
                }
            }
        }

        /// <summary>
        /// Abre a conexão ultilizando transação
        /// </summary>
        protected internal void OpenConnectionTrans(IType iType)
        {
            if (iType.IsTransaction || iType.Transaction != null)
            {
                if (iType.Transaction == null || iType.Transaction.Connection == null)
                {
                    OpenConnection(iType);
                    Cmd.Transaction = Cmd.Connection.BeginTransaction();
                    iType.Transaction = Cmd.Transaction;
                }
                else
                {
                    Cmd.Connection = iType.Transaction.Connection;
                    Cmd.Transaction = iType.Transaction;
                }
            }
            else OpenConnection(iType);
        }
        /// <summary>
        /// Seta parâmetros para procedure
        /// </summary>
        /// <param name="iType">interface do objeto</param>
        /// <param name="pi">proprieade do objeto</param>
        /// <param name="cmd">comando de execução</param>
        protected void setParan(IType iType, PropertyInfo pi, DbCommand cmd)
        {
            DbParameter paran = Cmd.CreateParameter();
            paran.ParameterName = "@" + CType.GetPropertyName(pi);
            if (pi.GetValue(iType, null) == null || pkValue) paran.Value = DBNull.Value;
            else paran.Value = pi.GetValue(iType, null);
            Cmd.Parameters.Add(paran);
        }

        /// <summary>
        /// Seta parâmetros para procedure, usado em delete
        /// </summary>
        /// <param name="iType">interface do objeto</param>
        /// <param name="pi">proprieade do objeto</param>
        /// <param name="cmd">comando de execução</param>
       
        private void setParanDel(IType iType, PropertyInfo pi, DbCommand cmd)
        {
            DbParameter paran = Cmd.CreateParameter();
            paran.ParameterName = "@" + CType.GetPropertyName(pi);
            if (pi.GetValue(iType, null) == null || pkValue) paran.Value = DBNull.Value;
            else
            {
                paran.Value = pi.GetValue(iType, null);

                PropertyAttribute[] pAttProperty = (PropertyAttribute[])pi.GetCustomAttributes(typeof(PropertyAttribute), false);
                if (pAttProperty != null && pAttProperty.Length > 0 && pAttProperty[0].IsPk)
                    pkValue = true;
            }
            Cmd.Parameters.Add(paran);
        }
        /// <summary>
        /// Seta parâmetros para o processo de save
        /// </summary>
        /// <param name="iType">interface do objeto</param>
        protected internal virtual PropertyInfo SetParanSave(IType iType)
        {
            PropertyInfo pInfo = null;
            foreach (PropertyInfo pi in iType.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                OperationsAttribute[] pAttOperation = (OperationsAttribute[])pi.GetCustomAttributes(typeof(OperationsAttribute), false);
                PropertyAttribute[] pAttProperty = (PropertyAttribute[])pi.GetCustomAttributes(typeof(PropertyAttribute), false);
                if (pAttOperation != null && pAttOperation.Length > 0 && pAttOperation[0].UseSave)
                    setParan(iType, pi, Cmd);
                if (pInfo == null && pAttProperty != null && pAttProperty.Length > 0 && pAttProperty[0].IsPk)
                   pInfo = pi;
            }
            return pInfo;
        }
        /// <summary>
        /// Seta parâmetros para o processo de delete
        /// </summary>
        /// <param name="iType">interface do objeto</param>      
        protected internal virtual void SetParanDelete(IType iType)
        {
            foreach (PropertyInfo pi in iType.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                OperationsAttribute[] pAttOperation = (OperationsAttribute[])pi.GetCustomAttributes(typeof(OperationsAttribute), false);
                if (pAttOperation != null && pAttOperation.Length > 0 && pAttOperation[0].UseDelete)
                    setParanDel(iType, pi, Cmd);
            }
        }
        /// <summary>
        /// Seta parâmetros para o processo de Get
        /// </summary>
        /// <param name="iType">interface do objeto</param>      
        protected internal virtual void SetParanGet(IType iType)
        {
            int? id = iType.Id;
            foreach (PropertyInfo pi in iType.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                OperationsAttribute[] pAttOperation = (OperationsAttribute[])pi.GetCustomAttributes(typeof(OperationsAttribute), false);
                PropertyAttribute[] pAttProperty = (PropertyAttribute[])pi.GetCustomAttributes(typeof(PropertyAttribute), false);
                if (pAttOperation != null && pAttOperation.Length > 0 && pAttOperation[0].UseGet)
                {
                    if (id != null)
                    {
                        if (pAttProperty != null && pAttProperty.Length > 0 && pAttProperty[0].IsPk)
                        {
                            setParan(iType, pi, Cmd);
                            return;
                        }
                    }

                    setParan(iType, pi, Cmd);
                }
            }
        }
        /// <summary>
        /// Metodo interno para setar e retorna um objeto pedido através de um DataReader
        /// </summary>
        /// <param name="dr">DataReader de cliente</param>
        /// <param name="objCliente">Referência para objeto Cliente</param>
        public virtual IType SetObject(DbDataReader dr, IType iType)
        {
            foreach (PropertyInfo pi in iType.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                PropertyAttribute[] pAttribute = (PropertyAttribute[])pi.GetCustomAttributes(typeof(PropertyAttribute), false);
                if (pAttribute != null && pAttribute.Length > 0 && (pAttribute[0].IsFieldBase))
                {
                    object value = DBNull.Value;
                    try { value = dr[CType.GetPropertyName(pi)]; }
                    catch { }
                    pi.SetValue(iType, iType.PrepareValueProperty(value, pi.PropertyType), null);
                }
            }
            iType.IsFull = true;
            return iType;
        }

        public virtual IType SetObject(DataRow drow, IType iType)
        {
            foreach (PropertyInfo pi in iType.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                PropertyAttribute[] pAttribute = (PropertyAttribute[])pi.GetCustomAttributes(typeof(PropertyAttribute), false);
                if (pAttribute != null && pAttribute.Length > 0 && (pAttribute[0].IsFieldBase))
                {
                    object value = DBNull.Value;
                    try { value = drow[CType.GetPropertyName(pi)]; }
                    catch { }
                    pi.SetValue(iType, iType.PrepareValueProperty(value, pi.PropertyType), null);
                }
            }
            iType.IsFull = true;
            return iType;
        }

        /// <summary>
        /// Cria tabela no banco de dados
        /// </summary>
        public void CreateTable(IType iType)
        {
            try
            {
                Cmd = DataBaseGeneric.CreateCommand(BaseType);
                Cmd.CommandType = CommandType.Text;

                string tableName = CType.GetTableName(iType);
                string sqlCommand = "IF EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[" + tableName + "]') AND OBJECTPROPERTY(id, N'IsUserTable') = 1)" +
                                    "DROP TABLE " + tableName;
                OpenConnection(iType);
                Cmd.CommandText = sqlCommand;
                Cmd.ExecuteNonQuery();

                string pkField = null;

                List<string> sqlCommandParam = new List<string>();
                sqlCommand = "CREATE TABLE " + tableName + "(";
                foreach (PropertyInfo pi in iType.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    PropertyAttribute[] pAttProperty = (PropertyAttribute[])pi.GetCustomAttributes(typeof(PropertyAttribute), false);
                    if (pAttProperty != null && pAttProperty.Length > 0)
                    {
                        if (pAttProperty[0].IsField)
                            addSqlParam(ref sqlCommandParam, pi, false, true, pAttProperty[0].Size);
                        else if (pAttProperty[0].IsPk)
                        {
                            pkField = CType.GetPropertyName(pi);
                            sqlCommand += CType.GetPropertyName(pi) + " " + GetIntDataType(pi) + "  IDENTITY(1,1) NOT NULL, ";
                        }
                    }
                }

                sqlCommand += string.Join(", ", sqlCommandParam.ToArray());
                sqlCommand = sqlCommand.Replace("@", "");

                sqlCommand += ") ON [PRIMARY]";
                OpenConnection(iType);
                Cmd.CommandText = sqlCommand;
                Cmd.ExecuteNonQuery();
            }
            catch (Exception ex) { throw ex; }
            finally { CloseConnection(iType); }
        }

        /// <summary>
        /// Cria FOREIGN KEYS tabelas envolvidas
        /// </summary>
        public void CreateForeignKeys(IType iType)
        {
            try
            {
                CreatePrimaryKeyIfNotExist(iType);
                var foreignKeys = GetForeignKeys(iType);
                foreignKeys.ForEach(fk => CreateForeignKey(iType, fk));
            }
            catch (Exception ex) { throw ex; }
        }

        public void DeleteForeignKeys(IType iType)
        {
            try
            {
                var foreignKeys = GetForeignKeys(iType);
                foreignKeys.ForEach(fk => DeleteForeignKey(iType, fk));
            }
            catch (Exception ex) { throw ex; }
        }

        private void CreateForeignKey(IType iType, PropertyInfo fk)
        {
            var className = fk.Name.Substring(2, fk.Name.Length - 2);
            var assemblyQualifiedName = "Didox.Business." + className + ", Business";
            var typeForeignKeyClass = Type.GetType(assemblyQualifiedName);
            var iTypeForeignKeyClass = (IType)Activator.CreateInstance(typeForeignKeyClass);
            CreatePrimaryKeyIfNotExist(iTypeForeignKeyClass);
            var pk = GetPk(iTypeForeignKeyClass);
            
            string tableName = CType.GetTableName(iType);
            string tableNameForeignKey = CType.GetTableName(iTypeForeignKeyClass);
            string propertyName = CType.GetPropertyName(pk);
                
            try
            {
                DeleteForeignKey(iType, pk, tableName, tableNameForeignKey);

                Cmd = DataBaseGeneric.CreateCommand(BaseType);
                Cmd.CommandType = CommandType.Text;
                var sqlCommand = "ALTER TABLE " + tableName + " ADD CONSTRAINT fk_" + tableName + "_" + tableNameForeignKey + " FOREIGN KEY (" + propertyName + ") REFERENCES " + tableNameForeignKey + "(" + propertyName + ")";
                Cmd.CommandText = sqlCommand;
                OpenConnection(iType);
                Cmd.ExecuteNonQuery();
            }
            catch (DbException dbError) { }
            catch (Exception err) { throw err; }
            finally { CloseConnection(iType); }
        }

        private void DeleteForeignKey(IType iType, PropertyInfo pk, string tableName, string tableNameForeignKey)
        {
            Cmd = DataBaseGeneric.CreateCommand(BaseType);
            Cmd.CommandType = CommandType.Text;

            if (pk == null) return;
            string sqlCommand = "ALTER TABLE " + tableName + " DROP CONSTRAINT fk_" + tableName + "_" + tableNameForeignKey + "";

            OpenConnection(iType);
            Cmd.CommandText = sqlCommand;
            try
            {
                Cmd.ExecuteNonQuery();
            }
            catch(DbException dbError) { }
            catch (Exception err) { throw err; }
            finally { CloseConnection(iType); }
        }

        private void DeleteForeignKey(IType iType, PropertyInfo fk)
        {
            var className = fk.Name.Substring(2, fk.Name.Length - 2);
            var assemblyQualifiedName = "Didox.Business." + className + ", Business";
            var typeForeignKeyClass = Type.GetType(assemblyQualifiedName);
            var iTypeForeignKeyClass = (IType)Activator.CreateInstance(typeForeignKeyClass);
            CreatePrimaryKeyIfNotExist(iTypeForeignKeyClass);
            var pk = GetPk(iTypeForeignKeyClass);

            string tableName = CType.GetTableName(iType);
            string tableNameForeignKey = CType.GetTableName(iTypeForeignKeyClass);
            string propertyName = CType.GetPropertyName(pk);

            DeleteForeignKey(iType, pk, tableName, tableNameForeignKey);
        }

        private List<PropertyInfo> GetForeignKeys(IType iType)
        {
            List<PropertyInfo> foreignKeys = new List<PropertyInfo>();
            foreach (PropertyInfo pi in iType.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                PropertyAttribute[] pAttProperty = (PropertyAttribute[])pi.GetCustomAttributes(typeof(PropertyAttribute), false);
                if (pAttProperty != null && pAttProperty.Length > 0 && pAttProperty[0].IsForeignKey)
                    foreignKeys.Add(pi);
            }
            return foreignKeys;
        }

        protected internal virtual PropertyInfo GetPk(IType iType)
        {
            foreach (PropertyInfo pi in iType.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                PropertyAttribute[] pAttProperty = (PropertyAttribute[])pi.GetCustomAttributes(typeof(PropertyAttribute), false);
                if (pAttProperty != null && pAttProperty.Length > 0 && pAttProperty[0].IsPk)
                    return pi;
            }
            return null;
        }

        private void CreatePrimaryKeyIfNotExist(IType iType)
        {
            try
            {
                Cmd = DataBaseGeneric.CreateCommand(BaseType);
                Cmd.CommandType = CommandType.Text;
                
                string tableName = CType.GetTableName(iType);
                var pk = GetPk(iType);
                if (pk == null) return;
                string sqlCommand = "ALTER TABLE " + tableName + " ADD CONSTRAINT pk_" + tableName + "_" + CType.GetPropertyName(pk) + " PRIMARY KEY (" + CType.GetPropertyName(pk) + ")";

                OpenConnection(iType);
                Cmd.CommandText = sqlCommand;
                Cmd.ExecuteNonQuery();
            }
            catch (DbException dbError) { }
            catch (Exception err) { throw err; }           
            finally { CloseConnection(iType); }
        }

        private string getConectionStringFromIType(IType iType)
        {
            MemberInfo info = iType.GetType();
            var tableAttributes = info.GetCustomAttributes(typeof(TableAttribute), true);
            if (tableAttributes.Length > 0)
            {
                TableAttribute pAttTable = (TableAttribute)tableAttributes[0];
                if (pAttTable != null)
                {
                    try
                    {
                        if (pAttTable.DynamicConectionString && !string.IsNullOrEmpty(iType.InstanceConectionString))
                            return iType.InstanceConectionString; 
                         else if (pAttTable.ConnectionString != null)
                            return ConfigurationManager.ConnectionStrings[pAttTable.ConnectionString].ConnectionString;
                        

                        return null;
                    }
                    catch (Exception ex) { throw new DidoxFrameworkError(ex.Message); }
                }
            }

            return null;
        }
        
        /// <summary>
        /// Cria procedures no banco de dados
        /// </summary>
        public virtual void CreateViews(IType iType)
        {
            var currentyPath = AppDomain.CurrentDomain.BaseDirectory.Replace("bin\\", "").Replace("Debug\\", "");
            currentyPath = currentyPath + @"..\";
            var viewPath = currentyPath + @"\DataBase\Generics\Views";
            var aspx = "Template.aspx.template";
            var aspxcs = "Template.aspx.cs.template";

            var pathSave = CType.GetViewPath(iType);

            var fileAspx = pathSave + @"\" + iType.GetType().Name + ".aspx";
            var fileAspxcs = pathSave + @"\" + iType.GetType().Name + ".aspx.cs";

            File.Copy(viewPath + @"\" + aspx, fileAspx);
            File.Copy(viewPath + @"\" + aspxcs, fileAspxcs);

            var myFile = new StreamReader(fileAspx);
            string bodyAspx = myFile.ReadToEnd();

            var htmlField = string.Empty;
            var htmlGridField = string.Empty;
            var csField = string.Empty;
            var csFieldGetById = string.Empty;
            var csFieldClear = string.Empty;
            foreach (PropertyInfo pi in iType.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                PropertyAttribute[] pAttrProperty = (PropertyAttribute[])pi.GetCustomAttributes(typeof(PropertyAttribute), false);
                if (pAttrProperty != null && pAttrProperty.Length > 0)
                {
                    if (pAttrProperty[0].IsFieldBase && !pAttrProperty[0].IsPk)
                    {
                        htmlField += "<div class=\"dvRoll\">";
                        htmlField += "  <div class=\"dvColl\">";
					    htmlField += "    " + pi.Name + ":";
				        htmlField += "  </div>";
                        htmlField += "  <div class=\"dvColl\">";
					    htmlField += "    <asp:TextBox ID=\"txt" + pi.Name + "\" runat=\"server\"></asp:TextBox><br />";
				        htmlField += "  </div>";
				        htmlField += "</div>";

                        htmlGridField += "<asp:TemplateField HeaderText=\"" + pi.Name + "\">";
                        htmlGridField += "  <ItemTemplate>";
                        htmlGridField += "      <%# DataBinder.Eval(Container.DataItem, \"" + pi.Name + "\") %>";
                        htmlGridField += "  </ItemTemplate>";
                        htmlGridField += "</asp:TemplateField>";

                        csField += "[Template]." + pi.Name + " = " + getObjectConverted(pi) + ";\n";
                        csFieldGetById += "txt" + pi.Name + ".Text = [Template]." + pi.Name + ".ToString();\n";
                        csFieldClear += "txt" + pi.Name + ".Text = string.Empty;\n";
                    }
                }
            }

            bodyAspx = bodyAspx.Replace("[GridFields]", htmlGridField);
            bodyAspx = bodyAspx.Replace("[EditFields]", htmlField);
            bodyAspx = bodyAspx.Replace("[Template]", iType.GetType().Name);

            myFile.Close();

            var myWrite = new StreamWriter(fileAspx);
            myWrite.Write(bodyAspx);
            myWrite.Close();

            myFile = new StreamReader(fileAspxcs);
            string bodyAspxcs = myFile.ReadToEnd();

            bodyAspxcs = bodyAspxcs.Replace("[SaveClickFields]", csField);
            bodyAspxcs = bodyAspxcs.Replace("[ClearFields]", csFieldClear);
            bodyAspxcs = bodyAspxcs.Replace("[FieldsGetById]", csFieldGetById);
            bodyAspxcs = bodyAspxcs.Replace("[Template]", iType.GetType().Name);
            myFile.Close();

            myWrite = new StreamWriter(fileAspxcs);
            myWrite.Write(bodyAspxcs);
            myWrite.Close();
        }

        private string getObjectConverted(PropertyInfo pi)
        {
            var dataType = pi.PropertyType.Name.ToLower();
            if (dataType == "nullable`1") dataType = GetIntDataType(pi);

            if (dataType == "string")
                return "txt" + pi.Name + ".Text";
            else if (dataType == "int")
                return "int.Parse(txt" + pi.Name + ".Text)";
            else if (dataType == "datetime")
                return "DateTime.Parse(txt" + pi.Name + ".Text)";
            else if (dataType == "int32")
                return "int.Parse(txt" + pi.Name + ".Text)";
            else if (dataType == "int64")
                return "long.Parse(txt" + pi.Name + ".Text)";
            else if (dataType == "double")
                return "double.Parse(txt" + pi.Name + ".Text)";
            else if (dataType == "single")
                return "float.Parse(txt" + pi.Name + ".Text)";
            else if (dataType == "boolean")
                return "bool.Parse(txt" + pi.Name + ".Text)";

            return "txt" + pi.Name + ".Text";
        }

        /// <summary>
        /// Cria procedures no banco de dados
        /// </summary>
        public virtual void CreateProcs(IType iType)
        {
            try
            {
                Cmd = DataBaseGeneric.CreateCommand(BaseType);
                Cmd.CommandType = CommandType.Text;
                OpenConnection(iType);

                string className = iType.GetType().Name;
                string sqlCommand = " IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[sp_SYS_" + className + "_Excluir]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)" +
                                    "DROP PROCEDURE [dbo].[sp_SYS_" + className + "_Excluir] ";                
                Cmd.CommandText = sqlCommand;
                Cmd.ExecuteNonQuery();

                sqlCommand = " IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[sp_SYS_" + className + "_Buscar]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)" +
                                   "DROP PROCEDURE [dbo].[sp_SYS_" + className + "_Buscar] ";
                Cmd.CommandText = sqlCommand;
                Cmd.ExecuteNonQuery();


                sqlCommand = " IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[sp_SYS_" + className + "_BuscarConditions]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)" +
                                   "DROP PROCEDURE [dbo].[sp_SYS_" + className + "_BuscarConditions] ";
                Cmd.CommandText = sqlCommand;
                Cmd.ExecuteNonQuery();                

                sqlCommand = " IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[sp_SYS_" + className + "_Salvar]') AND OBJECTPROPERTY(id,N'IsProcedure') = 1)" +
                                   "DROP PROCEDURE [dbo].[sp_SYS_" + className + "_Salvar] ";
                Cmd.CommandText = sqlCommand;
                Cmd.ExecuteNonQuery();

                string pkField = null;                
                List<string> sqlCommandWhereDel = new List<string>();
                List<string> sqlCommandParamDel = new List<string>();
                List<ParamGetSql> sqlCommandWhereGet = new List<ParamGetSql>();
                List<string> sqlCommandParamGet = new List<string>();
                List<string> sqlCommandSave = new List<string>();
                List<string> sqlCommandParamSave = new List<string>();
                List<string> sqlCommandOrderGet = new List<string>();

                foreach (PropertyInfo pi in iType.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
                {
                    PropertyAttribute[] pAttProperty = (PropertyAttribute[])pi.GetCustomAttributes(typeof(PropertyAttribute), false);
                    if (pAttProperty != null && pAttProperty.Length > 0)
                    {
                        if (pAttProperty[0].IsField)
                        {
                            OperationsAttribute[] pOpeProperty = (OperationsAttribute[])pi.GetCustomAttributes(typeof(OperationsAttribute), false);
                            if (pOpeProperty != null && pOpeProperty.Length > 0)
                            {
                                if (pOpeProperty[0].UseDelete)
                                {
                                    addSqlParam(ref sqlCommandParamDel, pi, true, false, pAttProperty[0].Size);
                                    sqlCommandWhereDel.Add(CType.GetPropertyName(pi));
                                }

                                if (pOpeProperty[0].UseSave)
                                {
                                    addSqlParam(ref sqlCommandParamSave, pi, true, false, pAttProperty[0].Size);
                                    sqlCommandSave.Add(CType.GetPropertyName(pi));
                                }

                                if (pOpeProperty[0].UseGet)
                                {
                                    addSqlParam(ref sqlCommandParamGet, pi, true, false, pAttProperty[0].Size);
                                    sqlCommandWhereGet.Add(
                                        new ParamGetSql(CType.GetPropertyName(pi), (pi.PropertyType.Name == "String" && !pAttProperty[0].DontUseLikeWithStrings && !pAttProperty[0].IsText)));                                    
                                }

                                if (pAttProperty[0].IsOrderField)
                                    sqlCommandOrderGet.Insert(pAttProperty[0].OrderOfPriority, CType.GetPropertyName(pi) + " " + (pAttProperty[0].DescOrder ? "desc" : "asc"));
                            }
                        }
                        else if (pAttProperty[0].IsPk)
                        {
                            pkField = CType.GetPropertyName(pi);
                            string dataType = GetIntDataType(pi);

                            sqlCommandParamDel.Add("@" + pkField + " " + dataType + " = null");
                            sqlCommandWhereDel.Add(CType.GetPropertyName(pi));

                            sqlCommandParamGet.Add("@" + pkField + " " + dataType + " = null");
                            sqlCommandWhereGet.Add(new ParamGetSql(CType.GetPropertyName(pi), false));

                            sqlCommandParamSave.Add("@" + pkField + " " + dataType + " = null");
                        }
                    }
                }

                sqlCommand = "CREATE PROCEDURE sp_SYS_" + className + "_Excluir ";
                sqlCommand += string.Join(", ", sqlCommandParamDel.ToArray()) + " ";
                sqlCommand += "AS BEGIN ";
                sqlCommand += "DELETE FROM " + CType.GetTableName(iType) + " ";
                sqlCommand += "WHERE " + pkField + " IS NOT NULL AND ";
                List<string> sqlCommandSetDel = new List<string>();
                sqlCommandWhereDel.ForEach(c => sqlCommandSetDel.Add("( @" + c + " IS NULL OR " + c + " = @" + c + " )"));
                sqlCommand += string.Join("AND ", sqlCommandSetDel.ToArray()) + " ";
                sqlCommand += "END ";
                Cmd.CommandText = sqlCommand;
                Cmd.ExecuteNonQuery();

                sqlCommand = "CREATE PROCEDURE sp_SYS_" + className + "_Salvar ";
                sqlCommand += string.Join(", ", sqlCommandParamSave.ToArray()) + " ";
                sqlCommand += "AS BEGIN ";
                sqlCommand += "IF(@" + pkField + " is null) ";
                sqlCommand += "BEGIN ";
                sqlCommand += "INSERT INTO " + CType.GetTableName(iType) + " ";
                List<string> fieldsSave = new List<string>();
                sqlCommandSave.ForEach(c => fieldsSave.Add("@" + c));
                sqlCommand += "( " + string.Join(", ", sqlCommandSave.ToArray()) + ", DataCriacao, DataAtualizacao ) Values ( " + string.Join(", ", fieldsSave.ToArray()) + ", getdate(), getdate() ) SELECT @@IDENTITY ";
                sqlCommand += "END ";
                sqlCommand += "ELSE BEGIN ";
                sqlCommand += "UPDATE " + CType.GetTableName(iType) + " SET ";
                List<string> sqlCommandSetSave = new List<string>();
                sqlCommandSave.ForEach(cSave => sqlCommandSetSave.Add(cSave + " = @" + cSave));
                sqlCommand += " " + string.Join(", ", sqlCommandSetSave.ToArray()) + ", DataAtualizacao = getdate() ";
                sqlCommand += "WHERE " + pkField + " = @" + pkField + " ";
                sqlCommand += " END ";
                sqlCommand += " END ";
                Cmd.CommandText = sqlCommand;
                Cmd.ExecuteNonQuery();

                sqlCommand = "CREATE PROCEDURE sp_SYS_" + className + "_Buscar ";
                sqlCommand += string.Join(", ", sqlCommandParamGet.ToArray()) + " ";
                sqlCommand += "AS BEGIN ";
                sqlCommand += "SELECT * FROM " + CType.GetTableName(iType) + " ";
                sqlCommand += "WHERE " + pkField + " IS NOT NULL AND "; // (DataExclusao >= getDate() OR DataExclusao IS NULL) AND
                List<string> sqlCommandSetGet = new List<string>();
                sqlCommandWhereGet.ForEach(c =>
                    sqlCommandSetGet.Add(
                    (c.UseLike ? "( @" + c.SqlParam + " IS NULL OR " + c.SqlParam + " like '%' + @" + c.SqlParam + " + '%' ) " : 
                    "( @" + c.SqlParam + " IS NULL OR " + c.SqlParam + " = @" + c.SqlParam + " ) ")
                    ));
                sqlCommand += string.Join("AND ", sqlCommandSetGet.ToArray()) + " ";
                
                if(sqlCommandOrderGet.Count > 0)
                    sqlCommand += " order by " + string.Join(", ", sqlCommandOrderGet.ToArray()) + " ";

                sqlCommand += "END ";
                Cmd.CommandText = sqlCommand;
                Cmd.ExecuteNonQuery();

                sqlCommand = "CREATE PROCEDURE sp_SYS_" + className + "_BuscarConditions ";
                sqlCommand += "@conditions varchar(2000)";
                sqlCommand += "AS BEGIN ";
                sqlCommand += "DECLARE @SQL varchar(3000) ";
                sqlCommand += "set @SQL = 'SELECT * FROM " + CType.GetTableName(iType) + " ";
                sqlCommand += "WHERE " + pkField + " IS NOT NULL AND ' + @conditions + ' ' ";

                if (sqlCommandOrderGet.Count > 0)
                    sqlCommand += "set @SQL += ' order by " + string.Join(", ", sqlCommandOrderGet.ToArray()) + " ' ";

                sqlCommand += "EXEC(@SQL) ";
                sqlCommand += "END ";
                Cmd.CommandText = sqlCommand;
                Cmd.ExecuteNonQuery();
            }
            catch (Exception ex) { throw ex; }
            finally { CloseConnection(iType); }
        }

        private string GetIntDataType(PropertyInfo pi)
        {
            if (pi.PropertyType.ToString().IndexOf("System.Int64") != -1)
                return "bigint";
            else if (pi.PropertyType.ToString().IndexOf("System.Int32") != -1)
                return "int";
            else if (pi.PropertyType.ToString().IndexOf("System.DateTime") != -1)
                return "datetime";
            
            return "varchar(80)";
        }

        private void addSqlParam(ref List<string> sqlCommandParam, PropertyInfo pi, bool isProcedure, bool haveValidate, int size)
        {
            string field = "";
            ValidateAttribute[] pValProperty = (ValidateAttribute[])pi.GetCustomAttributes(typeof(ValidateAttribute), false);
            PropertyAttribute[] pAttrProperty = (PropertyAttribute[])pi.GetCustomAttributes(typeof(PropertyAttribute), false);
            bool validate = (pValProperty != null && pValProperty.Length > 0);
            bool isText = (pAttrProperty != null && pAttrProperty.Length > 0 && pAttrProperty[0].IsText);
            if (!haveValidate) validate = false;
            
            switch (pi.PropertyType.Name)
            {
                case "String":
                    if (!isText) field = CType.GetPropertyName(pi) + " varchar(" + size + ")" + (validate ? " NOT NULL" : "");
                    else field = CType.GetPropertyName(pi) + " text " + (validate ? " NOT NULL" : "");
                    break;
                case "Nullable`1":
                    field = CType.GetPropertyName(pi) + " " + GetIntDataType(pi) + "" + (validate ? " NOT NULL" : "");
                    break;
                case "Int32":
                    field = CType.GetPropertyName(pi) + " int" + (validate ? " NOT NULL" : "");
                    break;
                case "Int64":
                    field = CType.GetPropertyName(pi) + " bigint" + (validate ? " NOT NULL" : "");
                    break;
                case "Double":
                    field = CType.GetPropertyName(pi) + " decimal(9, 2)" + (validate ? " NOT NULL" : "");
                    break;
                case "Single":
                    field = CType.GetPropertyName(pi) + " float" + (validate ? " NOT NULL" : "");
                    break;
                case "DateTime":
                    field = CType.GetPropertyName(pi) + " datetime" + (validate ? " NOT NULL" : "");
                    break;
                case "Boolean":
                    field = CType.GetPropertyName(pi) + " tinyint" + (validate ? " NOT NULL" : "");
                    break;
                default:
                    field = CType.GetPropertyName(pi) + " varchar(" + size + ")" + (validate ? " NOT NULL" : "");
                    break;
            }

            if (isProcedure) sqlCommandParam.Add("@" + field + " = null");
            else sqlCommandParam.Add(field);
        }

        protected LIType MakeListToGet(IType iType)
        {
            OpenConnectionTrans(iType);
            LIType l = new LIType();
            DbDataReader dr = Cmd.ExecuteReader();
            if (dr != null)
                while (dr.Read())
                    l.Add(SetObject(dr, (IType)Activator.CreateInstance(iType.GetType())));
            dr.Close();
            return l;
        }

        protected void SetObjectToGet(IType iType)
        {
            OpenConnectionTrans(iType);
            DbDataReader dr = Cmd.ExecuteReader();
            if (dr != null && dr.Read()) SetObject(dr, iType);
            else SetNullPk(iType);
            dr.Close();
        }

        /// <summary>
        /// Salva os dados na base através de parâmetros
        /// </summary>
        public virtual void Save(ref IType iType)
        {
            try
            {
                Cmd = DataBaseGeneric.CreateCommand(BaseType);
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.CommandText = "sp_SYS_" + iType.GetType().Name + "_Salvar";
                PropertyInfo cPk = SetParanSave(iType);
                OpenConnectionTrans(iType);
                if (cPk != null && cPk.GetValue(iType, null) == null) 
                    cPk.SetValue(iType, iType.PrepareValueProperty(Cmd.ExecuteScalar(), cPk.PropertyType), null);
                else Cmd.ExecuteNonQuery();
            }
            catch (Exception ex) { throw ex; }
            finally { CloseConnection(iType); }
        }
        /// <summary>
        /// Exclui os dados na base através dos parâmetros
        /// </summary>
        public virtual void Delete(ref IType iType)
        {
            try
            {
                Cmd = DataBaseGeneric.CreateCommand(BaseType);
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.CommandText = "sp_SYS_" + iType.GetType().Name + "_Excluir";
                SetParanDelete(iType);
                OpenConnectionTrans(iType);
                Cmd.ExecuteNonQuery();
            }
            catch (Exception ex) { throw ex; }
            finally { CloseConnection(iType); }
        }

        /// <summary>
        /// Busca os dados da base através dos parâmetros
        /// </summary>
        public virtual LIType Find(ref IType iType)
        {
            try
            {
                Cmd = DataBaseGeneric.CreateCommand(BaseType);
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.CommandText = "sp_SYS_" + iType.GetType().Name + "_Buscar";
                SetParanGet(iType);
                return MakeListToGet(iType);
            }
            catch (Exception ex) { throw ex; }
            finally { CloseConnection(iType); }
        }

        /// <summary>
        /// Busca os dados da base através dos parâmetros por condição
        /// </summary>
        public virtual LIType FindByConditions(ref IType iType, string conditions)
        {
            try
            {
                Cmd = DataBaseGeneric.CreateCommand(BaseType);
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.CommandText = "sp_SYS_" + iType.GetType().Name + "_BuscarConditions";
                DbParameter paran = Cmd.CreateParameter();
                paran.ParameterName = "@conditions";
                paran.Value = conditions;
                Cmd.Parameters.Add(paran);
                return MakeListToGet(iType);
            }
            catch (Exception ex) { throw ex; }
            finally { CloseConnection(iType); }
        }

        /// <summary>
        /// Busca os dados da base através dos parâmetros por SQL conditions
        /// </summary>
        public virtual LIType FindBySql(ref IType iType, string sql)
        {
            try
            {
                Cmd = DataBaseGeneric.CreateCommand(BaseType);
                Cmd.CommandType = CommandType.Text;
                Cmd.CommandText = sql;
                return MakeListToGet(iType);
            }
            catch (Exception ex) { throw ex; }
            finally { CloseConnection(iType); }
        }

        /// <summary>
        /// Executa codigo SQL
        /// </summary>
        public virtual void ExecSql(ref IType iType, string sql)
        {
            try
            {
                Cmd = DataBaseGeneric.CreateCommand(BaseType);
                Cmd.CommandType = CommandType.Text;
                Cmd.CommandText = sql;
                OpenConnectionTrans(iType);
                Cmd.ExecuteNonQuery();
            }
            catch (Exception ex) { throw ex; }
            finally { CloseConnection(iType); }
        }

        /// <summary>
        /// Busca os dados da base e seta a referencia
        /// </summary>
        public virtual void Get(ref IType iType)
        {   
            try
            {
                Cmd = DataBaseGeneric.CreateCommand(BaseType);
                Cmd.CommandType = CommandType.StoredProcedure;
                Cmd.CommandText = "sp_SYS_" + iType.GetType().Name + "_Buscar";
                SetParanGet(iType);
                SetObjectToGet(iType);
            }
            catch (Exception ex) { throw ex; }
            finally { CloseConnection(iType); }
        }

        private void SetNullPk(IType iType)
        {
            foreach (PropertyInfo pi in iType.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic))
            {
                PropertyAttribute[] pAttProperty = (PropertyAttribute[])pi.GetCustomAttributes(typeof(PropertyAttribute), false);
                if (pAttProperty != null && pAttProperty.Length > 0 && pAttProperty[0].IsPk)
                    pi.SetValue(iType, null, null);
            }
        }
	}

    public class ParamGetSql
    {
        public ParamGetSql(string sql, bool useLike)
        {
            SqlParam = sql;
            UseLike = useLike;
        }

        public string SqlParam { get; set; }
        public bool UseLike { get; set; }
    }
}
