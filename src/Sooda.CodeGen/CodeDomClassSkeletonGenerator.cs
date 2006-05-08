// 
// Copyright (c) 2003-2006 Jaroslaw Kowalski <jaak@jkowalski.net>
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

using System.CodeDom;
using System.CodeDom.Compiler;

using Sooda.Schema;

namespace Sooda.CodeGen
{
    public class CodeDomClassSkeletonGenerator : CodeDomHelpers
    {
        private ClassInfo classInfo;
        private SoodaProject options;

        public CodeDomClassSkeletonGenerator(ClassInfo ci, SoodaProject options)
        {
            this.classInfo = ci;
            this.options = options;
        }

        public CodeConstructor Constructor_Raw()
        {
            CodeConstructor ctor = new CodeConstructor();

            ctor.Attributes = MemberAttributes.Public;
            ctor.Parameters.Add(new CodeParameterDeclarationExpression("SoodaConstructor", "c"));
            ctor.BaseConstructorArgs.Add(Arg("c"));

            ctor.Statements.Add(new CodeCommentStatement("Do not modify this constructor."));

            return ctor;
        }

        public CodeConstructor Constructor_Inserting(bool useChainedCall)
        {
            CodeConstructor ctor = new CodeConstructor();
            ctor = new CodeConstructor();
            ctor.Attributes = MemberAttributes.Public;
            ctor.Parameters.Add(new CodeParameterDeclarationExpression("SoodaTransaction", "transaction"));
            ctor.BaseConstructorArgs.Add(Arg("transaction"));
            if (useChainedCall)
            {
                ctor.Statements.Add(new CodeCommentStatement(""));
                ctor.Statements.Add(new CodeCommentStatement("TODO: Add construction logic here."));
                ctor.Statements.Add(new CodeCommentStatement(""));
            }
            else
            {
                ctor.Statements.Add(new CodeCommentStatement("Do not modify this constructor."));
                ctor.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "InitObject"));
            }
            return ctor;
        }

        public CodeConstructor Constructor_Inserting2(bool useChainedCall)
        {
            CodeConstructor ctor = new CodeConstructor();
            ctor.Attributes = MemberAttributes.Public;
            if (useChainedCall)
            {
                ctor.ChainedConstructorArgs.Add(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("SoodaTransaction"), "ActiveTransaction"));
            }
            else
            {
                ctor.BaseConstructorArgs.Add(new CodePropertyReferenceExpression(new CodeTypeReferenceExpression("SoodaTransaction"), "ActiveTransaction"));
            }
            ctor.Statements.Add(new CodeCommentStatement("Do not modify this constructor."));
            if (!useChainedCall)
            {
                ctor.Statements.Add(new CodeMethodInvokeExpression(new CodeThisReferenceExpression(), "InitObject"));
            }
            return ctor;
        }

        public CodeMemberMethod Method_InitObject()
        {
            CodeMemberMethod method = new CodeMemberMethod();
            method.Name = "InitObject";
            method.Attributes = MemberAttributes.Private;
            method.Statements.Add(new CodeCommentStatement("TODO: Add construction logic here."));

            return method;
        }
    }
}
