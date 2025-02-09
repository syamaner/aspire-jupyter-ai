FROM ubuntu:22.04 AS build-native-env
# This stage is only needed for the .NET interactive kernel and OpenCvSharp
ENV DEBIAN_FRONTEND=noninteractive
# 4.7.0: 8 December 2023
# https://github.com/opencv/opencv/releases/tag/4.7.0
ENV OPENCV_VERSION=4.7.0
# 4.5.3.20211228: 13 January 2023
# https://github.com/shimat/opencvsharp/releases/tag/4.7.0.20230114
ENV OPENCVSHARP_VERSION=4.7.0.20230114

WORKDIR /
RUN apt-get update && apt-get -y install --no-install-recommends \
      apt-transport-https \
      software-properties-common \
      wget \
      unzip \
      openssl \
      cmake \
      ca-certificates \
      build-essential \
      git \
      ninja-build \
      libtbb-dev \
      libatlas-base-dev \
      libgtk2.0-dev \
      libavcodec-dev \
      libavformat-dev \
      libswscale-dev \
      libxine2-dev \
      libv4l-dev \
      libtheora-dev \
      libvorbis-dev \
      libxvidcore-dev \
      libopencore-amrnb-dev \
      libopencore-amrwb-dev \
      libopenjp2-7-dev \
      libswresample-dev \
      x264 \
      libtesseract-dev \
      libdc1394-dev \
      libgdiplus 

# Setup OpenCV and opencv-contrib sources using the specified release.
RUN wget https://github.com/opencv/opencv/archive/${OPENCV_VERSION}.zip && \
    unzip ${OPENCV_VERSION}.zip && \
    rm ${OPENCV_VERSION}.zip && \
    mv opencv-${OPENCV_VERSION} opencv
RUN wget https://github.com/opencv/opencv_contrib/archive/${OPENCV_VERSION}.zip && \
    unzip ${OPENCV_VERSION}.zip && \
    rm ${OPENCV_VERSION}.zip && \
    mv opencv_contrib-${OPENCV_VERSION} opencv_contrib
# cmake and build OpenCV optinally specying architecture related cmake options.
RUN if [ "$DOCKER_PLATFORM" = "linux/amd64" ]; then \
        ADDITIONAL_FLAGS=""; \
    elif [ "$DOCKER_PLATFORM" = "linux/arm64" ]; then \
        ADDITIONAL_FLAGS=" -D ENABLE_NEON=ON -D CPU_BASELINE=NEON ";  \
    elif [ "$DOCKER_PLATFORM" = "linux/arm/v7" ]; then \
        ADDITIONAL_FLAGS=" -D CPU_BASELINE=NEON -D ENABLE_NEON=ON "; \
    fi && cd opencv && mkdir build && cd build && \
    cmake $ADDITIONAL_FLAGS \
    -D OPENCV_EXTRA_MODULES_PATH=/opencv_contrib/modules \
    -D CMAKE_BUILD_TYPE=RELEASE \
    -D BUILD_EXAMPLES=OFF \
    -D WITH_TBB=ON \
    -D BUILD_TBB=ON \
    -D WITH_OPENMP=ON \
    -D WITH_QT=OFF \
    -D WITH_V4L=ON \
    -D WITH_EIGEN=ON \
    -D WITH_LIBV4L=ON \
    -D OPENCV_ENABLE_NONFREE=ON \
    -D OPENCV_GENERATE_PKGCONFIG=ON \
    -D BUILD_DOCS=OFF \
    -D BUILD_PERF_TESTS=OFF \
    -D BUILD_TESTS=OFF \
    -D BUILD_opencv_java=OFF \
    -D BUILD_opencv_python=OFF .. \
    && make -j$(nproc) \
    && make install \
    && ldconfig
    
# Download OpenCvSharp to build OpenCvSharpExtern native library
RUN git clone https://github.com/shimat/opencvsharp.git
RUN cd opencvsharp && git fetch --all --tags --prune && git checkout ${OPENCVSHARP_VERSION}

WORKDIR /opencvsharp/src
RUN mkdir /opencvsharp/make \
    && cd /opencvsharp/make \
    && cmake -D CMAKE_INSTALL_PREFIX=/opencvsharp/make /opencvsharp/src \
    && make -j$(nproc) \
    && make install \
    && cp /opencvsharp/make/OpenCvSharpExtern/libOpenCvSharpExtern.so /usr/lib/ \
    && ldconfig

# Copy the library and dependencies to /artifacts (to be used by images consuming this build)
# cpld.sh will copy the library we specify (./libOpenCvSharpExtern.so) and any dependencies
#    to the /artifacts directory. This is useful for sharing the library with other images
#    consuming this build.
WORKDIR /opencvsharp/make/OpenCvSharpExtern
COPY cpld.sh .
RUN chmod +x cpld.sh && \
    mkdir /artifacts && \
    ./cpld.sh ./libOpenCvSharpExtern.so /artifacts/ 
RUN cp ./libOpenCvSharpExtern.so /artifacts/ 

# Publish the artifacts using a clean image
FROM ubuntu:22.04 AS final
# This stage is only needed for the .NET interactive kernel and OpenCvSharp

RUN mkdir /artifacts
COPY --from=build-native-env /artifacts/ /artifacts
WORKDIR /artifacts

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

# Next three lines are only needed for the .NET interactive kernel and OpenCvSharp
COPY --from=final /artifacts/ /usr/lib/ 
RUN apt-get update && apt-get install -y ttf-mscorefonts-installer fontconfig libc6 libc6-dev libgtk2.0-0 libnss3 libatk-bridge2.0-0 libx11-xcb1 libxcb-dri3-0 libdrm-common libgbm1 libasound2 libappindicator3-1 libxrender1 libfreetype6 libfontconfig1 libxshmfence1
RUN ldconfig
 
USER ${NB_UID}

ENTRYPOINT ["/home/jovyan/run.sh"]