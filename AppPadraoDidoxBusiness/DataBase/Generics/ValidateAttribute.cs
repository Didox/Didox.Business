using System;

namespace Didox.DataBase.Generics
{
    /// <summary>
    /// Atributo usado para avisar que o campo é requerido.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class ValidateAttribute : Attribute { }
}
