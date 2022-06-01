Matia's DLL Map
===============

Register a DLL map of the name AssemblyName.DllConfig.xml.

This library is designed to help map a `DllImport` instruction to a different library using an XML
document of the name of the assembly and the .DllConfig suffix.

An Example Program
------------------

For a program with the assembly name "MyGlfwWrapper":

```dotnetcli
namespace MyGlfwWrapper;

public static class Glfw
{
    [DllImport("glfw", EntryPoint="glfwInit")]
    public static extern int Init();
}

public static class Program
{
    public static void Main()
    {
        if (Glfw.Init() < 0) Console.Error.WriteLine("Failed to init GLFW");
    }
}
```

This program will not work on any system that uses or ships a library that is GLFW but has a
different name. For example, the latest on Linux will be called libglfw3.so but on Windows it will
be GLFW3.dll. With this library, if we instead create the program as:

```dotnetcli
namespace MyGlfwWrapper;

public static class Glfw
{
    [DllImport("glfw", EntryPoint="glfwInit")]
    public static extern int Init();
}

public static class Program
{
    public static void Main()
    {
        DllMapper.Register(Assembly.GetExecutingAssembly());
        if (Glfw.Init() < 0) Console.Error.WriteLine("Failed to init GLFW");
    }
}
```

On the project, we would have a `Windows.MyGlfwWrapper.DllConfig.xml` and a
`Linux.MyGlfwWrapper.DllConfig.xml`.

The `Windows.MyGlfwWrapper.DllConfig.xml` would look like:

```xml
<configuration>
    <dllmap dll="glfw" target="GLFW3.dll" />
</configuration>
```

But the `Linux.MyGlfwWrapper.DllConfig.xml` would look like:

```xml
<configuration>
    <dllmap dll="glfw" target="libglfw3.so.0">
</configuration>
```

During build, due to the nature of the program, we'd want to use the -r (RID) option during build
and, run and/or publish/pack. For the "-r win-x64" option we'd want to Link the
`Windows.MyGlfwWrapper.DllConfig.xml` to the output directory as `MyGlfwWrapper.DllConfig.xml`. But
for the "-r linux-x64" option, we'd want to Link the `Linux.MyGlfwWrapper.DllConfig.xml` to the
output directory as `MyGlfwWrapper.DllConfig.xml`. This will ensure that the Windows build links to
the correct library and so will the Linux build, without having to change the native name of the
libraries.

A project like this would have a structure such as:

```
MyGlfwWrapper
|   README.md
|   LICENSE.md
|   MyGlfwWrapper.csproj
|
|___DllConfigs
|   |   Windows.xml
|   |   Linux.xml
|   |   Mac.xml
|
|___src
|   |   Program.cs
|
|___Native
|   
|   |___win-x64
|   |   |   GLFW3.dll
|   |
|   |___mac
|   |   |   libglfw3.dylib
|   |
|   |___linux-x64
|   |   |   libglfw3.so.0
```

After building this assembly for "-r win-x64", we'd want to see in the output:

```
out
|   GLFW3.dll
|   MyGlfwWrapper.dll
|   MyGlfwWrapper.exe
|   MyGlfwWrapper.DllConfig.xml
```

But for "-r linux-x64" we'd want to see:

```
out
|   libglfw3.so.0
|   MyGlfwWrapper.dll
|   MyGlfwWrapper
|   MyGlfwWrapper.DllConfig.xml
```

And so on. The only differences should be the origin of the XML file and the native library, with
the XML file having the
`<configuration><dllmap dll="stringUsedInDllImport" target="nativeLibraryName.ext"/></configuration>`
structure but changing the target string. Adding more `<dllmap>` lines can be used for other native
libraries so that multiple can be used.
