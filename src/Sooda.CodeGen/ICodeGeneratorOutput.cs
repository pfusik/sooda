using System;

namespace Sooda.CodeGen
{
    public interface ICodeGeneratorOutput
    {
        void Verbose(string s, params object[] p);
        void Info(string s, params object[] p);
        void Warning(string s, params object[] p);
    }
}
