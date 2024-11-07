#!/bin/bash

# Move up one directory level
cd ..

# Start the Docker container in detached mode
echo "Starting the Docker container..."
docker-compose -f ./infra/docker-compose-dev.yml up -d

# Confirm the container is running
echo "Checking container status..."
docker-compose -f ./infra/docker-compose-dev.yml ps

# Clone the setup repository
echo "Cloning the setup repository..."
git clone --branch main https://github.com/Latitude-OpenDATA-SIO-Saintbe/PythonPopPostgres.git /setupDB

# Run the setup script to create and seed the database
echo "Running database setup script..."
bash /setupDB/setup-py.sh

echo "All tasks completed successfully."