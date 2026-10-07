using Mono.Cecil;
if (args.Length < 2) { Console.Error.WriteLine("Usage: ImportAudit plugin.dll references-directory [additional-resolver-directories...]"); return 2; }
using var resolver = new DefaultAssemblyResolver();
foreach (var dir in args.Skip(1)) resolver.AddSearchDirectory(Path.GetFullPath(dir));
resolver.AddSearchDirectory(Path.GetDirectoryName(Path.GetFullPath(args[0]))!);
using var module = ModuleDefinition.ReadModule(args[0], new ReaderParameters { AssemblyResolver = resolver });
int checkedMembers = 0, failures = 0;
foreach (var member in module.GetMemberReferences())
{
    var scope = member.DeclaringType.Scope.Name;
    if (!scope.StartsWith("assembly_", StringComparison.Ordinal) && scope != "Assembly-CSharp" && !scope.StartsWith("UnityEngine", StringComparison.Ordinal)) continue;
    checkedMembers++;
    try
    {
        IMemberDefinition? resolved = member switch { MethodReference method => method.Resolve(), FieldReference field => field.Resolve(), _ => null };
        if (resolved == null) throw new InvalidOperationException("member not found");
    }
    catch (Exception e) { failures++; Console.Error.WriteLine($"UNRESOLVED {scope}: {member.FullName}: {e.Message}"); }
}
Console.WriteLine($"Import audit: {checkedMembers} game/Unity members, {failures} unresolved.");
return failures == 0 ? 0 : 1;
