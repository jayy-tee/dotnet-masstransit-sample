FROM rabbitmq:3-management

COPY rabbitmq.conf /etc/rabbitmq
COPY definitions.json /etc/rabbitmq

RUN rabbitmq-plugins enable --offline rabbitmq_shovel && rabbitmq-plugins enable --offline rabbitmq_shovel_management