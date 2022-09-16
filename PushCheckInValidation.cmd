REM run from the solution folder!!!
REM docker build . -f CheckInValidation\Server\Dockerfile -t checkinvalidation:latest
docker tag checkinvalidation:latest ghcr.io/minienw/checkinvalidation:latest
docker push ghcr.io/minienw/checkinvalidation:latest