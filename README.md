# EVENT SOURCING WITH .NET CORE

## Some notes about this app:

*   Persisted Oriented Repository (based in CommonDomain). It's enough for this demo app.
*   [AggregateSource](https://github.com/yreynhout/AggregateSource) for a collection oriented repository check out the great AggregateSource from @yreynhout
*   To have test results in XML: dotnet test -xml testout.xml
*   To have test results in NUnit format: xsltproc NUnitXml.xslt testout.xml > testoutnunit.xml