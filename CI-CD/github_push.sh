#!/usr/bin/env bash
set -euo pipefail

if ! command -v git >/dev/null 2>&1; then
  echo "git is required but was not found in PATH"
  exit 1
fi

if ! git rev-parse --is-inside-work-tree >/dev/null 2>&1; then
  echo "current directory is not a git repository"
  exit 1
fi

remote="${GITHUB_REMOTE:-origin}"
branch="$(git rev-parse --abbrev-ref HEAD)"

if ! git remote get-url "$remote" >/dev/null 2>&1; then
  echo "remote '$remote' is not configured"
  exit 1
fi

if [ -n "$(git status --porcelain)" ]; then
  echo "working tree is not clean. commit or stash changes before pushing"
  exit 1
fi

git push "$remote" "$branch"
