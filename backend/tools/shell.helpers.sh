#!/bin/bash

# Colors
if [ -n "$CIRCLECI" ]; then
  c_r=``
  c_g=``
  c_y=``
  c_x=``
else
  c_r=`tput setaf 1`
  c_g=`tput setaf 2`
  c_y=`tput setaf 3`
  c_x=`tput sgr0`
fi

env_or_prompt () {
  if [[ -z "${!2}" ]]; then

    # non-interactive
    if [ -n "$CIRCLECI" ]; then
       # non-interactive
      echo "${c_r}Error: Missing Environment: $1${c_x}";
      exit 1;
    fi

    printf "${c_g}%-20s${c_x} " "$1:"
    if [ -n "$3" ]; then
      printf "${c_y}$3${c_x} "
    fi
    read -r NEW
    if [ -n "$NEW" ]; then
      eval "$2=${NEW}";
    elif [ -n "$3" ]; then
      eval "$2=$3";
    else
      echo "${c_r}Error: Missing Environment: $1${c_x}";
      exit 1;
    fi
  else
    printf "${c_g}%-20s %s${c_x}\n" "$1:" "${!2}"
  fi
}
