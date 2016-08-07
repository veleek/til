# Loading Package Resources Files

One of the big things that has changed as part of Windows 8.X/10 is the application package infrastructure.  Where all the files and folders that you deploy with your application are actually located.  And along with that, the method for loading files from a C# application have changed pretty drastically.  The new C# APIs are thin wrappers around the underlying Windows Store application APIs in order to manage the sandboxing of everything effectively.

## Package Install Location

The entire UWP is deployed as a package.  You can get information about the currently executing package using `Windows.ApplicationModel.Package.Current`, and the package location can be retrieved with the `InstalledLocation` property.  When you're debugging, the package location will be something like `$(OutputPath)\AppX`.  

For the most part, resources that you include in your project (i.e. items that you set **Build Action** to *Content* for), will just be directly in this folder.  Any resources that are pulled in as part of another dependency you reference each get their own folder.  For example, for the library **CoreLibrary** you could have a file called `.\AppX\CoreLibrary\Images\thing.png`.

## Resource URIs

Within a UWP, you can access any resources in the package using the `ms-appx://` protocol, but the format of the paths is a little odd.  For the sample resource name defined above, you would use:

    ms-appx:///CoreLibrary/Images/thing.png
    
Note the required triple forward slash (`/`) at the beginning.  The extra slash is actually just part of the resource path, but this MUST be a forward slash, otherwise you will get a `System.UriFormatException: Invalid URI: The hostname could not be parse`.  On the other hand, all of the remaining slashes can be forward or backwards for whatever reason.

See [the StackOverflow question I asked](https://stackoverflow.com/questions/36858020/openstreamforreadasync-fails-with-the-parameter-is-incorrect-from-root) for the original problem that I was experiencing as part of trying to deal with this.

  
