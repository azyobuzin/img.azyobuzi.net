FROM debian:stretch

COPY . /app
WORKDIR /app

RUN set -ex && \
    apt-get update -qy && \
    apt-get install -qy \
        python python-dev python-pip \
        libmariadbclient-dev-compat libmariadbclient18 && \
    pip install -q pipenv && \
    if [ ! -f 'Pipfile.lock' ]; then pipenv lock; fi && \
    pipenv install --system --clear && \
    apt-get autoremove -qy python-dev python-pip libmariadbclient-dev-compat && \
    rm -rf /var/lib/apt/lists/*

ENV GUNICORN_CMD_ARGS="--workers 2 --bind 0.0.0.0:80"
CMD ["/usr/local/bin/gunicorn",  "--pythonpath",  "/app/src", "api:application"]
EXPOSE 80
