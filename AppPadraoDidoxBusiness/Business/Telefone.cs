using System;
using Didox.DataBase;
using Didox.DataBase.Generics;
using System.Data;
using System.Text;
using System.Configuration;
using System.Collections.Generic;

namespace Didox.Business
{
    #region Class Telefone


    [View(Path = @"C:\Users\didox\Desktop\AppPadraoDidoxBusiness\UI")]
    [Table(Name = "NEG_Telefone")]
    public class Telefone : CType
    {
        #region Construtores
        public Telefone()
        {
        }

        public Telefone(int? idTelefone)
        {
            this.IDTelefone = idTelefone;
        }

        #endregion

        #region Destrutor
        ~Telefone() { Dispose(); }
        #endregion

        #region Attributes
        private TipoTelefone tipoTelefone;
        #endregion

        #region Propriedades

        [Property(IsPk = true)]
        [Operations(UseSave = true, UseDelete = true, UseGet = true)]
        public int? IDTelefone { get; set; }

        [Validate]
        [Property(IsField = true, DefaultValue="55")]
        [Operations(UseSave = true)]
        public int? DDI { get; set; }

        [Validate]
        [Property(IsField = true)]
        [Operations(UseSave = true)]
        public int? DDD { get; set; }

        [Validate]
        [Property(IsField = true)]
        [Operations(UseSave = true)]
        public int? Numero { get; set; }

        [Validate]
        [Property(IsField = true, IsForeignKey = true)]
        [Operations(UseSave = true, UseGet=true)]
        public int? IDTipoTelefone { get; set; }
        public TipoTelefone TipoTelefone
        {
            get
            {
                if (this.tipoTelefone == null)
                {
                    this.tipoTelefone = new TipoTelefone();
                    this.tipoTelefone.IDTipoTelefone = this.IDTipoTelefone;
                }

                if (!this.tipoTelefone.IsFull)
                {
                    this.tipoTelefone.Transaction = this.Transaction;
                    this.tipoTelefone.Get();
                }

                return this.tipoTelefone;
            }
            set
            {
                this.tipoTelefone = value;
                if (value != null) this.IDTipoTelefone = value.IDTipoTelefone;
            }
        }

        #endregion

        #region "Methods"
        
        #endregion
    }

    #endregion
}