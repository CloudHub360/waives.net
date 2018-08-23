#!/bin/bash
read -r -d '' script <<-"EOF"
try {
  & build/_init.ps1; build build,test
} catch {
  throw; exit 1
}
EOF

# which dumps stderr (?) to console, which makes this a bit noisy.
# Can it be made quieter?
pwsh=$(which pwsh-preview || which pwsh || which powershell)
$pwsh -Command $script
