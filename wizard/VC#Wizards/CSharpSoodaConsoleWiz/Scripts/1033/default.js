// (c) Microsoft Corporation. 

function AddReferencesForSooda(oProj)
{
	var refmanager = GetCSharpReferenceManager(oProj);
	var bExpanded = IsReferencesNodeExpanded(oProj)
	refmanager.Add("System");
	refmanager.Add("System.Data");
	refmanager.Add("System.XML");
	refmanager.Add("Sooda");
	if(!bExpanded)
		CollapseReferencesNode(oProj);
}

function OnFinish(selProj, selObj)
{
    var oldSuppressUIValue = true;
    try
    {
        oldSuppressUIValue = dte.SuppressUI;
        var strProjectPath = wizard.FindSymbol("PROJECT_PATH");
        var strProjectName = wizard.FindSymbol("PROJECT_NAME");
        var strSafeProjectName = CreateSafeName(strProjectName);
        wizard.AddSymbol("SAFE_PROJECT_NAME", strSafeProjectName);

        var proj = CreateCSharpProject(strProjectName, strProjectPath, "default.csproj");

        var InfFile = CreateInfFile();
        if (proj)
        {
            AddReferencesForSooda(proj);
            AddFilesToCSharpProject(proj, strProjectName, strProjectPath, InfFile, false);
        }
        proj.Properties("PreBuildEvent").Value = "SoodaStubGen.exe --rebuild-if-changed --schema $(ProjectDir)SoodaSchema.xml --output $(ProjectDir) --namespace " + strSafeProjectName;
        proj.Properties("ApplicationIcon").Value = "App.ico";
        proj.Save();
    }
    catch(e)
    {
        if( e.description.length > 0 )
            SetErrorInfo(e);
        return e.number;
    }
    finally
    {
        dte.SuppressUI = oldSuppressUIValue;
        if( InfFile )
            InfFile.Delete();
    }
}

function GetCSharpTargetName(strName, strProjectName)
{
    var strTarget = strName;

    switch (strName)
    {
        case "readme.txt":
            strTarget = "ReadMe.txt";
        break;
        case "File1.cs":
            strTarget = "Class1.cs";
        break;
        case "assemblyinfo.cs":
            strTarget = "AssemblyInfo.cs";
        break;
    }
    return strTarget; 
}

function DoOpenFile(strName)
{
    var bOpen = false;

    switch (strName)
    {
        case "Class1.cs":
            bOpen = true;
        break;
    }
    return bOpen; 
}

function SetFileProperties(oFileItem, strFileName)
{
    if(strFileName == "File1.cs" || strFileName == "assemblyinfo.cs" || strFileName == "_Stubs.cs")
    {
        oFileItem.Properties("SubType").Value = "Code";
    }
    if(strFileName == "_DBSchema.bin")
    {
        oFileItem.Properties("BuildAction").Value = 3;  // embedded resource
    }
}

