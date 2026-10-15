# VS Code development

Open `SoundFingerprinting.code-workspace` at the repository root, or open the repository folder itself. C# Dev Kit loads `src/SoundFingerprinting.sln` automatically.

Install the recommended Microsoft C# and C# Dev Kit extensions and a .NET 10 SDK. The library targets .NET Standard 2.0; the NUnit test project runs on .NET 10. The repository does not pin an SDK, so `dotnet` selects an installed SDK normally.

## Build and test

- **Tasks: Run Build Task** builds the solution in Debug by default.
- **Tasks: Run Task** also offers solution restore and a library-only build.
- **Tasks: Run Test Task** offers the full suite, unit tests only, or a filtered run. Filters accept standard `dotnet test` expressions, such as `FullyQualifiedName~LowPassFilterTest`.
- In the **Testing** view, expand `SoundFingerprinting.Tests` to discover NUnit tests. Use the Run or Debug actions for an individual test, class, or the whole project.

The existing test-project build target copies `TestEnvironment` fixtures into the output directory. Run tests through these tasks or the Testing view so that the project is built first.

## Debug

This solution contains a library and its tests. To debug library code, place a breakpoint and use **Debug Test** in the Testing view or the test's editor gutter. Debugging a test builds both projects and loads the library's local symbols.

The Run and Debug menu includes **C#: Attach to a running .NET process** for debugging a separate application that uses this library. That application must load your matching local library build and symbols for source breakpoints to bind.

## Editor and shared configuration

C# uses the Microsoft language server and formatter. Generated build output is excluded from search and file watching. Theme and keyboard shortcuts follow your user settings.

Recent C# Dev Kit versions expose project information through **C# Project Details**, with project actions in the normal Explorer. Older versions use **Solution Explorer**.

The `.vscode` files and the workspace file use relative paths and can be committed. Keep personal editor preferences and machine-specific paths in user settings.
