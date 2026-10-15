#!/usr/bin/env bash
# Offline compile check for the Runner scripts using Unity's bundled Roslyn.
set -u
UNITY=/home/ha/Unity/Hub/Editor/6000.6.3f1-x86_64/Editor/Data
PROJ=/home/ha/ha/IUST/game/HW1/HW1
CSC="$UNITY/DotNetSdk/sdk/8.0.318/Roslyn/bincore/csc.dll"
DOTNET="$UNITY/DotNetSdk/dotnet"

REFS=()
for dll in "$UNITY/Managed/UnityEngine.dll" "$UNITY/Managed/UnityEditor.dll"; do
  REFS+=(-r:"$dll")
done
for dll in "$UNITY/Managed/UnityEngine/"*.dll; do
  case "$dll" in *.pdb|*.xml) continue;; esac
  REFS+=(-r:"$dll")
done
for dll in "$PROJ/Library/ScriptAssemblies/Unity.InputSystem.dll" \
           "$PROJ/Library/ScriptAssemblies/UnityEngine.UI.dll" \
           "$PROJ/Library/ScriptAssemblies/Unity.InputSystem.ForUI.dll"; do
  [ -f "$dll" ] && REFS+=(-r:"$dll")
done
REFS+=(-r:"$UNITY/NetStandard/ref/2.1.0/netstandard.dll")

FILES=()
for scope in "$@"; do
  while IFS= read -r f; do FILES+=("$f"); done < <(find "$scope" -name '*.cs' | sort)
done

exec "$DOTNET" exec "$CSC" -nologo -nostdlib -noconfig -target:library -langversion:9.0 \
  -nowarn:0169,0414,0649 \
  "${REFS[@]}" "${FILES[@]}" -out:/tmp/runner-check.dll
