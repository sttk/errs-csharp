#!/usr/bin/env bash

#
# Make a development environement of a C# project.
#

readonly DOTNETVER=net10.0
readonly PROJECT=Errs

## Make a new solution
dotnet new solution --name ${PROJECT}

## Make a project of a library
dotnet new classlib --name ${PROJECT} --output ${PROJECT} --framework ${DOTNETVER}
dotnet solution ${PROJECT}.slnx add ${PROJECT}/${PROJECT}.csproj

awk '/<\/PropertyGroup>/{print "    <GenerateDocumentationFile>true</GenerateDocumentationFile>"}1' ${PROJECT}/${PROJECT}.csproj > .tmp
mv .tmp ${PROJECT}/${PROJECT}.csproj

awk '/<\/Project>/{print "  <PropertyGroup Condition=\"\047$(Configuration)\047 == \047Release\047\">\n    <PathMap>$(MSBuildProjectDirectory)=/</PathMap>\n  </PropertyGroup>\n"}1' ${PROJECT}/${PROJECT}.csproj > .tmp
mv .tmp ${PROJECT}/${PROJECT}.csproj

## Make a project for unit tests
dotnet new install xunit.v3.templates
dotnet new xunit3 --name ${PROJECT}.Tests --output ${PROJECT}.Tests --framework ${DOTNETVER}
dotnet solution ${PROJECT}.slnx add ${PROJECT}.Tests/${PROJECT}.Tests.csproj
dotnet add ${PROJECT}.Tests/${PROJECT}.Tests.csproj reference ${PROJECT}/${PROJECT}.csproj

### for Coverage
dotnet add ${PROJECT}.Tests package Microsoft.Testing.Extensions.CodeCoverage
dotnet tool install --global dotnet-reportgenerator-globaltool

## Make a project for native build
dotnet new console --name ${PROJECT}.NativeTests --framework ${DOTNETVER}
dotnet solution ${PROJECT}.slnx add ${PROJECT}.NativeTests/${PROJECT}.NativeTests.csproj
dotnet add ${PROJECT}.NativeTests/${PROJECT}.NativeTests.csproj reference ${PROJECT}/${PROJECT}.csproj
awk '/<\/PropertyGroup>/{print "    <PublishAot>true</PublishAot>"}1' ${PROJECT}.NativeTests/${PROJECT}.NativeTests.csproj > .tmp
mv .tmp ${PROJECT}.NativeTests/${PROJECT}.NativeTests.csproj

## Make a project for benchmark
dotnet new install BenchmarkDotNet.Templates
dotnet new benchmark --name ${PROJECT}.Benchmarks --output ${PROJECT}.Benchmarks --framework ${DOTNETVER}
dotnet solution ${PROJECT}.slnx add ${PROJECT}.Benchmarks/${PROJECT}.Benchmarks.csproj
dotnet add ${PROJECT}.Benchmarks/${PROJECT}.Benchmarks.csproj reference ${PROJECT}/${PROJECT}.csproj

## NOTE: macOS-specific issue:
##
## The following errors:
##
##   ld: library 'ssl' not found
##   ld: library 'brotlienc' not found
##
## are caused by the following `clang` linker options:
##
##   -lssl, -lbrotlienc, -lbrotlidec, -lbrotlicommon
##
## To resolve this issue, set the Homebrew prefix in the `LIBRARY_PATH` environment variable.
##
##    export LIBRARY_PATH="$(brew --prefix)/lib:$LIBRARY_PATH"
##
