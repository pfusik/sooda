function OnFinish(selProj, selObj)
{
    var oldSuppressUIValue = true;
    try
    {
        oldSuppressUIValue = dte.SuppressUI;
        var strProjectName = wizard.FindSymbol("PROJECT_NAME");
        var strSafeProjectName = CreateSafeName(strProjectName);
        wizard.AddSymbol("SAFE_PROJECT_NAME", strSafeProjectName);
        SetTargetFullPath(selObj);
        var strProjectPath = wizard.FindSymbol("TARGET_FULLPATH");
        var strTemplatePath	= wizard.FindSymbol("TEMPLATES_PATH");
        wizard.AddSymbol("ITEM_NAME", "SoodaSchema.xml");
        AddDesignerFileToCSharpWebProject(selObj, strProjectName, strProjectPath, "SoodaSchema.xml", true);
        selProj.Properties("PreBuildEvent").Value = "SoodaStubGen.exe --rebuild-if-changed --schema $(ProjectDir)SoodaSchema.xml --output $(ProjectDir) --namespace " + strSafeProjectName;
    }
    catch(e)
    {
        if(e.description.length > 0)
            SetErrorInfo(e);
        return e.number;
    }
    finally
    {
        dte.SuppressUI = oldSuppressUIValue;
    }
}

function SetFileProperties(oFileItem, strFileName)
{
    if(strFileName == "SoodaSchema.xml")
    {
        // oFileItem.Properties("SubType").Value = "Component";
    }
}

