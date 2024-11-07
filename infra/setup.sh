#!/bin/bash

# Start the Docker container in detached mode
echo "Starting the Docker container..."
docker-compose -f ./infra/docker-compose-test.yml up -d

# Confirm the container is running
echo "Checking container status..."
docker-compose -f ./infra/docker-compose-test.yml ps

# Clone the setup repository
echo "Cloning the setup repository..."
git clone --branch main https://github.com/Latitude-OpenDATA-SIO-Saintbe/PythonPopPostgres.git ./db-seed

# Run the setup script to create and seed the database
echo "Running database setup script..."
bash ./db-seed/setup-py.sh

echo "All tasks completed successfully."

echo "you can now ether do dotnet run to view application or you can do dotnet test to test the application" 
