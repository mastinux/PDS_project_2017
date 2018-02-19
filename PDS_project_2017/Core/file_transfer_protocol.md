# File transfer protocol

Single file transfer using TCP connection

|Sender||Receiver|
|-|:-:|-:|
|| --- file name lenght (int) ---> ||
|| --- file name (String) ---> ||
|| <-- "ok " -- ||
||or||
|| <-- "no " -- ||
|| [ --- file content length (long) ---> ] ||
|| [ --- file content (byte[]) ---> ] ||