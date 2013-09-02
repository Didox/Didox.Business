using System;
using Didox.DataBase;
using Didox.DataBase.Generics;
using System.Data;
using System.Text;
using System.Configuration;
using System.Collections.Generic;

namespace Didox.Business
{
    #region Class TipoTelefone

    [View(Path = @"C:\Users\didox\Desktop\AppPadraoDidoxBusiness\UI")]    
    [Table(Name = "NEG_TipoTelefone")]
    public class TipoTelefone : CType
    {
        #region Construtores
        public TipoTelefone()
        {
        }

        public TipoTelefone(int? idTipoTelefone)
        {
            this.IDTipoTelefone = idTipoTelefone;
        }

        #endregion

        #region Destrutor
        ~TipoTelefone() { Dispose(); }
        #endregion

        #region Attributes
        #endregion

        #region Propriedades

        [Property(IsPk = true)]
        [Operations(UseSave = true, UseDelete = true, UseGet = true)]
        public int? IDTipoTelefone { get; set; }

        [Validate]
        [Property(IsField = true)]
        [Operations(UseSave = true)]
        public string Nome { get; set; }

        #endregion

        #region "Methods"

        public static TipoTelefone Celular()
        {
            return new TipoTelefone(1);
        }

        public static TipoTelefone Residencial()
        {
            return new TipoTelefone(2);
        }

        public static TipoTelefone Fax()
        {
            return new TipoTelefone(3);
        }

        public static TipoTelefone Comercial()
        {
            return new TipoTelefone(4);
        }

        #endregion
    }

    #endregion
}