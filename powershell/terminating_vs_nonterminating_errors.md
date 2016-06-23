# Terminating vs. Non-Terminating Errors

Powershell has two different kinds of errors that have slightly different semantics.

Terminating errors are produced using the `throw` keyword in powershell, or by any exception thrown by regular .NET code.  These can generally be handled the same way as exceptions and caught with try/catch statements, or using the powershell [`trap`](https://technet.microsoft.com/en-us/library/hh847742.aspx) (details of which are not worth going into here). 

Non-terminating errors are generally created using the `Write-Error` cmdlet.  These will get written to the output stream as `ErrorRecord` objects and usually printed in red text just like terminating errors, but the script will continue.  However, if [`$ErrorActionPreference`](https://technet.microsoft.com/en-us/library/hh847796.aspx) is set to `Stop`, all non-terminating errors will be treated as terminating errors.  This includes being caught by `catch` blocks and `trap` statements.
