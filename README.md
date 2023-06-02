# helpers
This here repository is purely for my support library, which I use in many projects and extend every time I need something.

Of course, you are free to use it however you want, just know that no support will be offered.

This library consists of these features
- An awful lot of Reflection helpers & extensions
- A networking API based on the TCP protocol using [WatsonTcp](https://github.com/jchristn/WatsonTcp)
- Serializing into pure binary and a binary file reader / writer
- A nice and simple logging system
- Extensions to basic system types (string, collections, random etc.)
- Utilities for executing async method on another thread via callback
- Pooling
- INI format configuration handler

And so on ..

# Dependencies
Unless you want to use literally any JSON feature (like the IO.Binary interface or encryption), you'll be fine as the dependencies are packaged with the assembly (hence those 3 MBs), otherwise you need to get `System.Security.Management` from NuGet.

With that said, I use these libraries:
- [WatsonTcp](https://github.com/jchristn/WatsonTcp)
- [Costura](https://github.com/Fody/Costura)
- [Enums.NET](https://github.com/TylerBrinkley/Enums.NET)
- Fasm.NET
- [Fasterflect](https://github.com/buunguyen/fasterflect)
- [FastGenericNew](https://github.com/Nyrest/FastGenericNew)
- [Fody](https://github.com/Fody/Fody)
- The amazing [Harmony](https://github.com/pardeike/Harmony) library
- [UnitsNet](https://github.com/angularsen/UnitsNet)
- [YamlDotNet](https://github.com/aaubry/YamlDotNet)
