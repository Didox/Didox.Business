using System;

namespace Didox.DataBase.Generics
{
    /// <summary>
    /// Atributo usado para setar o nome da classe
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ViewAttribute : Attribute {
        #region Properties
        public string Path { get; set; }     
        #endregion
    }
}
