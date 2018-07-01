using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// アセンブリに関する一般情報は以下の属性セットをとおして制御されます。
// アセンブリに関連付けられている情報を変更するには、
// これらの属性値を変更してください。
[assembly: AssemblyTitle("IceMilkTea")]
[assembly: AssemblyDescription("Unity Game Framework")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Sinoa")]
[assembly: AssemblyProduct("IceMilkTea")]
[assembly: AssemblyCopyright("Copyright © Sinoa 2018")]
[assembly: AssemblyTrademark("Sinoa")]
[assembly: AssemblyCulture("")]

// ComVisible を false に設定すると、その型はこのアセンブリ内で COM コンポーネントから 
// 参照不可能になります。COM からこのアセンブリ内の型にアクセスする場合は、 
// その型の ComVisible 属性を true に設定してください。
[assembly: ComVisible(false)]

// このプロジェクトが COM に公開される場合、次の GUID が typelib の ID になります
[assembly: Guid("364eeace-e8ff-4adc-be10-4540b12b35ec")]

// アセンブリのバージョン情報は次の 4 つの値で構成されています:
//
//      メジャー バージョン
//      マイナー バージョン
//      ビルド番号
//      Revision
//
[assembly: AssemblyVersion("0.0.1.*")]
[assembly: AssemblyFileVersion("0.0.1.0")]

// Internal公開
#if DEBUG
[assembly: InternalsVisibleTo("IceMilkTeaEditor")]
#endif