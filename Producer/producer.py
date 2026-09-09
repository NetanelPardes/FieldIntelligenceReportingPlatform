import json
import logging
import os
from logging.handlers import TimedRotatingFileHandler
from pathlib import Path
import sys
from confluent_kafka import KafkaError, KafkaException, Producer
from confluent_kafka.admin import AdminClient, NewTopic
from dotenv import load_dotenv

root_directory = Path(__file__).resolve().parent.parent

load_dotenv(root_directory / ".env")

logs_directory = root_directory / "Logs"
logs_directory.mkdir(parents=True,exist_ok=True)
log_file_path = logs_directory / "producer.log"
logging.basicConfig(level=logging.INFO,format="%(asctime)s [%(levelname)s] %(message)s",datefmt="%Y-%m-%d %H:%M:%S",
    handlers=[logging.StreamHandler(),TimedRotatingFileHandler(filename=log_file_path,when="midnight",interval=1,backupCount=7,encoding="utf-8")])
logger = logging.getLogger("producer")

def create_topic(bootstrap_servers, topic_name):
    admin_client = AdminClient({"bootstrap.servers": bootstrap_servers})
    new_topic = NewTopic(topic=topic_name,num_partitions=1,replication_factor=1)
    topic_futures = admin_client.create_topics([new_topic])
    try:
        topic_futures[topic_name].result()
        logger.info("Topic '%s' created successfully",topic_name)
    except KafkaException as error:
        if error.args[0].code() == KafkaError.TOPIC_ALREADY_EXISTS:
            logger.info("Topic '%s' already exists",topic_name)
        else:
            raise

def load_reports(file_path):
    with open(file_path,"r",encoding="utf-8") as file:
        return json.load(file)

def delivery_report(error, message):
    if error is not None:
        logger.error("Message delivery failed: %s", error)

def send_reports(reports,bootstrap_servers,topic_name):
    producer = Producer({"bootstrap.servers": bootstrap_servers})
    for report in reports:
        message = json.dumps(report,ensure_ascii=False)
        producer.produce(topic=topic_name,key=report["reportId"],value=message,callback=delivery_report)
        producer.poll(0)
    undelivered_messages = producer.flush()
    if undelivered_messages > 0:
        raise RuntimeError(f"{undelivered_messages} messages were not delivered")
    logger.info("Successfully sent %s reports to topic '%s'",len(reports),topic_name)

def main():
    bootstrap_servers = os.getenv("KAFKA_BOOTSTRAP_SERVERS")
    topic_name = os.getenv("REPORTS_TOPIC")
    reports_file_path = os.getenv("REPORTS_FILE_PATH")
    if not bootstrap_servers:
        raise ValueError("KAFKA_BOOTSTRAP_SERVERS is missing from .env")
    if not topic_name:
        raise ValueError("REPORTS_TOPIC is missing from .env")
    if not reports_file_path:
        raise ValueError("REPORTS_FILE_PATH is missing from .env")
    create_topic(bootstrap_servers=bootstrap_servers,topic_name=topic_name)
    full_file_path = (root_directory / reports_file_path)
    reports = load_reports(full_file_path)
    logger.info("Successfully loaded %s reports",len(reports))
    send_reports(reports=reports,bootstrap_servers=bootstrap_servers,topic_name=topic_name)


if __name__ == "__main__":
    try:
        logger.info("Producer application started")
        main()
    except KeyboardInterrupt:
        logger.info("Producer stopped by the user")
    except FileNotFoundError as error:
        logger.error("Reports file was not found: %s",error)
        sys.exit(1)
    except json.JSONDecodeError as error:
        logger.error("Reports file contains invalid JSON: %s",error)
        sys.exit(1)
    except KafkaException as error:
        logger.error("Kafka error: %s",error)
        sys.exit(1)
    except Exception as error:
        logger.error("Unexpected producer error: %s",error)
        sys.exit(1)
    finally:
        logger.info("Producer application finished")