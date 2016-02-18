# Formatting the output of Outlook COM objects

I was looking into using Powershell to manage/modify my Outlook rules since the built in rules editor is a pain to work with.  Working with Outlook is a completely different topic, but the basics are easy enough.  To get started:

```powershell
Add-Type -assembly "Microsoft.Office.Interop.Outlook"
$Outlook = New-Object -comobject Outlook.Application
$namespace = $Outlook.GetNameSpace("MAPI")
```
    
Now, with a the namespace you can easily query all of the rules but you get something that's not particularly nice to look at.

```
> $rules = $namespace.DefaultStore.GetRules()
> $rules 

Application    : Microsoft.Office.Interop.Outlook.ApplicationClass
Class          : 115
Session        : Microsoft.Office.Interop.Outlook.NameSpaceClass
Parent         : System.__ComObject
Name           : MS: Account Notifications
ExecutionOrder : 1
RuleType       : 0
Enabled        : True
IsLocalRule    : False
Actions        : System.__ComObject
Conditions     : System.__ComObject
Exceptions     : System.__ComObject
<repeat for every rule>
```
    
Very difficult to get the lay of the land with a format like this.  Now obviously, you could construct a `Select-Object` statement to filter out the properties you care about and the use `Format-Table` to make it easier to scan, but that's not what this is about.

Powershell provides the ability to create a custom output format for any type via [a `.Format.ps1xml` file](https://technet.microsoft.com/en-us/library/hh847831.aspx).  I'm not going to go into the details of structuring the file, you can get all the info you need there.  

First things first, we need the types that we're going to be formatting.  Now grabbing an individual rule object (`($rules | select -First 1).GetType()`) unfortunately only tells us that its a `System.__ComObject` which is probaly not precise enough for our purposes.  Thankfully, Powershell already has quite a bit of built-in magic for dealing with COM objects, so for whatever reason, `$rules | Get-Member` can pull out a more specific type name - `System.__ComObject#{000630cd-0000-0000-c000-000000000046}` in this case.

Using this we can construct `Outlook.Format.ps1xml`.

```xml
<Configuration>
  <ViewDefinitions>
    <View>
      <Name>OutlookRule</Name>
      <ViewSelectedBy>
        <TypeName>System.__ComObject#{000630cd-0000-0000-c000-000000000046}</TypeName>
      </ViewSelectedBy>

      <TableControl>
        <TableHeaders>

          <TableColumnHeader>
            <Label>Enabled</Label>
            <Width>7</Width>
            <Alignment>right</Alignment>
          </TableColumnHeader>

          <TableColumnHeader>
            <Label>Local</Label>
            <Width>5</Width>
            <Alignment>right</Alignment>
          </TableColumnHeader>

          <TableColumnHeader>
            <Label>#</Label>
            <Width>3</Width>
            <Alignment>right</Alignment>
          </TableColumnHeader>

          <TableColumnHeader>
            <Label>Name</Label>
          </TableColumnHeader>

        </TableHeaders>

        <TableRowEntries>
          <TableRowEntry>
            <TableColumnItems>

              <TableColumnItem>
                <PropertyName>Enabled</PropertyName>
              </TableColumnItem>

              <TableColumnItem>
                <PropertyName>IsLocalRule</PropertyName>
              </TableColumnItem>

              <TableColumnItem>
                <PropertyName>ExecutionOrder</PropertyName>
              </TableColumnItem>

              <TableColumnItem>
                <PropertyName>Name</PropertyName>
              </TableColumnItem>

            </TableColumnItems>
          </TableRowEntry>
        </TableRowEntries>
      </TableControl>
    </View>

  </ViewDefinitions>
</Configuration>
```
    
Now load the format data with `Update-FormatData .\Outlook.Format.ps1xml`.  If there are any errors in the format, this'll let you know.  Dump out the same output we had previously, but now it's formatted and super easy to understand, while at the same time not actually modifying any of the objects in the pipeline, making adding additional operations after the fact much easier.

```
> $rules 

Enabled Local   # Name
------- -----   - ----
   True False   1 Account Notifications
   True  True   2 Messages from me to DLs that I'm on
   True  True   3 Messages Only To Me
  False  True   4 Messages To Me
   True False   5 Filter Deployment Mails
   True False   6 Alert Spam
                  .
                  .
                  .
```

I added rules for various other classes as I browsed including folder objects, rule actions.  The full format file is published in [Outlook.Format.ps1xml](modules/Outlook.Format.ps1xml)
