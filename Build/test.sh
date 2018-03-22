#!/bin/env bash
set -euo pipefail

SCRIPT_ROOT="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

dotnet test -c Release "${SCRIPT_ROOT}/../Test/Flurl.Test/"