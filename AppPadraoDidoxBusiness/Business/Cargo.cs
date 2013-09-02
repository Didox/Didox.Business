using System;
using Didox.DataBase;
using Didox.DataBase.Generics;
using System.Data;
using System.Text;
using System.Configuration;
using System.Collections.Generic;

namespace Didox.Business
{
    #region Class Cargo

    [View(Path = @"C:\Users\didox\Desktop\AppPadraoDidoxBusiness\UI")]
    [Table(Name = "NEG_Cargo")]
    public class Cargo : CType
    {
        #region Construtores
        public Cargo()
        {
        }

        public Cargo(int? idCargo)
        {
            this.IDCargo = idCargo;
        }

        #endregion

        #region Destrutor
        ~Cargo() { Dispose(); }
        #endregion

        #region Attributes
        #endregion

        #region Propriedades

        [Property(IsPk = true)]
        [Operations(UseSave = true, UseDelete = true, UseGet = true)]
        public int? IDCargo { get; set; }

        [Validate]
        [Property(IsField = true)]
        [Operations(UseSave = true, UseGet=true)]
        public string Descricao { get; set; }

        #endregion

        #region "Methods"

        #endregion
    }

    #endregion
}