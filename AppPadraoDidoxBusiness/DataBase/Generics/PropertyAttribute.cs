using System;

namespace Didox.DataBase.Generics
{
    /// <summary>
    /// Atributo usado para identificar propriedade, [PropertyAttribute(isPk, isField, isReference, isCollection)]
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class PropertyAttribute : Attribute
    {
        #region Attributes
        private int _size = 80;
        #endregion

        #region Properties

        public bool DontUseLikeWithStrings { get; set; }
        public bool IsText { get; set; }
        public string Name { get; set; }
        public bool IsOrderField { get; set; }
        public int OrderOfPriority { get; set; }
        public bool DescOrder { get; set; }
        public bool IsPk { get; set; }
        public int Size { get { return _size; } set { _size = value; } }
        public bool IsFieldBase { get { return (IsPk || IsField); } }
        public bool IsField { get; set; }
        public bool IsForeignKey { get; set; }
        public bool IsReference { get; set; }
        public bool IsCollection { get; set; }
        public string DefaultValue { get; set; }

        #endregion

        #region Constructor
        public PropertyAttribute() { }
        #endregion
    }
}
