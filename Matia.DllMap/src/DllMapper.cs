using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml.Linq;

namespace Matia.DllMap;

/// <summary>Class that allows to register the XML document with DLL maps.</summary>
public static class DllMapper
{
    /// <summary>Register the current assembly XML document with DLL maps.</summary>
    /// <param name="assembly">The assembly to register.</param>
    /// <remarks>
    ///     The registration expects an XML document of the name AssemblyName.DllConfig.xml
    ///     with the DLL that was expected and the DLL to target. For example:
    ///     <code>
    /// <![CDATA[<]]> configuration <![CDATA[>]]>
    ///     <![CDATA[<]]> dllmap dll="GLFW" target="glfw3" / <![CDATA[>]]>
    ///<![CDATA[<]]> /configuration <![CDATA[>]]>
    ///     </code>
    ///     Will map the order <c>[DllImport("GLFW")]</c> to <c>[DllImport("glfw3")]</c>. This
    ///     can be useful for libraries with different names on different systems. You'd want to
    ///     have for example a Windows.AssemblyName.DllConfig.xml and a
    ///     Linux.AssemblyName.DllConfig.xml and link them into the output directory depending
    ///     on the runtime identifier with the name AssemblyName.DllConfig.xml instead.
    /// </remarks>
    public static void Register(Assembly assembly)
    {
        NativeLibrary.SetDllImportResolver(assembly, MapAndLoad);
    }

    #region Private

    private static bool MapLibraryName(string assemblyLocation, string originalLibName, out string? mappedLibName)
    {
        var xmlPath = Path.Combine(
            Path.GetDirectoryName(assemblyLocation)
                ?? throw new NullReferenceException($"Location '{assemblyLocation}' does not have a valid directory name"),
            Path.GetFileNameWithoutExtension(assemblyLocation) + ".DllConfig.xml");

        mappedLibName = null;

        if (!File.Exists(xmlPath)) return false;

        var root = XElement.Load(xmlPath);
        var map =
            (from element in root.Elements("dllmap")
             where (string?)element.Attribute("dll") == originalLibName
             select element)
                .SingleOrDefault();

        if (map is not null) mappedLibName = map.Attribute("target")?.Value;
        return mappedLibName is not null;
    }

    private static IntPtr MapAndLoad(string libraryName, Assembly assembly, DllImportSearchPath? dllImportSearchPath)
    {
        var name = MapLibraryName(assembly.Location, libraryName, out var mappedName) ? mappedName : libraryName;
        return NativeLibrary.Load(name!, assembly, dllImportSearchPath);
    }

    #endregion Private
}