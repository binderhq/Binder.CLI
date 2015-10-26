# Binder.CLI
Command line interface for carrying out functions on the Binder platform

**Requirements**

For Windows: XP or later with .NET 4.0

For OSX: [Mono](http://www.mono-project.com/) 4.0

**Limitations**

1. Only accesses the Production ecosystem (cannot be set to developer mode)
2. Source must be a either the path to a file, or the path to a wildcard spec of some files 
3. Destination must be the path, within the site, of an existing folder
4. Site must be the full hostname of the site


**Installation**

Unzip the files to a folder and either add that folder to your PATH or change to that folder each time.


**Example commands**

    binder upload -site mysite.binder.com.au -source "C:\var\myfiles\*.*" -destination "/My box/myfolder" -username user@myemail.com -password "mypassword"

    binder dir -site mysite.binder.com.au -source "/My box/myfolder" -username user@myemail.com -password "mypassword"


