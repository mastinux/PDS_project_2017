# File transfer protocol

Single file transfer using TCP connection

**S** sender  
**R** receiver

|S||R||
|-|:-:|-:||
|| --- "FIL" ---> |||
|| or |||
|| --- "DIR" ---> |||
|| --- description lenght (int) ---> |||
|| --- description (String) ---> || FileNode or DirectoryNode|
|| <-- "OK " -- |||
||or|||
|| <-- "NO " -- |||
|| [ --- content (byte[]) ---> ] |||