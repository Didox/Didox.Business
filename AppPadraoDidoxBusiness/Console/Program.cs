using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Didox.Business;

namespace Console
{
    class Program
    {
        static void Main(string[] args)
        {
            new Telefone().CreateTable();

            new Cargo().Scaffold();
            new TipoTelefone().Scaffold();
            new Telefone().Scaffold();
        }
    }
}
