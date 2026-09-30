#!/usr/bin/env bash

set -euo pipefail

readonly PROJECT="Errs"
readonly CWD="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"

clean() {
  # dotnet clean -c Debug && dotnet clean -c Release  # Not enough
  find "${CWD}" -type d \( -name bin -o -name obj \) -prune -exec rm -rf {} +
}

compile() {
  dotnet build "${PROJECT}/${PROJECT}.csproj"
}

format() {
  dotnet format
}

test() {
  dotnet test "${PROJECT}.Tests/${PROJECT}.Tests.csproj" --results-directory=_site/TestResults
}

cover() {
  dotnet test "${PROJECT}.Tests/${PROJECT}.Tests.csproj" --coverage --coverage-output-format cobertura --results-directory=_site/TestResults
  reportgenerator -reports:_site/TestResults/*.cobertura.xml -targetdir:_site/CoverageReport
}

bench() {
  pushd "${PROJECT}.Benchmarks"
  dotnet run -c Release
  popd
}

deps() {
  dotnet package update
}

doc() {
  docfx docfx.json
}

pack() {
  dotnet pack "${PROJECT}/${PROJECT}.csproj" -c Release
}

native_test() {
  export DOTNET_CLI_UI_LANGUAGE=en
  OS="$(uname -s)"
  ARCH="$(uname -m)"
  case "$OS" in
    Darwin)
      case "$ARCH" in
        x86_64)
          RID="osx-x64"
          ;;
        arm64)
          RID="osx-arm64"
          ;;
        *)
          echo "Unsupported macOS architecture: $ARCH" >&2
          exit 1
          ;;
      esac
      APPLE_MIN_OS_VERSION="$(sw_vers -productVersion | cut -f1-2 -d'.')"
      AOT_OPTIONS=(
        "-p:AppleMinOSVersion=$APPLE_MIN_OS_VERSION"
      )
      ;;
    Linux)
      case "$ARCH" in
        x86_64)
          RID="linux-x64"
          ;;
        aarch64 | arm64)
          RID="linux-arm64"
          ;;
        *)
          echo "Unsupported Linux architecture: $ARCH" >&2
          exit 1
          ;;
      esac
      AOT_OPTIONS=()
      ;;
  esac
  dotnet publish "${PROJECT}.NativeTests/${PROJECT}.NativeTests.csproj" -v diag -c Release -r "$RID" "${AOT_OPTIONS[@]}"
  find "${CWD}/${PROJECT}.NativeTests/bin/Release" -path "*/publish/${PROJECT}.NativeTests" -type f -name "${PROJECT}.NativeTests" -exec {} \;
}

if [[ "$#" == "0" ]]; then
  clean
  format
  compile
  test
  native_test
else
  for a in "$@"; do
    case "$a" in
    clean)
      clean
      ;;
    compile)
      compile
      ;;
    format)
      format
      ;;
    test)
      test
      ;;
    cover)
      cover
      ;;
    bench)
      bench
      ;;
    deps)
      deps
      ;;
    doc)
      doc
      ;;
    pack)
      pack
      ;;
    native-test)
      native_test
      ;;
    *)
      echo "Bad task: $a"
      exit 1
      ;;
    esac
  done
fi
