set ARGS=%*
nant %ARGS% doc
nant -t:net-2.0 %ARGS% clean release build binary_snapshot installer
nant -t:net-2.0 %ARGS% clean debug build binary_snapshot installer
nant %ARGS% source_snapshot
