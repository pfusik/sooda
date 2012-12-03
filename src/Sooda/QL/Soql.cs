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

using Sooda.Schema;

namespace Sooda.QL
{
    public class Soql
    {
        public static SoqlParameterLiteralExpression Param(int parameterPos)
        {
            return new SoqlParameterLiteralExpression(parameterPos);
        }

        public static SoqlRawExpression RawQuery(string text)
        {
            return new SoqlRawExpression(text);
        }

        public static SoqlBooleanExpression ClassRestriction(SoqlPathExpression path, SchemaInfo schema, ClassInfo classInfo)
        {
            // returns no additional filter clause for parent (master-parent) class
            if (classInfo.InheritsFromClass == null)
                return null;

            SoqlExpressionCollection literals = new SoqlExpressionCollection();

            foreach (ClassInfo subclass in classInfo.GetSubclassesForSchema(schema))
            {
                if (subclass.SubclassSelectorValue != null)
                {
                    literals.Add(new SoqlLiteralExpression(subclass.SubclassSelectorValue));
                }
            }
            if (classInfo.SubclassSelectorValue != null)
            {
                literals.Add(new SoqlLiteralExpression(classInfo.SubclassSelectorValue));
            }

            // returns false when class is abstract (no SubClassSelectorValue) and there is no subclasses
            if (literals.Count == 0)
                return new SoqlBooleanLiteralExpression(false);

            SoqlBooleanExpression restriction = new SoqlBooleanInExpression(
                new SoqlPathExpression(path, classInfo.SubclassSelectorField.Name),
                literals
            );

            return restriction;
        }

    }
}
