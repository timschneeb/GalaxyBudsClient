using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;

// Allgemeine Informationen über eine Assembly werden über die folgenden
// Attribute gesteuert. Ändern Sie diese Attributwerte, um die Informationen zu ändern,
// die einer Assembly zugeordnet sind.
[assembly: AssemblyTitle("Galaxy Buds Client")]
[assembly: AssemblyDescription("Unofficial Galaxy Buds Manager")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("ThePBone (Tim Schneeberger)")]
[assembly: AssemblyProduct("Galaxy Buds Client")]
[assembly: AssemblyCopyright("Copyright ©  2020")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Durch Festlegen von ComVisible auf FALSE werden die Typen in dieser Assembly
// für COM-Komponenten unsichtbar.  Wenn Sie auf einen Typ in dieser Assembly von
// COM aus zugreifen müssen, sollten Sie das ComVisible-Attribut für diesen Typ auf "True" festlegen.
[assembly: ComVisible(false)]

//Um mit dem Erstellen lokalisierbarer Anwendungen zu beginnen, legen Sie
//<UICulture>ImCodeVerwendeteKultur</UICulture> in der .csproj-Datei
//in einer <PropertyGroup> fest.  Wenn Sie in den Quelldateien beispielsweise Deutsch
//(Deutschland) verwenden, legen Sie <UICulture> auf \"de-DE\" fest.  Heben Sie dann die Auskommentierung
//des nachstehenden NeutralResourceLanguage-Attributs auf.  Aktualisieren Sie "en-US" in der nachstehenden Zeile,
//sodass es mit der UICulture-Einstellung in der Projektdatei übereinstimmt.

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]


[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //Speicherort der designspezifischen Ressourcenwörterbücher
                                     //(wird verwendet, wenn eine Ressource auf der Seite nicht gefunden wird,
                                     // oder in den Anwendungsressourcen-Wörterbüchern nicht gefunden werden kann.)
    ResourceDictionaryLocation.SourceAssembly //Speicherort des generischen Ressourcenwörterbuchs
                                              //(wird verwendet, wenn eine Ressource auf der Seite nicht gefunden wird,
                                              // designspezifischen Ressourcenwörterbuch nicht gefunden werden kann.)
)]


// Versionsinformationen für eine Assembly bestehen aus den folgenden vier Werten:
//
//      Hauptversion
//      Nebenversion
//      Buildnummer
//      Revision
//
// Sie können alle Werte angeben oder Standardwerte für die Build- und Revisionsnummern verwenden,
// indem Sie "*" wie unten gezeigt eingeben:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("2.3.3.5")]
[assembly: AssemblyFileVersion("2.3.3.5")]
