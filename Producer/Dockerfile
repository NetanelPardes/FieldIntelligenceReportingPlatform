FROM python:3.13-slim

WORKDIR /app

COPY Producer/requirements.txt ./Producer/requirements.txt

RUN pip install --no-cache-dir \
    -r ./Producer/requirements.txt

COPY Producer/ ./Producer/
COPY Data/ ./Data/

CMD ["python", "./Producer/producer.py"]