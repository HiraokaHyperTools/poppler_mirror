#!/bin/bash

dynamicLinks=`cygcheck $1 | tr "\\\\\\\\" "/" | grep "/msys32/" | grep -e "^\s\s\S" | xargs -r basename -a`

if [ -n "$dynamicLinks" ]; then
  echo \"$1\" isn\'t static link: $dynamicLinks >&2
  exit 1
fi
