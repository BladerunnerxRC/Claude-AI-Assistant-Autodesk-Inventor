#!/usr/bin/env bash
set -euo pipefail

# -----------------------------------------------------------------------------
# check-windows-paths
#
# Fails if any tracked path can't be checked out on Windows. Git on Windows
# aborts the whole checkout on the first bad path ("error: invalid path"),
# leaving the clone empty.
#
# Checks each path component for:
#   - reserved characters  < > : " | ? * \
#   - control characters
#   - a trailing space or dot
#   - reserved device names (CON, PRN, AUX, NUL, COM1-9, LPT1-9), with or
#     without an extension
# and the whole tree for paths that differ only by case.
#
# Usage: run from anywhere inside the repo. Exit 0 = clean, 1 = problems found.
# -----------------------------------------------------------------------------

cd "$(git rev-parse --show-toplevel)"

bad=0

while IFS= read -r -d '' path; do
  IFS=/ read -ra parts <<< "$path"
  for part in "${parts[@]}"; do
    base="${part%%.*}"
    reason=""
    if [[ $part =~ [\<\>:\"\|\?\*\\] ]]; then
      reason='reserved character (< > : " | ? * \)'
    elif [[ $part =~ [[:cntrl:]] ]]; then
      reason='control character'
    elif [[ $part == *[\ .] ]]; then
      reason='ends with a space or dot'
    elif [[ ${base^^} =~ ^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])$ ]]; then
      reason="reserved device name '$base'"
    fi
    if [[ -n $reason ]]; then
      printf '%s: %s\n' "$path" "$reason"
      bad=1
      break
    fi
  done
done < <(git ls-files -z)

while IFS= read -r dup; do
  printf '%s: differs from another path only by case\n' "$dup"
  bad=1
done < <(git ls-files | sort -f | uniq -Di)

if (( bad )); then
  echo "Rename the paths above so the repo can be cloned on Windows." >&2
  exit 1
fi

echo "OK: all tracked paths are Windows-safe."
