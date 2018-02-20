# File transfer protocol

Single file transfer using TCP connection

|Sender||Receiver||
|-|:-:|-:||
|| --- "FIL" ---> ||single file|
|| or |||
|| --- "DIR" ---> ||directory structure|
|| --- description lenght (int) ---> |||
|| --- description (String) ---> |||
|| <-- "OK " -- |||
||or|||
|| <-- "NO " -- |||
|| [ --- content length (long) ---> ] |||
|| [ --- content (byte[]) ---> ] |||