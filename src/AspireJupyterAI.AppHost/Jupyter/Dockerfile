FROM jupyter/base-notebook:ubuntu-22.04
ENV PYTHONDONTWRITEBYTECODE=1

USER root

RUN apt-get update && apt-get install software-properties-common cmake build-essential  libc6  -y

RUN add-apt-repository ppa:dotnet/backports \
    && apt-get update \
    && apt-get install -y dotnet-sdk-9.0  libgl1-mesa-dev  libglib2.0-0 \
    && apt-get clean && rm -rf /var/cache/apt/archives /var/lib/apt/lists/*

USER ${NB_UID}

RUN dotnet tool install -g Microsoft.dotnet-interactive
ENV PATH="${PATH}:/home/jovyan/.dotnet/tools"

COPY ./requirements.txt /home/jovyan/requirements.txt
RUN pip install --no-cache-dir -r /home/jovyan/requirements.txt
RUN dotnet interactive jupyter install

USER root
COPY ./run.sh /home/jovyan/run.sh
RUN chmod +x /home/jovyan/run.sh

USER ${NB_UID}

ENTRYPOINT ["/home/jovyan/run.sh"]