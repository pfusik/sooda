@echo off
set MODE=check
if not (%1)==() set MODE=%1
cd ..
ufind.exe src/Sooda -name "*.cs" | xargs tools\LicenseHeader.exe LICENSE.txt %MODE% 
ufind.exe src/Sooda.CodeGen -name "*.cs" | xargs tools\LicenseHeader.exe LICENSE.txt %MODE% 
ufind.exe src/SoodaStubGen -name "*.cs" | xargs tools\LicenseHeader.exe LICENSE.txt %MODE% 
ufind.exe src/SoodaFixKeygen -name "*.cs" | xargs tools\LicenseHeader.exe LICENSE.txt %MODE% 
ufind.exe src/SoodaSchemaTool -name "*.cs" | xargs tools\LicenseHeader.exe LICENSE.txt %MODE% 
ufind.exe src/SoodaCompileStubs -name "*.cs" | xargs tools\LicenseHeader.exe LICENSE.txt %MODE% 
ufind.exe src/Sooda.NAnt.Tasks -name "*.cs" | xargs tools\LicenseHeader.exe LICENSE.txt %MODE% 
ufind.exe src/Sooda.Logging.log4net -name "*.cs" | xargs tools\LicenseHeader.exe LICENSE.txt %MODE% 
ufind.exe src/Sooda.Logging.NLog -name "*.cs" | xargs tools\LicenseHeader.exe LICENSE.txt %MODE% 
ufind.exe tests -name "*.cs" | xargs tools\LicenseHeader.exe LICENSE.txt %MODE% 

