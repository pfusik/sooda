#!/bin/bash
find src/Library -name '*.cs' | xargs mono tools/LicenseHeader.exe LICENSE.txt $MODE
find tools -name '*.cs' | xargs mono tools/LicenseHeader.exe LICENSE.txt $MODE
find src/Tools/StubGen -name '*.cs' | xargs mono tools/LicenseHeader.exe LICENSE.txt $MODE
find src/Tools/SoodaQuery -name '*.cs' | xargs mono tools/LicenseHeader.exe src/Tools/SoodaQuery/LICENSE.txt $MODE
