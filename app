#!/bin/bash

# Check if a script name is provided
if [[ -z "$1" ]]; then
    echo "Usage: $0 <service_name> <action_name> [arguments...]"
    echo "Available services:"
    find ./Scripts/Services -maxdepth 1 -type f -exec basename {} \;
    echo ""
    echo ""
    echo "Available actions:"
    find ./Scripts/Actions -maxdepth 1 -type f -exec basename {} \;
    exit 1
fi

# Extract the service name
SERVICE_NAME="$1"
shift # Remove the first argument (service name)
ACTION_NAME="$1"
shift # Remove the second argument (action name)

# Run the target script with the remaining arguments
./Scripts/Actions/"$ACTION_NAME" "$SERVICE_NAME" "$@"