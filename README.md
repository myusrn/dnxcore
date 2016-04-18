# dnxcore


This repo contains a .net x-platform [dnx] core library that i bundle into a nuget package for easy consumption and updating.

Using [nuget.org](http://nuget.org/packages/MyUsrn.Dnx.Core/) publicly accessible package feed versus a visual studio team services [vsts], pka visual studio 
online [vso], everyone in account only accessible package feed.

- - -

So far this package includes:

  * a RouteExAttribute implementation to enable use of query string parameter, in addition to out of the box [oob] provided request url, based web api versioning support

  * an azure redis cache based TokenCache implemenation to facilitate openid connect [oidc] and on-behalf of token caching in web apps running across multiple servers

### examples of using RouteExAttribute
// GET api/values or api/v1.0/values or api/values?api-version=1.0  
[Route("api/v1.0/values"), RouteEx("api/values", "1.0")]  
public IEnumerable&lt;string&gt; Get() { . . . }  
  
// GET api/v2.0/values or api/values?api-version=2.0  
[Route("api/v2.0/values"), RouteEx("api/values", "2.0")]  
public IEnumerable&lt;string&gt; GetV2() { . . . }
  
### examples of using azure redis cache based TokenCache 
var authority = "https://login.microsoftonline.com/myaadtenant.onmicrosoft.com"  
var userId = context.AuthenticationTicket.Identity.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier").Value;  
AuthenticationContext acWebApp = new AuthenticationContext(authority, new AzRedisTokenCache(userId));    
<br />

- - - 

#### solution notes 
continuous integration [ or delivery ] nuget package generation is carried out using vsts hosted build and release management nuget package & publish tasks

localhost nuget package generation is carried out using following command:  
nuget pack Core\Core.csproj -IncludeReferencedProjects -Symbols -OutputDirectory %temp%\packages -Prop Configuration=Release  
and for reviewing package output, along with forcing use of symbols package output use following command:  
move /y %temp%\packages\MyUsrn.Dnx.Core.&lt;version&gt;.nupkg %temp%\packages\MyUsrn.Dnx.Core.&lt;version&gt;.nupkg.zip

or to enable localhost nuget package dependency update every time you build the following project PostBuildEvent setting:  
set nugetExe=&lt;some path not currently included system path environment variable&gt;\NuGet.exe  
if /i "$(BuildingInsideVisualStudio)" == "true" if /i "$(ConfigurationName)" == "debug" (  
&nbsp;&nbsp;%nugetExe% pack $(ProjectPath) -IncludeReferencedProjects -Symbols -OutputDirectory %temp%\packages  
)  

localhost nuget package publishing is carried out using following command:  
nuget setApiKey &lt;nuget.org/symbolsource.org apikey&gt;  
nuget push %temp%\packages\MyUsrn.Dnx.Core.&lt;version&gt;.nupkg [ -Source https://api.nuget.org/v3/index.json ]  
where presence of symbols.nupkg will cause above to also execute nuget push %temp%\packages\MyUsrn.Dnx.Core.&lt;version&gt;.symbols.nupkg [ -Source https://nuget.smbsrc.net/ ]  
where https://nuget.smbsrc.net/ is the feed url for symbolsource.org packages  

or localhost nuget package publishing, to vsts account feeed, is carried out using following command:  
nuget push %temp%\packages\MyUsrn.Dnx.Core.&lt;version&gt;.symbols.nupkg -Source https://&lt;account&gt;.pkgs.visualstudio.com/DefaultCollection/_packaging/&lt;feed&gt;/nuget/v3/index.json -ApiKey VSTS  

for redis cache learning and expermintation see [intro to redis](http://redis.io/topics/data-types-intro) using redis-cli.exe for windows found at 
[MsOpenTech redis for windows](https://github.com/MSOpenTech/redis/) | releases | latest release | downloads | Redis-x64-3.0.500.zip  
