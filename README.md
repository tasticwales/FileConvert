# FileConvert

Simple demonstration console application for converting from CSV to XML or JSON formats.  Conversion from XML to CSV also included as proof of concept.  Really simle database implementation to showcase repository pattern usage.

.NET Core console application


Usage:
fileconvert /f = [source file] /tojson|toxml|fromxml = true
Example: fileconvert /f='c:\test.csv' /tojson=true

fileconvert /connectionstring = [database connection string] /readcommand = [stored procedure] /tojson|toxml = true
Example: fileconvert /connectionstring='Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=testdb' /readcommand='getcsvlist' /tojson=true
