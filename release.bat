nant source_snapshot
nant -t:net-1.1 clean release build binary_snapshot
nant -t:net-1.1 clean debug build binary_snapshot
nant -t:net-2.0 clean release build binary_snapshot installer
nant -t:net-2.0 clean debug build binary_snapshot installer
