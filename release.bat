set ARGS=%*
nant -t:net-1.1 %ARGS% clean release build binary_snapshot
nant -t:net-1.1 %ARGS% clean debug build binary_snapshot
nant -t:net-2.0 %ARGS% clean release build binary_snapshot installer
nant -t:net-2.0 %ARGS% clean debug build binary_snapshot installer
nant %ARGS% source_snapshot
