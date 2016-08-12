# Enable access to a machine using a DNS Alias

I could document this in it's entirety, but this ServerFault answer is just beyond fantastic: http://serverfault.com/a/23824.

Just pulling out the important scripts for ease of reference:

```
HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\lanmanserver\parameters
    DisableStrictNameChecking = (DWORD) 1
    OptionalNames = (Multi-String) <Host Names> // Optional to allow Net-Bios Browsing

HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Lsa\MSV1_0
    BackConnectionHostNames = (Multi-String) <Host Names>
```
