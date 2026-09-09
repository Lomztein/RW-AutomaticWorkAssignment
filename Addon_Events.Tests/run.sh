#!/bin/sh
set -eu
cd "$(dirname "$0")"
output=$(mktemp -d)
trap 'rm -rf "$output"' EXIT
mcs -langversion:latest -out:"$output/tests.exe" CoroutineRegressionTests.cs ../AutomaticWorkAssignment/Source/Buffer.cs ../Addon_Events/Source/PawnPostProcessors/DoOnConditionChangedPostProcessor.cs ../Addon_Events/Source/PawnPostProcessors/DoRepeatPostProcessor.cs
mono "$output/tests.exe"
