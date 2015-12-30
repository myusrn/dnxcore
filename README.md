# dnxcore


This repo contains asp.net execution environment core helper types that i bundle into a nuget package for easy consumption and updating.

Using [nuget.org](http://nuget.org/packages/MyUsrn.Dnx.Core/) publicly accessible package feed versus a visual studio team services [vsts], pka visual studio online [vso] 
everyone in account only accessible package feed.

- - -

So far i'm looking to have this package include:

  * a RouteExAttribute to enable use of query string parameter, in addition to oob provided request url, based versioning support

  * an azure redis cache based TokenCache implemenation to facilitate openid connect [oidc] and on-behalf of token caching in web apps running across multiple servers

### examples of using RouteExAttribute
// GET api/values or api/v1.0/values or api/values?api-version=1.0  
[Route("api/v1.0/values"), RouteEx("api/values", "1.0")]  
public IEnumerable<string> Get() { . . . }  
  
// GET api/v2.0/values or api/values?api-version=2.0  
[Route("api/v2.0/values"), RouteEx("api/values", "2.0")]  
public IEnumerable<string> GetV2() { . . . }
  
### examples of using azure redis cache based TokenCache 
tbd    

- - - 

#### solution notes 
continuous integration [ or delivery ] nuget package generation is carried out using vsts nuget package & publish tasks

localhost nuget package generation is carried out using following command:  
nuget pack Core\Core.csproj -IncludeReferencedProjects -Symbols -OutputDirectory %temp%\packages [ -Prop Configuration=$(ConfigurationName) ]  
and for reviewing package output, along with forcing use of symbols package output use following command:  
move /y %temp%\packages\MyUsrn.Dnx.Core.1.0.0.nupkg %temp%\packages\MyUsrn.Dnx.Core.1.0.0.nupkg.zip  

or to enable localhost nuget package dependency update every time you build the following project PostBuildEvent setting:  
set nugetExe=&lt;some path not currently included system path environment variable&gt;\NuGet.exe  
if /i "$(BuildingInsideVisualStudio)" == "true" if /i "$(ConfigurationName)" == "debug" (  
&nbsp;&nbsp;%nugetExe% pack $(ProjectPath) -IncludeReferencedProjects -Symbols -OutputDirectory %temp%\packages  
)  
