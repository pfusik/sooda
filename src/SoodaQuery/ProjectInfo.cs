// 
// SoodaQuery - A Sooda database query tool
// 
// Copyright (C) 2003-2004 Jaroslaw Kowalski
// 
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation; either version 2 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
// 
// Jaroslaw Kowalski (jaak@polbox.com)
// 

using System;
using System.Data;

namespace SoodaQuery {
    public class ProjectInfo {
        public string connectionType;
        public string soodaSchema;

        public ProjectInfo() {}

        public IDbConnection CreateConnection() {
            return (IDbConnection)Activator.CreateInstance(Type.GetType(connectionType));
        }
    }
}
