@echo off
set MODE=check
if not (%1)==() set MODE=%1
cd ..
ufind.exe src/Library -name "*.cs" | xargs tools\LicenseHeader.exe LICENSE.txt %MODE% 
ufind.exe tools -name "*.cs" | xargs tools\LicenseHeader.exe LICENSE.txt %MODE% 
ufind.exe src/Tools/StubGen -name "*.cs" | xargs tools\LicenseHeader.exe LICENSE.txt %MODE% 
ufind.exe src/Tools/SoodaQuery -name "*.cs" | xargs tools\LicenseHeader.exe src/Tools/SoodaQuery/LICENSE.txt %MODE% 
ufind.exe tests -name "*.cs" | xargs tools\LicenseHeader.exe LICENSE.txt %MODE% 

