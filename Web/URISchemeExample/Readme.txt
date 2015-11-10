This program registers a custom URI Scheme with the OS.
The URI accepts one argument. Eg. urischeme:arg
This argument is retrieved in the Main() function.

Followed official Microsoft instructions from here:
https://msdn.microsoft.com/en-us/library/aa767914(v=vs.85).aspx#app_reg
 
Any project that uses this code MUST request administrator access.
To do this you must have an app.manifest file with the following inside:
<requestedExecutionLevel level="requireAdministrator" uiAccess="false" />
    
To create an app.manifest, click the project and go to Add Item.
Scroll down to Application Manifest and select it. It should be straight
forward from there once you open it.

- Jeff