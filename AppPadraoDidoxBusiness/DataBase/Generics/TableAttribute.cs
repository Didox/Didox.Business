using System;

namespace Didox.DataBase.Generics
{
    /// <summary>
    /// Atributo usado para setar o nome da classe
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : Attribute {
        #region Properties
        public string Name { get; set; }   
        public string ConnectionString { get; set; }
        public bool DynamicConectionString { get; set; }
        #endregion
    }
}
