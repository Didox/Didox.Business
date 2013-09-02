using System;

namespace Didox.DataBase.Generics
{
    public class DidoxFrameworkError : Exception
    {
        public DidoxFrameworkError(string message)
        {
            this._message = message;
        }

        private string _message;

        public override string Message
        {
            get { return this._message; }
        }
    }
}
